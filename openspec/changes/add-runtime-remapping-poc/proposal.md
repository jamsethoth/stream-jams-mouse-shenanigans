## Why

The project now has a testable core remapping model, but it has not yet answered the highest-risk MVP question: whether a small user-session tray app can reliably intercept normal mouse movement, transform it, and re-inject corrected movement only for a chosen third-party target. This change creates the smallest Windows-only runtime proof of concept before adding profile UI, hotkeys, persistence, or Streamer.bot automation.

## What Changes

- Add a Windows runtime remapping coordinator that can be enabled and disabled by the tray host.
- Add low-level mouse movement observation through standard Win32 APIs.
- Add target-window gating for one configured process name or window-title match.
- Apply the active core remapping profile to captured mouse deltas when the target gate matches.
- Inject remapped cursor movement through standard Win32 cursor/input APIs.
- Add a feedback-loop guard so injected movement is not immediately remapped again.
- Provide one hard-coded runtime profile and target configuration path suitable for manual proof-of-concept verification.
- Add unit coverage for pure runtime state decisions where they can be tested without a Windows desktop session.
- Defer global hotkeys, emergency-disable hotkey wiring, runtime profile switching UI, profile file persistence, Streamer.bot/local automation endpoints, installers, signing, and driver-level approaches.

## Capabilities

### New Capabilities

- `runtime-remapping-poc`: Covers the Windows-only proof-of-concept runtime that gates mouse remapping to one configured third-party target, applies a core remapping profile, injects corrected movement, and avoids self-remapping feedback loops.

### Modified Capabilities

None.

## Impact

- Affected code will primarily be `MouseShenanigans.Windows`, `MouseShenanigans.Tray`, and focused test seams in `MouseShenanigans.Core` or Windows-adjacent abstractions where practical.
- The runtime implementation will add Win32 interop boundaries for mouse hooks, target-window inspection, and cursor/input injection.
- The tray shell will gain only enough composition to start, stop, dispose, and surface the proof-of-concept runtime status.
- Manual Windows desktop verification becomes required for hook installation, target-window gating, injected movement, and feedback-loop behavior.
- No public remote API, driver-level input layer, or Streamer.bot command protocol is introduced in this change.
