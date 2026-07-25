// GAN Smart Timer / GAN Halo Smart Timer (Web Bluetooth) TimerDriver for the
// submission timer. Hand-rolled protocol decoding after the public write-up by
// Andy Fedotov (https://gist.github.com/afedotov/a025fa5796c9c727b04cf98b293a02f6):
// one GATT service, a notify characteristic streaming short state packets, and
// the recorded time carried inside the STOPPED packet. Web Bluetooth only
// exists in Chromium-based browsers (never on iOS), so the driver registers
// unconditionally and reports support via isSupported().
(function () {
    'use strict';

    const TIMER_SERVICE_UUID = '0000fff0-0000-1000-8000-00805f9b34fb';
    const STATE_CHARACTERISTIC_UUID = '0000fff5-0000-1000-8000-00805f9b34fb';
    // Read-only, 16 bytes: currently displayed time + three previous times.
    const STORED_TIME_CHARACTERISTIC_UUID = '0000fff2-0000-1000-8000-00805f9b34fb';

    // Timer state codes carried in byte 3 of a state packet.
    const STATE_GET_SET = 0x01;   // grace delay expired, ready to start
    const STATE_HANDS_OFF = 0x02; // hands removed before the grace delay expired
    const STATE_RUNNING = 0x03;
    const STATE_STOPPED = 0x04;   // includes the recorded time
    const STATE_IDLE = 0x05;      // timer reset
    const STATE_HANDS_ON = 0x06;
    // 0x07 FINISHED follows STOPPED immediately and carries nothing new.

    const RECONNECT_ATTEMPTS = 3;
    const RECONNECT_DELAY_MS = 2000;

    const CHOOSER_CANCELLED_MESSAGE = 'Nie wybrano urządzenia. Kliknij „Połącz” i wybierz timer GAN z listy.';
    const BLUETOOTH_BLOCKED_MESSAGE = 'Przeglądarka zablokowała dostęp do Bluetooth. Zezwól stronie na użycie Bluetooth i spróbuj ponownie.';
    const CONNECT_FAILED_MESSAGE = 'Nie udało się połączyć z timerem. Sprawdź, czy timer jest włączony i w zasięgu, i spróbuj ponownie.';
    const DISCONNECTED_MESSAGE = 'Utracono połączenie z timerem. Próbuję połączyć ponownie…';
    const RECONNECT_FAILED_MESSAGE = 'Nie udało się ponownie połączyć z timerem. Wybierz ponownie metodę wprowadzania albo odśwież stronę.';

    let callbacks = null;
    let device = null;             // page-lifetime: lets re-attach skip the chooser
    let stateCharacteristic = null;
    let storedTimeCharacteristic = null;
    let running = false;           // a solve is in progress on the device
    let attachToken = 0;

    function mapRequestError(error) {
        const name = error && error.name;
        // The chooser rejects with NotFoundError both when the user cancels it
        // and when no matching device exists nearby.
        if (name === 'NotFoundError') return CHOOSER_CANCELLED_MESSAGE;
        if (name === 'NotAllowedError' || name === 'SecurityError') return BLUETOOTH_BLOCKED_MESSAGE;
        return CONNECT_FAILED_MESSAGE;
    }

    function crc16CcittFalse(data, start, end) {
        let crc = 0xFFFF;
        for (let i = start; i < end; i++) {
            crc ^= data.getUint8(i) << 8;
            for (let bit = 0; bit < 8; bit++) {
                crc = (crc & 0x8000) !== 0 ? ((crc << 1) ^ 0x1021) & 0xFFFF : (crc << 1) & 0xFFFF;
            }
        }
        return crc;
    }

    // Packet layout: 0xFE magic, remaining length, 0x01 data prefix, state code,
    // optional 4-byte time, CRC-16/CCITT-FALSE (little-endian) over everything
    // between the length byte and the CRC itself.
    function isValidStatePacket(data) {
        if (!data || data.byteLength < 6 || data.getUint8(0) !== 0xFE) return false;
        const expectedCrc = data.getUint16(data.byteLength - 2, true);
        return expectedCrc === crc16CcittFalse(data, 2, data.byteLength - 2);
    }

    // Time layout: minutes (uint8), seconds (uint8), milliseconds (uint16 LE).
    function readTimeMs(data, offset) {
        const minutes = data.getUint8(offset);
        const seconds = data.getUint8(offset + 1);
        const milliseconds = data.getUint16(offset + 2, true);
        return 60000 * minutes + 1000 * seconds + milliseconds;
    }

    function handleStateNotification(event) {
        handleStatePacket(event.target.value);
    }

    function handleStatePacket(data) {
        if (!callbacks || !isValidStatePacket(data)) return;
        switch (data.getUint8(3)) {
            case STATE_HANDS_ON:
                if (!running) callbacks.onArming();
                break;
            case STATE_GET_SET:
                if (!running) callbacks.onReady();
                break;
            case STATE_HANDS_OFF:
                if (!running) callbacks.onIdle();
                break;
            case STATE_RUNNING:
                if (running) break;
                running = true;
                // Walk the core through its guarded phases so a start is
                // honoured even if the HANDS_ON/GET_SET notifications were
                // missed. The driver sends no ticks: the core interpolates the
                // running display locally and the recorded result comes
                // exclusively from the STOPPED packet below.
                callbacks.onArming();
                callbacks.onReady();
                callbacks.onStart();
                break;
            case STATE_STOPPED:
                // A stop with no solve in progress (e.g. the timer was running
                // before the page connected) carries no attempt — ignore it.
                if (!running || data.byteLength < 10) break;
                running = false;
                callbacks.onStop(readTimeMs(data, 4));
                break;
            case STATE_IDLE:
                if (running) {
                    // Reset pressed mid-solve — the attempt is discarded.
                    running = false;
                    callbacks.onAbort();
                } else {
                    callbacks.onIdle();
                }
                break;
        }
    }

    async function openConnection() {
        const server = await device.gatt.connect();
        const service = await server.getPrimaryService(TIMER_SERVICE_UUID);
        stateCharacteristic = await service.getCharacteristic(STATE_CHARACTERISTIC_UUID);
        stateCharacteristic.addEventListener('characteristicvaluechanged', handleStateNotification);
        await stateCharacteristic.startNotifications();
        try {
            storedTimeCharacteristic = await service.getCharacteristic(STORED_TIME_CHARACTERISTIC_UUID);
        } catch (error) {
            storedTimeCharacteristic = null; // model without stored times — dirty detection is off
        }
    }

    // The state characteristic only notifies on transitions, so right after a
    // (re)connection a timer already showing a leftover time — or already held —
    // would silently pass for a fresh 0.00. Read what the protocol allows: the
    // stored-time characteristic carries the currently displayed time (dirty
    // detection), and a readValue() on the state characteristic is attempted
    // too even though the spec marks it notify-only — harmless when rejected.
    async function reportInitialState(token) {
        if (storedTimeCharacteristic) {
            try {
                const data = await storedTimeCharacteristic.readValue();
                if (token !== attachToken || !callbacks) return;
                const displayedMs = data.byteLength >= 4 ? readTimeMs(data, 0) : 0;
                if (!running && displayedMs > 0 && callbacks.onDirty) callbacks.onDirty(displayedMs);
            } catch (error) {
                // Read failed — keep the clean 0.00 assumption.
            }
        }
        if (!stateCharacteristic) return;
        try {
            const data = await stateCharacteristic.readValue();
            if (token === attachToken) handleStatePacket(data);
        } catch (error) {
            // Notify-only firmware — hands-on arrives with the next notification.
        }
    }

    function delay(ms) {
        return new Promise(resolve => setTimeout(resolve, ms));
    }

    async function attemptReconnect(token) {
        for (let attempt = 0; attempt < RECONNECT_ATTEMPTS; attempt++) {
            await delay(RECONNECT_DELAY_MS);
            if (token !== attachToken || !callbacks) return;
            try {
                await openConnection();
                // Recovered — the error banner clears on the next arming.
                reportInitialState(token);
                return;
            } catch (error) {
                // Try again until the attempts run out.
            }
        }
        if (token === attachToken && callbacks) callbacks.onError(RECONNECT_FAILED_MESSAGE);
    }

    function handleUnexpectedDisconnect() {
        const wasRunning = running;
        running = false;
        stateCharacteristic = null;
        storedTimeCharacteristic = null;
        if (!callbacks) return; // deliberate detach() disconnect — already handled
        if (wasRunning) callbacks.onAbort();
        callbacks.onError(DISCONNECTED_MESSAGE);
        attemptReconnect(attachToken);
    }

    function adoptDevice(chosen) {
        // The chooser can hand back the very same device object; keep its
        // listener rather than tearing down a connection just re-established.
        if (device === chosen) return;
        if (device) {
            device.removeEventListener('gattserverdisconnected', handleUnexpectedDisconnect);
            if (device.gatt.connected) device.gatt.disconnect();
        }
        stateCharacteristic = null;
        storedTimeCharacteristic = null;
        device = chosen;
        device.addEventListener('gattserverdisconnected', handleUnexpectedDisconnect);
    }

    // Re-attach after a detach (which drops GATT): reconnect to the remembered
    // device without showing the chooser again — gatt.connect() needs no gesture.
    async function ensureConnected(token) {
        if (!device) return;
        if (device.gatt.connected && stateCharacteristic) {
            if (callbacks && callbacks.onConnected) callbacks.onConnected();
            reportInitialState(token);
            return;
        }
        try {
            await openConnection();
            if (token !== attachToken) return;
            if (callbacks && callbacks.onConnected) callbacks.onConnected();
            reportInitialState(token);
        } catch (error) {
            if (token === attachToken && callbacks) callbacks.onError(RECONNECT_FAILED_MESSAGE);
        }
    }

    (window.BldTimerDrivers = window.BldTimerDrivers || []).push({
        id: 'gan-bluetooth',
        label: 'Timer Bluetooth (GAN)',
        isSupported: () => !!navigator.bluetooth && window.isSecureContext,
        unsupportedReason: 'wymaga przeglądarki z Web Bluetooth (np. Chrome lub Edge; niedostępne na iOS) i połączenia HTTPS',
        requiresConnect: true,
        hints: {
            connect: 'Kliknij „Połącz”, aby wybrać timer GAN.',
            connecting: 'Trwa łączenie z timerem…',
            dirty: 'Zresetuj timer przyciskiem z logo GAN, aby rozpocząć próbę.',
            idle: 'Połóż ręce na timerze, aby przygotować start.',
            arming: 'Trzymaj ręce na timerze…',
            ready: 'Puść ręce, aby wystartować.',
            running: 'Zatrzymaj timer dłońmi, aby zapisać czas.',
            review: 'Oznacz DNF lub +2, przejdź dalej przyciskiem albo zresetuj timer.'
        },
        async connect() {
            // Runs from the user's click on „Połącz” — Web Bluetooth requires
            // requestDevice() to be called from a user gesture. Throws a Polish
            // message on failure; the core shows it and re-renders the prompt.
            let chosen;
            try {
                chosen = await navigator.bluetooth.requestDevice({
                    filters: [
                        { namePrefix: 'GAN' },
                        { namePrefix: 'gan' },
                        { namePrefix: 'Gan' }
                    ],
                    optionalServices: [TIMER_SERVICE_UUID]
                });
            } catch (error) {
                throw new Error(mapRequestError(error));
            }
            adoptDevice(chosen);
            try {
                await openConnection();
            } catch (error) {
                throw new Error(CONNECT_FAILED_MESSAGE);
            }
        },
        async disconnect() { },
        attach(cb) {
            callbacks = cb;
            running = false;
            ensureConnected(++attachToken);
        },
        detach() {
            attachToken++;
            running = false;
            callbacks = null;
            if (stateCharacteristic) {
                stateCharacteristic.removeEventListener('characteristicvaluechanged', handleStateNotification);
                stateCharacteristic = null;
            }
            storedTimeCharacteristic = null;
            // Deliberate disconnect: callbacks is already null, so the
            // gattserverdisconnected handler treats it as expected.
            if (device && device.gatt.connected) device.gatt.disconnect();
        }
    });
})();
