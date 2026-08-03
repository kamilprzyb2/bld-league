# GAN Smart Timer (Web Bluetooth) input — findings & implementation notes

The submission timer on `/Submit/SubmitResults` supports reading a GAN Smart
Timer / GAN Halo Smart Timer over Bluetooth Low Energy. This document records
the protocol knowledge and design decisions behind the driver, in the same
spirit as `docs/stackmat-timer.md`.

**Protocol source:** the driver is hand-rolled (no vendored library) after
Andy Fedotov's public write-up:
<https://gist.github.com/afedotov/a025fa5796c9c727b04cf98b293a02f6>.
**Tested hardware:** a GAN smart timer over Chromium — basic solve flow and
connect-time dirty detection confirmed working. <!-- TODO: exact model -->

## Architecture recap

- Driver: `src/Web/wwwroot/js/timer-drivers/gan-bluetooth.js`, a
  self-registering `TimerDriver` for the submission-timer core
  (`submit-timer.js`). No external library — the GATT protocol is small
  enough to decode directly.
- The device chooser is the browser's own `requestDevice()` prompt, filtered
  on the `GAN` name prefix. Web Bluetooth requires that call to happen inside
  a user gesture, which is why connecting starts from a click on the pad.

## Protocol

One primary service, UUID `0000fff0-0000-1000-8000-00805f9b34fb`, with two
characteristics:

| Characteristic | UUID (`0000xxxx-0000-1000-8000-00805f9b34fb`) | Access | Content |
|---|---|---|---|
| Timer state | `fff5` | **NOTIFY only** | Short state packets on every transition |
| Stored times | `fff2` | **READ only** | 16 bytes: currently displayed time + three previous recorded times |

State packet layout: `0xFE` magic, remaining-length byte, `0x01` data prefix,
state code, optional 4-byte time, CRC-16/CCITT-FALSE (little-endian) over
everything between the length byte and the CRC.

Time layout (both characteristics): minutes (uint8), seconds (uint8),
milliseconds (uint16 LE).

State codes (byte 3 of a state packet):

| Code | State | Meaning |
|---|---|---|
| `0x01` | GET_SET | Grace delay expired, ready to start |
| `0x02` | HANDS_OFF | Hands removed before the grace delay expired |
| `0x03` | RUNNING | Timer counting |
| `0x04` | STOPPED | Solve finished — **carries the recorded time** |
| `0x05` | IDLE | Timer reset (GAN logo button) |
| `0x06` | HANDS_ON | Both hands placed on the timer |
| `0x07` | FINISHED | Follows STOPPED automatically, carries nothing new — ignored |

## What works

- Full solve flow: HANDS_ON → arming, GET_SET → ready, RUNNING → start,
  STOPPED → recorded time, IDLE mid-solve → abort, HANDS_OFF before the
  grace delay → back to idle.
- The recorded result comes **exclusively** from the STOPPED packet — never
  from a local clock.
- After a stop the page advances straight to the next attempt (next scramble
  shown) and asks for a reset („Zresetuj timer”); the
  finished time stays on the display and DNF/+2 keep targeting it until the
  next solve starts. The IDLE notification (logo button pressed) returns the
  display to `0.00` with the normal start hint.
- Leftover-time ("dirty") detection **at connect time**: after every
  (re)connection the driver reads `fff2`; a non-zero displayed time flips the
  UI into its dirty state (leftover time mirrored, "Zresetuj timer" hint)
  instead of pretending `0.00`. A solve can still start straight from the
  dirty state — a genuine start always wins.
- Automatic reconnection: on an unexpected GATT disconnect a running attempt
  is aborted, then up to 3 reconnect attempts run 2 s apart. The device
  object is kept for the page lifetime, so re-attaching (e.g. after switching
  input methods) reconnects without showing the chooser again —
  `gatt.connect()` needs no user gesture, only `requestDevice()` does.

## What cannot be done (and the partial workarounds)

### The current state cannot be queried

`fff5` is notify-only: state is delivered on *transitions*, so right after a
connection the page cannot ask "are hands on the pads right now?" or "is a
solve running?". Consequences and mitigations:

- **Dirty display** — covered by the `fff2` read above; this is the one piece
  of current state the protocol lets us read.
- **Hands already on the pads at connect** — the driver attempts a
  best-effort `readValue()` on `fff5` anyway (a CRC-valid response would be
  fed through the normal packet handler); the spec says the read should be
  rejected, in which case the attempt fails silently and the first real
  notification takes over. <!-- TODO: confirm on hardware whether the read is
  rejected or returns a packet -->
- **Connecting mid-solve** — the first observed packet is then RUNNING (or
  nothing until the solve ends). A STOPPED packet with no previously observed
  RUNNING is deliberately ignored: it carries a time for an attempt the page
  never saw start.

### No running ticks

The state stream has no periodic time packets while running, so the driver
sends no `onTick` — the core interpolates the running display from a local
clock. The displayed running time is therefore approximate; only the final
STOPPED time is authoritative, and that is what gets recorded.

### Dirty detection is connect-time only

Unlike the Stackmat (which streams packets continuously, re-flagging a stale
display), the GAN timer sends nothing while sitting idle. The `fff2` read
happens once per (re)connection; a leftover time appearing later without any
state transition cannot be noticed.

## Browser support

Web Bluetooth exists only in Chromium-based browsers (Chrome, Edge, Opera) on
a secure context (HTTPS). It is unavailable in Firefox and Safari, and on iOS
every browser is WebKit — so no iOS support at all. The submission-timer core
keeps the driver selectable in unsupported browsers and renders the full
explanation inside the timer display (`unsupported` phase) instead of
disabling the `<option>` — a disabled option with the reason appended made
the select unusably wide.

## UI notes

Shared display conventions (DSEG7 seven-segment font, connect-by-clicking-
the-pad, text states falling back to the UI font) are documented in
`docs/stackmat-timer.md` and apply unchanged.
