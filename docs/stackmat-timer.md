# Stackmat (audio jack) input — findings & implementation notes

The submission timer on `/Submit/SubmitResults` supports reading a physical
Stackmat timer through the computer's microphone jack. This document records
what was learned while making that work, because almost none of it was obvious
from the library documentation and most of it was established empirically.

**Tested hardware:** SpeedStacks StackMat **Gen4**, connected through a
generic *"USB Audio Device Mono"* USB sound-card adapter.
**Tested browsers:** Chromium 150 and Firefox 153 — both work; Firefox has
device-selection pitfalls (see below).

## Architecture recap

- Vendored decoder library: [stilesdev/stackmat](https://github.com/stilesdev/stackmat)
  v1.1.1 (`src/Web/wwwroot/lib/stackmat/`), used **unmodified** — all quirks
  are worked around from the driver.
- Driver: `src/Web/wwwroot/js/timer-drivers/stackmat.js`, a self-registering
  `TimerDriver` for the submission-timer core (`submit-timer.js`).

## What works

- Decoding the Gen4 packet stream (10-byte packets: status char, 6 time
  digits, checksum, LF, CR) in Chromium and Firefox.
- Start/stop/reset detection, live running time, recording the final time
  with device precision (the result always comes from the device's packet,
  never from a local clock).
- Signal-polarity independence (see below).
- Leftover-time detection: if the timer still displays a result from before
  the session, the page mirrors it and asks for a reset instead of showing
  a fake `0.00`.
- Resetting the timer while a result is under review confirms it and
  advances to the next attempt.

## What does not work (and why)

### Hand detection — impossible on this hardware

The Stackmat protocol has status characters for hand contact (`L`/`R`/`C`)
and the pre-start green light (`A`), and the driver maps them to the UI's
arming/ready states. However, a raw signal capture taken **while both hands
rested on the pads** contained exclusively `I` (idle) packets — this Gen4
unit simply does not transmit hand states on its data port. csTimer shows
the same behaviour with this hardware, which confirms it is the timer, not
our stack. The mapping is kept in the driver in case other units (or
firmware revisions) do transmit them, but on the tested Gen4 the first
observable event of a solve is the first running packet.

## Browser notes

### Firefox — works, but the permission prompt is the only device picker

Firefox works once the **correct input is chosen in its own permission
prompt**. Two pitfalls, both device-selection related:

- Firefox only exposes the granted device in `enumerateDevices`, so an
  in-page device picker cannot offer alternatives (one reason the in-page
  picker was removed — see UI notes). If the wrong device was granted and
  remembered, the site permission must be cleared and re-granted.
- Sessions where Firefox had granted the internal microphone produced
  useless captures (44.1 kHz, low amplitude, `track.getSettings()` showing
  `autoGainControl`/`echoCancellation` back on) that decoded to zero
  packets — easy to misread as a decoder problem when it is simply the
  wrong device. Checking `track.label` is the fastest way to tell.

The driver additionally re-applies the no-processing constraints on the
live track after acquisition (`applyConstraints`) as belt and braces.

## Hardware/protocol quirks discovered

### Signal polarity is inverted (device-dependent)

The library converts samples to bits with a bare sign test
(`sample <= 0 → 1`). With the tested USB adapter the electrical polarity is
the opposite of what the library expects: the inter-packet idle level is
*positive*, so the library's packet-boundary search never matched and zero
packets decoded — while csTimer, which tries both polarities, worked fine.

**Workaround:** the driver runs **two** library instances in parallel — one
fed the raw microphone stream, the other a polarity-inverted copy (WebAudio
`GainNode` with gain −1). Packets carry a checksum, so only the correctly
oriented instance ever produces valid packets; the first valid packet picks
the winner and the losing instance is stopped.

### Stops are reported as idle, not stopped

The protocol has a dedicated stopped status (`S`), but the tested Gen4 jumps
straight to `I` (idle) with the final time frozen. Consequently the library
fires its `reset` event instead of `stopped` at the end of a solve. The
driver treats *reset while running with time > 0* as a stop. Similarly, the
library's `reset` event does not re-fire when the user later zeroes the
timer (its internal flag only rearms on a start), so the driver detects the
display dropping back to zero at packet level.

### Event ordering race

The library fires `packetReceived` *before* the derived events (`started`,
`stopped`, `reset`) for the same packet. The first running packet of a solve
therefore arrives while the driver still considers the timer stopped — it
must not be interpreted as a stale leftover time (this caused a real bug:
solves froze in the "reset the timer" state at ~0.2 s).

## Library quirks worked around (without modifying it)

| Quirk | Workaround in the driver |
|---|---|
| `AudioProcessor.start()` hardcodes its `getUserMedia` constraints (no `autoGainControl: false`) and swallows errors via `console.error` | The driver acquires the stream itself with full constraints and substitutes `navigator.mediaDevices.getUserMedia` for the synchronous duration of `stackmat.start()` (the library calls it synchronously), restoring it in `finally` |
| `TimerEventManager` starts a `setInterval` it never clears | Library instances are created once per page (a fixed pair) and reused across attach/detach cycles |
| The decode worklet buffers 10 240 samples (~213 ms at 48 kHz) and extracts one packet per buffer → only ~4–5 device ticks/s | The driver interpolates between device ticks with a local `requestAnimationFrame` clock (throttled to one UI update per 30 ms); the recorded result still comes exclusively from device packets |
| The library never resumes its `AudioContext`, which browsers may create suspended (autoplay policy) | The driver nudges `context.resume()` shortly after `start()` |

## Diagnosing problems

A standalone diagnostic page was used during development (raw signal
statistics, both-polarity decoding, WAV capture). If hardware issues come
back, the most effective loop was:

1. Confirm the browser opened the right device (`track.label`,
   `track.getSettings()` — check the processing flags are really off).
2. Look at raw stats: near-full-scale peak with hundreds of zero
   crossings/s means the timer signal is arriving; a big DC offset or low
   amplitude means the audio path is mangling it.
3. Record a few seconds of WAV and decode offline (both polarities,
   checksum-validated) — this separates "no signal" from "signal present
   but decoder fails" definitively.

## UI notes

- The time is rendered in the vendored [DSEG7](https://github.com/keshikan/DSEG)
  seven-segment font (`src/Web/wwwroot/lib/dseg/`) — fixed-width digits, no
  layout jitter. Text states (connect prompt) fall back to the UI font since
  DSEG7 has no Polish diacritics.
- Connecting is done by clicking the timer pad itself (`Połącz` →
  `Trwa łączenie…` → `0.00`); the page only reports "connected" after the
  first valid packet, not merely after microphone permission.
- Device selection is deliberately left to the browser's permission prompt —
  an in-page device picker was tried and removed (Firefox cannot populate it
  meaningfully, and Chromium's prompt already remembers the choice).
