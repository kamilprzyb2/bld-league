// Keyboard/touch TimerDriver for the submission timer.
// Hold space (or press the tap pad) to arm, release after 300ms to start,
// any key (or a tap) to stop. Esc aborts a running attempt.
(function () {
    'use strict';

    const HOLD_TO_READY_MS = 300;

    let callbacks = null;
    let phase = 'idle'; // idle | holding | ready | running
    let holdTimeout = null;
    let runStart = 0;
    let pad = null;

    function isFormElement(target) {
        if (!target || !target.tagName) return false;
        const tag = target.tagName.toLowerCase();
        return tag === 'input' || tag === 'textarea' || tag === 'select' || tag === 'button';
    }

    function beginHold() {
        if (phase !== 'idle') return;
        phase = 'holding';
        callbacks.onArming();
        holdTimeout = setTimeout(function () {
            if (phase === 'holding') {
                phase = 'ready';
                callbacks.onReady();
            }
        }, HOLD_TO_READY_MS);
    }

    function cancelHold() {
        clearTimeout(holdTimeout);
        phase = 'idle';
        callbacks.onIdle();
    }

    function releaseHold() {
        if (phase === 'holding') {
            cancelHold();
            return;
        }
        if (phase === 'ready') {
            clearTimeout(holdTimeout);
            phase = 'running';
            runStart = performance.now();
            callbacks.onStart();
        }
    }

    function stopRun() {
        if (phase !== 'running') return;
        const elapsedMs = performance.now() - runStart;
        phase = 'idle';
        callbacks.onStop(elapsedMs);
    }

    function abortRun() {
        if (phase !== 'running') return;
        phase = 'idle';
        callbacks.onAbort();
    }

    function handleKeyDown(e) {
        if (isFormElement(e.target)) return;
        if (phase === 'running') {
            e.preventDefault();
            if (e.key === 'Escape') abortRun();
            else stopRun();
            return;
        }
        if (e.code === 'Space') {
            e.preventDefault();
            if (!e.repeat) beginHold();
        } else if (e.key === 'Escape' && (phase === 'holding' || phase === 'ready')) {
            cancelHold();
        }
    }

    function handleKeyUp(e) {
        if (isFormElement(e.target)) return;
        if (e.code !== 'Space') return;
        e.preventDefault();
        releaseHold();
    }

    function handlePointerDown(e) {
        e.preventDefault();
        if (pad && pad.setPointerCapture && e.pointerId !== undefined) {
            try {
                pad.setPointerCapture(e.pointerId);
            } catch (error) {
                // Pointer capture is best-effort.
            }
        }
        if (phase === 'running') {
            stopRun();
            return;
        }
        beginHold();
    }

    function handlePointerUp(e) {
        e.preventDefault();
        releaseHold();
    }

    function handlePointerCancel() {
        if (phase === 'holding' || phase === 'ready') cancelHold();
    }

    (window.BldTimerDrivers = window.BldTimerDrivers || []).push({
        id: 'keyboard',
        label: 'Klawiatura / dotyk',
        isSupported: () => true,
        unsupportedReason: '',
        requiresConnect: false,
        async connect() { },
        async disconnect() { },
        attach(cb) {
            callbacks = cb;
            phase = 'idle';
            pad = document.getElementById('timer-pad');
            document.addEventListener('keydown', handleKeyDown);
            document.addEventListener('keyup', handleKeyUp);
            if (pad) {
                pad.addEventListener('pointerdown', handlePointerDown);
                pad.addEventListener('pointerup', handlePointerUp);
                pad.addEventListener('pointercancel', handlePointerCancel);
            }
        },
        detach() {
            document.removeEventListener('keydown', handleKeyDown);
            document.removeEventListener('keyup', handleKeyUp);
            if (pad) {
                pad.removeEventListener('pointerdown', handlePointerDown);
                pad.removeEventListener('pointerup', handlePointerUp);
                pad.removeEventListener('pointercancel', handlePointerCancel);
            }
            clearTimeout(holdTimeout);
            phase = 'idle';
            callbacks = null;
            pad = null;
        }
    });
})();
