// Stackmat (audio jack) TimerDriver for the submission timer.
// Decodes the signal a Stackmat/SpeedStacks timer streams over the 3.5mm jack
// using the vendored stackmat library (wwwroot/lib/stackmat/). If that library
// failed to load, this file registers nothing and the other drivers still work.
(function () {
    'use strict';

    if (typeof window.Stackmat !== 'function') return;

    const NO_SIGNAL_TIMEOUT_MS = 3000;

    const PERMISSION_DENIED_MESSAGE = 'Brak dostępu do mikrofonu. Zezwól stronie na użycie mikrofonu i spróbuj ponownie.';
    const NO_DEVICES_MESSAGE = 'Nie znaleziono żadnego wejścia audio. Podłącz timer do wejścia mikrofonowego i spróbuj ponownie.';
    const NO_SIGNAL_MESSAGE = 'Brak sygnału z timera. Sprawdź, czy w oknie przeglądarki wybrane jest właściwe wejście audio i czy timer jest podłączony i włączony.';
    const STREAM_FAILED_MESSAGE = 'Nie udało się otworzyć wejścia audio.';

    const RUNNING_STATUS = ' '; // PacketStatus.RUNNING in the stackmat library

    let callbacks = null;
    let decoders = null;        // page-lifetime pair, see ensureDecoders()
    let activePolarity = null;  // 'normal' | 'inverted', picked by the first valid packet
    let inverterContext = null; // per-session context producing the inverted stream
    let stream = null;
    let running = false;
    let lastPacketMs = -1;      // time carried by the previous packet, any status
    let lastTick = null;        // { deviceMs, at } — most recent running packet
    let lastTickSentAt = 0;
    let interpolationFrame = null;
    let signalTimeout = null;
    let attachToken = 0;

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

    async function requestStream() {
        // Which input to open is the browser's job: its permission prompt lets
        // the user pick the device and remembers the choice per site.
        try {
            const acquired = await navigator.mediaDevices.getUserMedia({
                audio: {
                    echoCancellation: false,
                    noiseSuppression: false,
                    autoGainControl: false
                }
            });
            await disableProcessing(acquired);
            return acquired;
        } catch (error) {
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

    // The library only surfaces ~4-5 device ticks per second (its worklet
    // buffers ~213ms of audio per decode), which makes the display jump.
    // Interpolate between device ticks with a local clock so the display is
    // smooth; the recorded result still comes exclusively from the device
    // (onStop below always uses the packet time). Ticks are throttled so the
    // UI is asked to refresh at most every TICK_INTERVAL_MS.
    const TICK_INTERVAL_MS = 30;

    function interpolateTick() {
        interpolationFrame = null;
        if (!callbacks || !running || !lastTick) return;
        const now = performance.now();
        if (now - lastTickSentAt >= TICK_INTERVAL_MS) {
            lastTickSentAt = now;
            callbacks.onTick(lastTick.deviceMs + (now - lastTick.at));
        }
        interpolationFrame = requestAnimationFrame(interpolateTick);
    }

    function recordDeviceTick(deviceMs) {
        lastTick = { deviceMs: deviceMs, at: performance.now() };
        if (interpolationFrame === null) interpolationFrame = requestAnimationFrame(interpolateTick);
    }

    function stopInterpolation() {
        if (interpolationFrame !== null) {
            cancelAnimationFrame(interpolationFrame);
            interpolationFrame = null;
        }
        lastTick = null;
        lastTickSentAt = 0;
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
                if (callbacks && callbacks.onConnected) callbacks.onConnected();
            }
            clearSignalWatchdog();
            if (!callbacks) return;
            const ms = packet.timeInMilliseconds;
            if (running) {
                if (packet.status === RUNNING_STATUS) recordDeviceTick(ms);
            } else if (ms === 0 && lastPacketMs > 0) {
                // The displayed time dropped back to zero outside a solve: the
                // user pressed reset. The library's own 'reset' event does not
                // re-fire on timers that already report their stopped state as
                // idle, so detect it at the packet level.
                callbacks.onIdle();
            } else if (ms > 0 && packet.status !== RUNNING_STATUS && callbacks.onDirty) {
                // Stopped/idle status with a time on the display — a leftover
                // result from before the session. The core mirrors it and asks
                // for a reset. Running-status packets are excluded: the first
                // one of a solve arrives before the library's 'started' event
                // (packetReceived always fires first), so it must not be
                // mistaken for a stale time.
                callbacks.onDirty(ms);
            }
            lastPacketMs = ms;
        });
        stackmat.on('leftHandDown', guard(polarity, handleHandsChanged));
        stackmat.on('rightHandDown', guard(polarity, handleHandsChanged));
        stackmat.on('leftHandUp', guard(polarity, handleHandsChanged));
        stackmat.on('rightHandUp', guard(polarity, handleHandsChanged));
        stackmat.on('starting', guard(polarity, function () {
            if (!callbacks || running) return;
            callbacks.onReady();
        }));
        stackmat.on('started', guard(polarity, function (packet) {
            if (!callbacks) return;
            running = true;
            // Walk the core through its guarded phases so a start is honoured
            // even when no arming/green-light packets were observed beforehand.
            callbacks.onArming();
            callbacks.onReady();
            callbacks.onStart();
            // Seed the interpolation clock from the packet that triggered the
            // start, so the display doesn't lag a whole decode buffer behind.
            recordDeviceTick(packet.timeInMilliseconds);
        }));
        stackmat.on('stopped', guard(polarity, function (packet) {
            if (!callbacks || !running) return;
            running = false;
            stopInterpolation();
            callbacks.onStop(packet.timeInMilliseconds);
        }));
        stackmat.on('reset', guard(polarity, function (packet) {
            if (!callbacks) return;
            const wasRunning = running;
            running = false;
            stopInterpolation();
            if (wasRunning && packet.timeInMilliseconds > 0) {
                // Some timers jump straight to the idle status with the final
                // time frozen instead of sending a stop packet — that is a
                // finished solve, not a reset.
                callbacks.onStop(packet.timeInMilliseconds);
            } else if (wasRunning) {
                // A reset mid-solve discards the attempt: the core ignores
                // onIdle while in its running phase, so abort explicitly.
                callbacks.onAbort();
            } else {
                callbacks.onIdle();
            }
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
        // autoGainControl) and swallows errors, so hand it the prepared stream
        // by substituting getUserMedia for the synchronous duration of start().
        // This relies on the vendored stackmat v1.1.1 calling getUserMedia
        // synchronously inside start() — do not upgrade lib/stackmat without
        // re-testing this substitution against real timer hardware.
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
        lastPacketMs = -1;
        stopInterpolation();
        if (decoders) {
            decoders.normal.stop();
            decoders.inverted.stop();
        }
        activePolarity = null;
        closeInverter();
        releaseStream();
    }

    async function startSession(token) {
        let sessionStream = stream; // stream pre-acquired by connect(), if any
        if (!sessionStream) {
            try {
                sessionStream = await requestStream();
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
        startDecoder();
    }

    (window.BldTimerDrivers = window.BldTimerDrivers || []).push({
        id: 'stackmat',
        label: 'Stackmat',
        isSupported: () => !!(navigator.mediaDevices && navigator.mediaDevices.getUserMedia && window.isSecureContext),
        unsupportedReason: 'wymaga bezpiecznego połączenia (HTTPS) i dostępu do mikrofonu',
        requiresConnect: true,
        requiresReset: true,
        hints: {
            connect: 'Kliknij „Połącz”, aby połączyć z timerem.',
            connecting: 'Czekam na sygnał z timera…',
            idle: 'Połóż ręce na timerze, aby wystartować.',
            arming: 'Trzymaj ręce na timerze…',
            ready: 'Puść ręce, aby wystartować.',
            running: 'Zatrzymaj timer dłońmi, aby zapisać czas.'
        },
        async connect() {
            // Runs from the user's click on „Połącz”, so the permission prompt
            // is tied to a user gesture. Throws a Polish message on failure.
            stream = await requestStream();
        },
        async disconnect() { },
        attach(cb) {
            callbacks = cb;
            running = false;
            startSession(++attachToken);
        },
        detach() {
            attachToken++;
            stopDecoder();
            callbacks = null;
        }
    });
})();
