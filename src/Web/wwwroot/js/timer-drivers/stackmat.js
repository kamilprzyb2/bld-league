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
    let decoders = null;        // page-lifetime pair, see ensureDecoders()
    let activePolarity = null;  // 'normal' | 'inverted', picked by the first valid packet
    let inverterContext = null; // per-session context producing the inverted stream
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

    async function disableProcessing(mediaStream) {
        // Firefox can hand back a track with auto gain and echo cancellation
        // still enabled despite the constraints passed to getUserMedia; both
        // mangle the 1200 baud timer signal, so ask once more on the live track.
        const track = mediaStream.getAudioTracks()[0];
        if (!track || !track.applyConstraints) return;
        try {
            await track.applyConstraints({
                echoCancellation: false,
                noiseSuppression: false,
                autoGainControl: false
            });
        } catch (error) {
            // Best effort — decoding may still work with processing enabled.
        }
    }

    async function requestStream(deviceId) {
        const audio = {
            echoCancellation: false,
            noiseSuppression: false,
            autoGainControl: false
        };
        if (deviceId) audio.deviceId = { exact: deviceId };
        try {
            const acquired = await navigator.mediaDevices.getUserMedia({ audio: audio });
            await disableProcessing(acquired);
            return acquired;
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

    function isActivePolarity(polarity) {
        return activePolarity === null || activePolarity === polarity;
    }

    function guard(polarity, handler) {
        return function (packet) {
            if (!isActivePolarity(polarity)) return;
            handler(packet);
        };
    }

    function ensureDecoders() {
        if (decoders) return;
        // Two shared instances for the whole page: the library's event manager
        // starts an interval it never clears, so per-attach instances would leak.
        // Two are needed because the electrical polarity of the jack signal
        // varies by sound card and the library's decoder only understands one
        // polarity: each instance is fed one orientation of the same signal and
        // the first valid packet picks the winner (packets carry a checksum, so
        // the wrongly oriented instance never produces any).
        decoders = { normal: createDecoder('normal'), inverted: createDecoder('inverted') };
    }

    function createDecoder(polarity) {
        const stackmat = new window.Stackmat();
        stackmat.on('packetReceived', function (packet) {
            if (!isActivePolarity(polarity)) return;
            if (activePolarity === null) {
                activePolarity = polarity;
                const loser = polarity === 'normal' ? decoders.inverted : decoders.normal;
                loser.stop();
                if (polarity === 'normal') closeInverter();
            }
            clearSignalWatchdog();
            if (!callbacks || !running) return;
            if (packet.status === RUNNING_STATUS) callbacks.onTick(packet.timeInMilliseconds);
        });
        stackmat.on('leftHandDown', guard(polarity, handleHandsChanged));
        stackmat.on('rightHandDown', guard(polarity, handleHandsChanged));
        stackmat.on('leftHandUp', guard(polarity, handleHandsChanged));
        stackmat.on('rightHandUp', guard(polarity, handleHandsChanged));
        stackmat.on('starting', guard(polarity, function () {
            if (!callbacks || running) return;
            callbacks.onReady();
        }));
        stackmat.on('started', guard(polarity, function () {
            if (!callbacks) return;
            running = true;
            // Walk the core through its guarded phases so a start is honoured
            // even when no arming/green-light packets were observed beforehand.
            callbacks.onArming();
            callbacks.onReady();
            callbacks.onStart();
        }));
        stackmat.on('stopped', guard(polarity, function (packet) {
            if (!callbacks || !running) return;
            running = false;
            callbacks.onStop(packet.timeInMilliseconds);
        }));
        stackmat.on('reset', guard(polarity, function () {
            if (!callbacks) return;
            running = false;
            callbacks.onIdle();
        }));
        return stackmat;
    }

    function createInvertedStream(mediaStream) {
        inverterContext = new AudioContext();
        if (inverterContext.state === 'suspended') inverterContext.resume();
        const source = inverterContext.createMediaStreamSource(mediaStream);
        const inverter = inverterContext.createGain();
        inverter.gain.value = -1;
        const destination = inverterContext.createMediaStreamDestination();
        source.connect(inverter);
        inverter.connect(destination);
        return destination.stream;
    }

    function closeInverter() {
        if (inverterContext) {
            inverterContext.close();
            inverterContext = null;
        }
    }

    function startInstance(stackmat, instanceStream) {
        // The library hardcodes its own getUserMedia constraints (it ignores
        // deviceId and leaves autoGainControl on) and swallows errors, so hand
        // it the prepared stream by substituting getUserMedia for the
        // synchronous duration of start().
        const mediaDevices = navigator.mediaDevices;
        const originalGetUserMedia = mediaDevices.getUserMedia;
        mediaDevices.getUserMedia = () => Promise.resolve(instanceStream);
        try {
            stackmat.start();
        } finally {
            mediaDevices.getUserMedia = originalGetUserMedia;
        }
        // The library creates its AudioContext one microtask after start() and
        // never resumes it, so a context born suspended (autoplay policy)
        // would silently decode nothing. Nudge it once it exists.
        setTimeout(function () {
            const context = stackmat.audioProcessor && stackmat.audioProcessor.context;
            if (context && context.state === 'suspended') context.resume();
        }, 50);
    }

    function startDecoder() {
        ensureDecoders();
        activePolarity = null;
        startInstance(decoders.normal, stream.clone());
        startInstance(decoders.inverted, createInvertedStream(stream));
        startSignalWatchdog();
    }

    function stopDecoder() {
        clearSignalWatchdog();
        running = false;
        if (decoders) {
            decoders.normal.stop();
            decoders.inverted.stop();
        }
        activePolarity = null;
        closeInverter();
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
