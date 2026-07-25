// Stackmat (audio jack) TimerDriver for the submission timer.
// Decodes the signal a Stackmat/SpeedStacks timer streams over the 3.5mm jack
// using the vendored stackmat library (wwwroot/lib/stackmat/). If that library
// failed to load, this file registers nothing and the other drivers still work.
(function () {
    'use strict';

    if (typeof window.Stackmat !== 'function') return;

    const DEVICE_STORAGE_KEY = 'bld.stackmatDeviceId';
    const NO_SIGNAL_TIMEOUT_MS = 3000;

    const PERMISSION_DENIED_MESSAGE = 'Brak dostępu do mikrofonu. Zezwól stronie na użycie mikrofonu i spróbuj ponownie.';
    const NO_DEVICES_MESSAGE = 'Nie znaleziono żadnego wejścia audio. Podłącz timer do wejścia mikrofonowego i spróbuj ponownie.';
    const NO_SIGNAL_MESSAGE = 'Brak sygnału z timera. Sprawdź, czy wybrane jest właściwe wejście audio i czy timer jest podłączony i włączony.';
    const STREAM_FAILED_MESSAGE = 'Nie udało się otworzyć wejścia audio.';

    const RUNNING_STATUS = ' '; // PacketStatus.RUNNING in the stackmat library

    let callbacks = null;
    let stackmat = null;
    let stream = null;
    let running = false;
    let signalTimeout = null;
    let attachToken = 0;
    let deviceRow = null;
    let deviceSelect = null;

    function selectedDeviceId() {
        if (deviceSelect && deviceSelect.value) return deviceSelect.value;
        return localStorage.getItem(DEVICE_STORAGE_KEY);
    }

    function mapStreamError(error) {
        const name = error && error.name;
        if (name === 'NotAllowedError' || name === 'SecurityError') return PERMISSION_DENIED_MESSAGE;
        if (name === 'NotFoundError') return NO_DEVICES_MESSAGE;
        return STREAM_FAILED_MESSAGE;
    }

    async function requestStream(deviceId) {
        const audio = {
            echoCancellation: false,
            noiseSuppression: false,
            autoGainControl: false
        };
        if (deviceId) audio.deviceId = { exact: deviceId };
        try {
            return await navigator.mediaDevices.getUserMedia({ audio: audio });
        } catch (error) {
            if (deviceId && (error.name === 'OverconstrainedError' || error.name === 'NotFoundError')) {
                // The remembered device is gone — fall back to the default input.
                localStorage.removeItem(DEVICE_STORAGE_KEY);
                return requestStream(null);
            }
            throw new Error(mapStreamError(error));
        }
    }

    function stopTracks(mediaStream) {
        mediaStream.getTracks().forEach(track => track.stop());
    }

    function releaseStream() {
        if (stream) {
            stopTracks(stream);
            stream = null;
        }
    }

    function clearSignalWatchdog() {
        if (signalTimeout !== null) {
            clearTimeout(signalTimeout);
            signalTimeout = null;
        }
    }

    function startSignalWatchdog() {
        clearSignalWatchdog();
        signalTimeout = setTimeout(function () {
            signalTimeout = null;
            if (callbacks) callbacks.onError(NO_SIGNAL_MESSAGE);
        }, NO_SIGNAL_TIMEOUT_MS);
    }

    function handleHandsChanged(packet) {
        if (!callbacks || running || packet.timeInMilliseconds !== 0) return;
        if (packet.isLeftHandDown || packet.isRightHandDown) callbacks.onArming();
        else callbacks.onIdle();
    }

    function ensureStackmat() {
        if (stackmat) return;
        // A single shared instance for the whole page: the library's event
        // manager starts an interval it never clears, so one per attach would leak.
        stackmat = new window.Stackmat();
        stackmat.on('packetReceived', function (packet) {
            clearSignalWatchdog();
            if (!callbacks || !running) return;
            if (packet.status === RUNNING_STATUS) callbacks.onTick(packet.timeInMilliseconds);
        });
        stackmat.on('leftHandDown', handleHandsChanged);
        stackmat.on('rightHandDown', handleHandsChanged);
        stackmat.on('leftHandUp', handleHandsChanged);
        stackmat.on('rightHandUp', handleHandsChanged);
        stackmat.on('starting', function () {
            if (!callbacks || running) return;
            callbacks.onReady();
        });
        stackmat.on('started', function () {
            if (!callbacks) return;
            running = true;
            // Walk the core through its guarded phases so a start is honoured
            // even when no arming/green-light packets were observed beforehand.
            callbacks.onArming();
            callbacks.onReady();
            callbacks.onStart();
        });
        stackmat.on('stopped', function (packet) {
            if (!callbacks || !running) return;
            running = false;
            callbacks.onStop(packet.timeInMilliseconds);
        });
        stackmat.on('reset', function () {
            if (!callbacks) return;
            running = false;
            callbacks.onIdle();
        });
    }

    function startDecoder() {
        ensureStackmat();
        // The library hardcodes its own getUserMedia constraints (it ignores
        // deviceId and leaves autoGainControl on) and swallows errors, so hand
        // it the stream acquired above by substituting getUserMedia for the
        // synchronous duration of start().
        const mediaDevices = navigator.mediaDevices;
        const originalGetUserMedia = mediaDevices.getUserMedia;
        mediaDevices.getUserMedia = () => Promise.resolve(stream);
        try {
            stackmat.start();
        } finally {
            mediaDevices.getUserMedia = originalGetUserMedia;
        }
        startSignalWatchdog();
    }

    function stopDecoder() {
        clearSignalWatchdog();
        running = false;
        if (stackmat) stackmat.stop();
        releaseStream();
    }

    async function populateDeviceSelect() {
        if (!deviceSelect) return;
        const devices = await navigator.mediaDevices.enumerateDevices();
        const inputs = devices.filter(device => device.kind === 'audioinput');
        deviceSelect.innerHTML = '';
        inputs.forEach((device, i) => {
            const option = document.createElement('option');
            option.value = device.deviceId;
            option.textContent = device.label || `Wejście audio ${i + 1}`;
            deviceSelect.appendChild(option);
        });
        const storedId = localStorage.getItem(DEVICE_STORAGE_KEY);
        const activeTrack = stream ? stream.getAudioTracks()[0] : null;
        const activeId = activeTrack && activeTrack.getSettings ? activeTrack.getSettings().deviceId : null;
        if (storedId && inputs.some(device => device.deviceId === storedId)) deviceSelect.value = storedId;
        else if (activeId) deviceSelect.value = activeId;
    }

    async function startSession(token) {
        let sessionStream = stream; // stream pre-acquired by connect(), if any
        if (!sessionStream) {
            try {
                sessionStream = await requestStream(selectedDeviceId());
            } catch (error) {
                if (token === attachToken && callbacks) callbacks.onError(error.message);
                return;
            }
        }
        if (token !== attachToken || !callbacks) {
            stopTracks(sessionStream);
            if (stream === sessionStream) stream = null;
            return;
        }
        stream = sessionStream;
        try {
            await populateDeviceSelect();
        } catch (error) {
            // The device list is best-effort; decoding works without it.
        }
        if (token !== attachToken || !callbacks) {
            stopTracks(sessionStream);
            if (stream === sessionStream) stream = null;
            return;
        }
        startDecoder();
    }

    function handleDeviceChange() {
        localStorage.setItem(DEVICE_STORAGE_KEY, deviceSelect.value);
        if (!callbacks) return;
        if (running) {
            running = false;
            callbacks.onAbort();
        }
        stopDecoder();
        startSession(++attachToken);
    }

    (window.BldTimerDrivers = window.BldTimerDrivers || []).push({
        id: 'stackmat',
        label: 'Stackmat (jack audio)',
        isSupported: () => !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.isSecureContext),
        unsupportedReason: 'wymaga bezpiecznego połączenia (HTTPS) i dostępu do mikrofonu',
        requiresConnect: true,
        async connect() {
            // Runs from the user's click on „Połącz”, so the permission prompt
            // is tied to a user gesture. Throws a Polish message on failure.
            stream = await requestStream(selectedDeviceId());
        },
        async disconnect() { },
        attach(cb) {
            callbacks = cb;
            running = false;
            deviceRow = document.getElementById('stackmat-device-row');
            deviceSelect = document.getElementById('stackmat-device-select');
            if (deviceSelect) deviceSelect.addEventListener('change', handleDeviceChange);
            if (deviceRow) deviceRow.classList.replace('d-none', 'd-flex');
            startSession(++attachToken);
        },
        detach() {
            attachToken++;
            stopDecoder();
            if (deviceSelect) deviceSelect.removeEventListener('change', handleDeviceChange);
            if (deviceRow) deviceRow.classList.replace('d-flex', 'd-none');
            callbacks = null;
            deviceRow = null;
            deviceSelect = null;
        }
    });
})();
