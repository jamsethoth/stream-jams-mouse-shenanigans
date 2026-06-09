## 1. Runtime Decision Model

- [x] 1.1 Add runtime options for one target match and one active `RemappingProfile`.
- [x] 1.2 Add runtime status and lifecycle abstractions for disabled, enabled, unsupported, and failed states.
- [x] 1.3 Add target-match decision logic for foreground-window and under-cursor process-name or title matching.
- [x] 1.4 Add mouse-event decision logic that distinguishes physical movement, injected movement, target matches, and pass-through outcomes.
- [x] 1.5 Add deterministic integer movement conversion at the Windows boundary for remapped `double` deltas.

## 2. Win32 Runtime Boundaries

- [x] 2.1 Add a low-level mouse hook adapter using `SetWindowsHookEx` with `WH_MOUSE_LL` and safe unhook disposal.
- [x] 2.2 Add a target-window adapter for foreground window, window under cursor, window title, process id, and process name lookup.
- [x] 2.3 Add a relative movement injection adapter using `SendInput`.
- [x] 2.4 Keep P/Invoke types and constants scoped to `MouseShenanigans.Windows`.
- [x] 2.5 Report hook or injection setup failures through runtime status instead of leaving the runtime half-enabled.
- [x] 2.6 Add a Raw Input mouse observation adapter for the absolute cursor-positioning proof-of-concept path.
- [x] 2.7 Add an absolute cursor position adapter using `GetCursorPos` and `SetCursorPos`.

## 3. Runtime Remapping Coordinator

- [x] 3.1 Implement a coordinator that enables, disables, and disposes the hook and adapters.
- [x] 3.2 On eligible target movement, compute raw deltas, apply the active core remapping profile, suppress the original event, and request replacement movement injection.
- [x] 3.3 Pass through non-target movement without injecting replacement movement.
- [x] 3.4 Pass through injected movement without applying the remapping profile again.
- [x] 3.5 Keep hook callback work minimal and move deterministic decisions into testable methods.
- [x] 3.6 On eligible Raw Input movement, compute a cursor-position correction from the active profile and set the final absolute cursor position.
- [x] 3.7 Keep Raw Input absolute correction bounded to raw input magnitude after cursor-baseline screen-delta correction proved unstable under fast movement.
- [x] 3.8 Add an explicit absolute correction scale so different mouse DPI settings can be calibrated without changing profile math.

## 4. Tray Proof-of-Concept Integration

- [x] 4.1 Wire the tray host to construct runtime proof-of-concept options using the built-in horizontal inversion profile and one localized target configuration.
- [x] 4.2 Add minimal tray commands to enable and disable the proof-of-concept runtime.
- [x] 4.3 Update tray text or menu state to show disabled, enabled, unsupported, or failed runtime status.
- [x] 4.4 Dispose the runtime when the tray app exits.
- [x] 4.6 Route the tray Exit menu through `ApplicationContext.ExitThread()` so a tray-only app process terminates cleanly.
- [x] 4.5 Keep global hotkeys, profile switching UI, profile persistence, and external automation endpoints out of this slice.

## 5. Automated Validation

- [x] 5.1 Add unit tests for target-match decisions without calling real Win32 APIs.
- [x] 5.2 Add unit tests for runtime remapping decisions: disabled pass-through, target pass-through, target remap, zero-output suppression, and injected-event pass-through.
- [x] 5.3 Add unit tests for integer movement conversion at the Windows boundary.
- [x] 5.4 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 5.5 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 5.6 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 5.7 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.

## 6. Manual Windows Proof-of-Concept Verification

- [x] 6.1 In a real Windows desktop session, launch the tray app and verify the runtime starts disabled.
- [x] 6.2 Enable the runtime and verify Raw Input observation registers without blocking ordinary desktop input.
- [x] 6.3 With the configured target foreground, verify horizontal movement is inverted and vertical movement is preserved.
- [x] 6.4 With the configured target under the cursor, verify remapping applies according to the target gate.
- [x] 6.5 With a non-target window active and under the cursor, verify ordinary movement passes through unchanged.
- [x] 6.6 Disable the runtime and verify no further mouse movement is remapped or injected.
- [x] 6.7 Verify absolute cursor correction does not create repeated remapping or runaway cursor movement.
- [x] 6.8 Record any limitation involving Raw Input, DirectInput, elevated targets, protected apps, captured cursors, cursor recentring, or mouse DPI calibration.
- [x] 6.9 Verify the accepted bounded correction path at representative low, medium, and high mouse DPI settings.

### Manual Verification Notes

- Accepted path: Raw Input observation plus bounded absolute cursor correction with `SetCursorPos`.
- Target used: `Streamer.bot.exe`.
- Successful behavior: smooth movement after returning from the cursor-baseline experiment, no runaway feedback loop, disable stops remapping, and tray Exit terminates `MouseShenanigans.Tray.exe`.
- Rejected paths: low-level hook suppression with relative `SendInput`, correction-delta relative injection, and cursor-position baseline remapping because manual testing showed feedback loops or violent jumps.
- DPI coverage: jitter was acceptable at tested DPI settings up to 12800 with the default `absoluteCorrectionScale` of `1.0`.
- Known follow-up: if the cursor leaves the target window while Streamer.bot remains foreground, foreground matching keeps remapping active outside the window. Consider pause-on-leave/resume-on-enter target-boundary controls in the next runtime slice.
