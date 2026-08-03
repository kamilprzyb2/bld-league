// Core of the built-in submission timer on /Submit/SubmitResults:
// slot model, state machine, rendering, draft persistence and the TimerDriver registry.
// Input methods self-register by pushing a driver object onto window.BldTimerDrivers
// (see wwwroot/js/timer-drivers/), so <script> order does not matter.
(function () {
    'use strict';

    const MODE_STORAGE_KEY = 'bld.submitMode';
    const DRIVER_STORAGE_KEY = 'bld.timerDriver';
    const DRAFT_KEY_PREFIX = 'bld.submitDraft.';
    const DRAFT_SAVE_DELAY_MS = 250;
    const PLUS_TWO_CS = 200;
    // The server-side regex allows at most two minute digits, so 100+ minutes cannot be posted.
    const MAX_CS = 100 * 60 * 100;

    const HINTS = {
        connect: 'Kliknij „Połącz”, aby połączyć z urządzeniem.',
        connecting: 'Trwa łączenie…',
        dirty: 'Zresetuj timer, aby rozpocząć kolejną próbę.',
        idle: 'Przytrzymaj spację lub dotknij pola, aby wystartować timer.',
        arming: 'Trzymaj…',
        ready: 'Puść, aby wystartować.',
        running: 'Naciśnij dowolny klawisz lub dotknij pola, aby zatrzymać. Esc — przerwij bez zapisu.',
        done: 'Wszystkie próby zakończone — możesz wgrać wyniki.',
        unsupported: 'Wybierz inną metodę wprowadzania.'
    };

    const OVERFLOW_MESSAGE = 'Czas przekracza 99:59.99 i nie może zostać zapisany.';
    const CONFIRM_MESSAGE = 'Czy na pewno chcesz wgrać wyniki? Tej operacji nie można cofnąć.';

    function formatCs(cs) {
        const m = Math.floor(cs / 6000);
        const s = Math.floor((cs % 6000) / 100);
        const c = cs % 100;
        const cc = String(c).padStart(2, '0');
        return m > 0 ? `${m}:${String(s).padStart(2, '0')}.${cc}` : `${s}.${cc}`;
    }

    // The running display refreshes at most this often — full centisecond
    // precision, but without the every-frame churn that reads as flicker.
    const RUNNING_UPDATE_MS = 30;

    function dnsWarning(count) {
        if (count === 1) return '1 pusta próba zostanie zapisana jako DNS.';
        if (count >= 2 && count <= 4) return `${count} puste próby zostaną zapisane jako DNS.`;
        return `${count} pustych prób zostanie zapisanych jako DNS.`;
    }

    document.addEventListener('DOMContentLoaded', function () {
        const root = document.getElementById('timer-root');
        if (!root) return;

        const form = root.closest('form');
        const matchId = root.dataset.matchId;
        const userId = root.dataset.userId;
        const noScrambleText = root.dataset.noScrambleText || '';
        // Keyed by user as well as match — both opponents share the match id,
        // so on a shared browser one player's draft must not prefill the other's form.
        const draftKey = DRAFT_KEY_PREFIX + userId + '.' + matchId;

        const modeToggle = document.getElementById('mode-toggle');
        const modeManualButton = document.getElementById('mode-manual-btn');
        const modeTimerButton = document.getElementById('mode-timer-btn');
        const manualPanel = document.getElementById('manual-panel');
        const timerPanel = document.getElementById('timer-panel');
        const driverSelect = document.getElementById('timer-driver-select');
        const errorAlert = document.getElementById('timer-error');
        const timerPad = document.getElementById('timer-pad');
        const attemptNumberEl = document.getElementById('timer-attempt-number');
        const scrambleEl = document.getElementById('timer-scramble');
        const scrambleSource = document.getElementById('timer-scrambles');
        const displayEl = document.getElementById('timer-display');
        const hintEl = document.getElementById('timer-hint');
        const reviewPanel = document.getElementById('timer-review');
        const dnfButton = document.getElementById('timer-dnf-btn');
        const plusTwoButton = document.getElementById('timer-plus2-btn');
        const slotButtons = Array.from(root.querySelectorAll('button[data-slot]'));

        const inputs = Array.from(root.querySelectorAll('input[data-solve-index]'))
            .sort((a, b) => Number(a.dataset.solveIndex) - Number(b.dataset.solveIndex));

        // Per-slot timing metadata; the input values themselves are the single source
        // of truth for what gets posted.
        const slots = inputs.map(() => ({ baseCs: null, penalty: null }));

        const connectedDriverIds = new Set();

        let mode = 'manual';
        let phase = 'idle'; // idle | arming | ready | running | done (+ connect | connecting | dirty | unsupported)
        let currentSlot = 0;
        // The most recently timed slot: after a stop the timer advances to the
        // next attempt immediately (csTimer-style — no confirmation step), but
        // DNF/+2 keep applying to this slot until the next solve starts.
        let lastRecordedSlot = null;
        let activeDriver = null;
        let driverAttached = false;
        let runningStart = 0;
        let lastDisplayUpdate = 0;
        let rafId = null;
        let hasDeviceTick = false;
        let submitted = false;
        let draftSaveTimeout = null;

        function availableDrivers() {
            return Array.isArray(window.BldTimerDrivers) ? window.BldTimerDrivers : [];
        }

        function showError(message) {
            errorAlert.textContent = message;
            errorAlert.classList.remove('d-none');
        }

        function clearError() {
            errorAlert.classList.add('d-none');
        }

        function driverNeedsConnect() {
            return activeDriver && activeDriver.requiresConnect && !connectedDriverIds.has(activeDriver.id);
        }

        function firstEmptySlot(startAfter) {
            for (let step = 1; step <= inputs.length; step++) {
                const i = (startAfter + step + inputs.length) % inputs.length;
                if (inputs[i].value.trim() === '') return i;
            }
            return -1;
        }

        function scrambleTextFor(index) {
            const span = scrambleSource
                ? scrambleSource.querySelector(`span[data-solve-index="${index}"]`)
                : null;
            const text = span ? span.textContent.trim() : '';
            return text !== '' ? text : noScrambleText;
        }

        function refreshAttemptView() {
            if (currentSlot >= 0) {
                attemptNumberEl.textContent = String(currentSlot + 1);
                scrambleEl.textContent = scrambleTextFor(currentSlot);
            } else {
                scrambleEl.textContent = '';
            }
        }

        function refreshSlotButtons() {
            slotButtons.forEach(button => {
                const i = Number(button.dataset.slot);
                const value = inputs[i].value.trim();
                button.textContent = `${i + 1}: ${value === '' ? '—' : value}`;
                button.classList.toggle('active', i === currentSlot);
                button.disabled = value !== '';
            });
        }

        function enterPhase(next) {
            phase = next;
            displayEl.classList.toggle('timer-arming', next === 'arming');
            displayEl.classList.toggle('timer-ready', next === 'ready');
            displayEl.classList.toggle('timer-label', next === 'connect' || next === 'connecting');
            displayEl.classList.toggle('timer-message', next === 'unsupported');
            const reviewVisible = lastRecordedSlot !== null
                && (next === 'idle' || next === 'done' || next === 'dirty');
            reviewPanel.classList.toggle('d-none', !reviewVisible);
            const driverHints = activeDriver && activeDriver.hints ? activeDriver.hints : null;
            hintEl.textContent = (driverHints && driverHints[next]) || HINTS[next] || '';
            refreshSlotButtons();
            refreshAttemptView();
        }

        function enterConnectPrompt() {
            stopDisplayLoop();
            displayEl.textContent = 'Połącz';
            enterPhase('connect');
        }

        // An unsupported driver stays selectable (a disabled <option> with the
        // reason appended blows up the select's width); the full explanation is
        // shown in the timer display instead.
        function enterUnsupportedPrompt(driver) {
            stopDisplayLoop();
            displayEl.textContent = `${driver.label} ${driver.unsupportedReason}.`;
            enterPhase('unsupported');
        }

        function setIdle() {
            stopDisplayLoop();
            if (driverNeedsConnect()) {
                enterConnectPrompt();
                return;
            }
            displayEl.textContent = '0.00';
            enterPhase('idle');
        }

        function resetTimerState() {
            stopDisplayLoop();
            if (currentSlot === -1) {
                displayEl.textContent = '0.00';
                enterPhase('done');
            } else {
                setIdle();
            }
        }

        function displayLoop() {
            if (phase !== 'running') {
                rafId = null;
                return;
            }
            const now = performance.now();
            if (!hasDeviceTick && now - lastDisplayUpdate >= RUNNING_UPDATE_MS) {
                lastDisplayUpdate = now;
                displayEl.textContent = formatCs(Math.floor((now - runningStart) / 10));
            }
            rafId = requestAnimationFrame(displayLoop);
        }

        function stopDisplayLoop() {
            if (rafId !== null) {
                cancelAnimationFrame(rafId);
                rafId = null;
            }
        }

        function writeSlotValue(index, value) {
            inputs[index].value = value;
            refreshSlotButtons();
            scheduleDraftSave();
        }

        function recordTime(index, cs) {
            slots[index] = { baseCs: cs, penalty: null };
            inputs[index].dataset.baseCs = String(cs);
            writeSlotValue(index, formatCs(cs));
        }

        function refreshReviewView() {
            if (lastRecordedSlot === null) return;
            const slot = slots[lastRecordedSlot] || { baseCs: null, penalty: null };
            dnfButton.classList.toggle('active', slot.penalty === 'dnf');
            plusTwoButton.classList.toggle('active', slot.penalty === 'plus2');
            if (slot.baseCs === null) return;
            if (slot.penalty === 'dnf') {
                displayEl.textContent = 'DNF';
            } else if (slot.penalty === 'plus2') {
                displayEl.textContent = `${formatCs(slot.baseCs)} +2 = ${formatCs(slot.baseCs + PLUS_TWO_CS)}`;
            } else {
                displayEl.textContent = formatCs(slot.baseCs);
            }
        }

        function applyPenaltyToggle(kind) {
            if (phase !== 'idle' && phase !== 'done' && phase !== 'dirty') return;
            if (lastRecordedSlot === null) return;
            const slot = slots[lastRecordedSlot];
            if (!slot || slot.baseCs === null) return;
            const next = slot.penalty === kind ? null : kind;
            if (next === 'plus2' && slot.baseCs + PLUS_TWO_CS >= MAX_CS) {
                showError(OVERFLOW_MESSAGE);
                return;
            }
            slot.penalty = next;
            let value;
            if (next === 'dnf') value = 'DNF';
            else if (next === 'plus2') value = formatCs(slot.baseCs + PLUS_TWO_CS);
            else value = formatCs(slot.baseCs);
            writeSlotValue(lastRecordedSlot, value);
            refreshReviewView();
        }

        const driverCallbacks = {
            onIdle() {
                if (mode !== 'timer') return;
                if (phase === 'arming' || phase === 'ready' || phase === 'dirty') setIdle();
            },
            onDirty(ms) {
                // The device shows a leftover time from before the session —
                // mirror it and ask for a reset instead of pretending 0.00.
                if (mode !== 'timer') return;
                if (phase !== 'idle' && phase !== 'arming' && phase !== 'dirty') return;
                // The frozen display of the solve that was just recorded is not
                // a dirty timer (a stopped Stackmat keeps streaming its final
                // time) — keep showing the result with its penalty formatting.
                if (lastRecordedSlot !== null && slots[lastRecordedSlot].baseCs === Math.floor(ms / 10)) return;
                displayEl.textContent = formatCs(Math.floor(ms / 10));
                if (phase !== 'dirty') enterPhase('dirty');
            },
            onArming() {
                if (mode !== 'timer') return;
                // 'dirty' is allowed through: a solve genuinely starting must
                // win over a stale-time display (a reset can slip between two
                // decoded packets and go unnoticed).
                if (phase !== 'idle' && phase !== 'dirty') return;
                clearError();
                displayEl.textContent = '0.00';
                enterPhase('arming');
            },
            onReady() {
                if (mode !== 'timer' || phase !== 'arming') return;
                enterPhase('ready');
            },
            onStart() {
                if (mode !== 'timer' || phase !== 'ready') return;
                // The penalty window for the previous solve closes for good
                // the moment the next one starts.
                lastRecordedSlot = null;
                runningStart = performance.now();
                lastDisplayUpdate = 0;
                hasDeviceTick = false;
                enterPhase('running');
                rafId = requestAnimationFrame(displayLoop);
            },
            onTick(ms) {
                if (phase !== 'running') return;
                hasDeviceTick = true;
                displayEl.textContent = formatCs(Math.floor(ms / 10));
            },
            onStop(ms) {
                if (phase !== 'running') return;
                stopDisplayLoop();
                const cs = Math.floor(ms / 10); // truncate to centiseconds, never round
                if (cs >= MAX_CS) {
                    showError(OVERFLOW_MESSAGE);
                    setIdle();
                    return;
                }
                recordTime(currentSlot, cs);
                // Advance immediately: the next scramble is shown and the timer
                // is ready to start, while the display keeps the finished
                // result and DNF/+2 still target it. Hardware that must be
                // reset before the next solve parks in 'dirty' ("Zresetuj
                // timer") until the driver reports the reset via onIdle.
                lastRecordedSlot = currentSlot;
                currentSlot = firstEmptySlot(currentSlot);
                if (currentSlot === -1) enterPhase('done');
                else if (activeDriver && activeDriver.requiresReset) enterPhase('dirty');
                else enterPhase('idle');
                refreshReviewView();
            },
            onAbort() {
                if (phase !== 'running') return;
                stopDisplayLoop();
                setIdle();
            },
            onError(message) {
                showError(message);
                if (phase === 'connecting' && activeDriver) {
                    // Connection attempt failed — return to the prompt so the
                    // user can retry from scratch.
                    connectedDriverIds.delete(activeDriver.id);
                    if (driverAttached) {
                        activeDriver.detach();
                        driverAttached = false;
                    }
                    enterConnectPrompt();
                }
            },
            onConnected() {
                if (mode !== 'timer' || phase !== 'connecting') return;
                clearError();
                setIdle();
            }
        };

        function populateDriverSelect() {
            driverSelect.innerHTML = '';
            availableDrivers().forEach(driver => {
                const option = document.createElement('option');
                option.value = driver.id;
                option.textContent = driver.label;
                driverSelect.appendChild(option);
            });
            const storedId = localStorage.getItem(DRIVER_STORAGE_KEY);
            const stored = availableDrivers().find(d => d.id === storedId && d.isSupported());
            const fallback = availableDrivers().find(d => d.isSupported());
            const chosen = stored || fallback;
            if (chosen) driverSelect.value = chosen.id;
        }

        function attachActiveDriver() {
            if (!activeDriver || driverAttached) return;
            activeDriver.attach(driverCallbacks);
            driverAttached = true;
        }

        function activateDriver() {
            const driver = availableDrivers().find(d => d.id === driverSelect.value);
            if (!driver) {
                showError('Brak dostępnej metody wprowadzania.');
                return;
            }
            if (!driver.isSupported()) {
                activeDriver = null;
                enterUnsupportedPrompt(driver);
                return;
            }
            activeDriver = driver;
            localStorage.setItem(DRIVER_STORAGE_KEY, driver.id);
            enterPhase(phase); // re-render the hint with the driver's own texts
            if (driverNeedsConnect()) {
                enterConnectPrompt();
                return;
            }
            attachActiveDriver();
        }

        async function beginConnect() {
            if (!activeDriver) return;
            clearError();
            displayEl.textContent = 'Trwa łączenie…';
            enterPhase('connecting');
            try {
                await activeDriver.connect();
                connectedDriverIds.add(activeDriver.id);
                // Stay in 'connecting' — the driver reports onConnected once
                // it actually receives a signal, which flips the pad to 0.00.
                attachActiveDriver();
            } catch (error) {
                showError(error && error.message ? error.message : 'Nie udało się połączyć z urządzeniem.');
                enterConnectPrompt();
            }
        }

        function deactivateDriver() {
            if (activeDriver && driverAttached) activeDriver.detach();
            driverAttached = false;
            stopDisplayLoop();
        }

        function setMode(next, persist) {
            mode = next;
            modeManualButton.classList.toggle('active', next === 'manual');
            modeTimerButton.classList.toggle('active', next === 'timer');
            manualPanel.classList.toggle('d-none', next !== 'manual');
            timerPanel.classList.toggle('d-none', next !== 'timer');
            if (persist) localStorage.setItem(MODE_STORAGE_KEY, next);
            if (next === 'timer') {
                clearError();
                currentSlot = firstEmptySlot(-1);
                resetTimerState();
                activateDriver();
            } else {
                deactivateDriver();
            }
        }

        function scheduleDraftSave() {
            clearTimeout(draftSaveTimeout);
            draftSaveTimeout = setTimeout(saveDraft, DRAFT_SAVE_DELAY_MS);
        }

        function saveDraft() {
            const draft = {
                matchId: matchId,
                values: inputs.map(i => i.value),
                baseCs: slots.map(s => s.baseCs),
                penalties: slots.map(s => s.penalty)
            };
            try {
                localStorage.setItem(draftKey, JSON.stringify(draft));
            } catch (e) {
                // Storage unavailable — drafts are best-effort only.
            }
        }

        function clearDraft() {
            clearTimeout(draftSaveTimeout);
            try {
                localStorage.removeItem(draftKey);
            } catch (e) {
                // Storage unavailable — nothing to clear.
            }
        }

        function adoptDraftMeta(index, draft) {
            const base = Array.isArray(draft.baseCs) ? draft.baseCs[index] : null;
            const penalty = Array.isArray(draft.penalties) ? draft.penalties[index] : null;
            slots[index] = {
                baseCs: typeof base === 'number' ? base : null,
                penalty: penalty === 'dnf' || penalty === 'plus2' ? penalty : null
            };
            if (slots[index].baseCs !== null) {
                inputs[index].dataset.baseCs = String(slots[index].baseCs);
            }
        }

        function restoreDraft() {
            let draft = null;
            try {
                draft = JSON.parse(localStorage.getItem(draftKey) || 'null');
            } catch (e) {
                draft = null;
            }
            if (!draft) return;
            if (draft.matchId !== matchId) {
                clearDraft();
                return;
            }
            inputs.forEach((input, i) => {
                const draftValue = Array.isArray(draft.values) ? (draft.values[i] || '') : '';
                if (input.value.trim() !== '') {
                    // Server re-rendered values win; adopt metadata only when they agree.
                    if (input.value === draftValue) adoptDraftMeta(i, draft);
                    return;
                }
                if (draftValue !== '') {
                    input.value = draftValue;
                    adoptDraftMeta(i, draft);
                }
            });
        }

        modeManualButton.addEventListener('click', function () {
            this.blur();
            setMode('manual', true);
        });

        modeTimerButton.addEventListener('click', function () {
            this.blur();
            setMode('timer', true);
        });

        driverSelect.addEventListener('change', function () {
            deactivateDriver();
            activeDriver = null;
            resetTimerState();
            if (mode === 'timer') activateDriver();
        });

        timerPad.addEventListener('click', function () {
            if (phase === 'connect') beginConnect();
        });

        dnfButton.addEventListener('click', function () {
            this.blur();
            applyPenaltyToggle('dnf');
        });

        plusTwoButton.addEventListener('click', function () {
            this.blur();
            applyPenaltyToggle('plus2');
        });

        slotButtons.forEach(button => button.addEventListener('click', function () {
            if (phase === 'arming' || phase === 'ready' || phase === 'running' || phase === 'connecting' || phase === 'unsupported') return;
            const i = Number(this.dataset.slot);
            if (inputs[i].value.trim() !== '') return;
            this.blur();
            currentSlot = i;
            setIdle();
        }));

        inputs.forEach((input, i) => input.addEventListener('input', function () {
            slots[i] = { baseCs: null, penalty: null };
            delete input.dataset.baseCs;
            // Manual edits detach the slot from the timer's penalty buttons.
            if (i === lastRecordedSlot) lastRecordedSlot = null;
            refreshSlotButtons();
            scheduleDraftSave();
        }));

        if (form) {
            form.addEventListener('submit', function (e) {
                const emptyCount = inputs.filter(i => i.value.trim() === '').length;
                let message = CONFIRM_MESSAGE;
                if (emptyCount > 0) message += `\n\nUwaga: ${dnsWarning(emptyCount)}`;
                if (!window.confirm(message)) {
                    e.preventDefault();
                    return;
                }
                submitted = true;
                clearDraft();
            });
        }

        window.addEventListener('beforeunload', function (e) {
            if (submitted) return;
            if (!inputs.some(i => i.value.trim() !== '')) return;
            e.preventDefault();
            e.returnValue = '';
        });

        restoreDraft();
        refreshSlotButtons();
        populateDriverSelect();
        modeToggle.classList.remove('d-none');
        const storedMode = localStorage.getItem(MODE_STORAGE_KEY);
        setMode(storedMode === 'timer' ? 'timer' : 'manual', false);
    });
})();
