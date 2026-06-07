## 1. Runtime Decision Model

- [ ] 1.1 Add runtime options for one target match and one active `RemappingProfile`.
- [ ] 1.2 Add runtime status and lifecycle abstractions for disabled, enabled, unsupported, and failed states.
- [ ] 1.3 Add target-match decision logic for foreground-window and under-cursor process-name or title matching.
- [ ] 1.4 Add mouse-event decision logic that distinguishes physical movement, injected movement, target matches, and pass-through outcomes.
- [ ] 1.5 Add deterministic integer movement conversion at the Windows boundary for remapped `double` deltas.

## 2. Win32 Runtime Boundaries

- [ ] 2.1 Add a low-level mouse hook adapter using `SetWindowsHookEx` with `WH_MOUSE_LL` and safe unhook disposal.
- [ ] 2.2 Add a target-window adapter for foreground window, window under cursor, window title, process id, and process name lookup.
- [ ] 2.3 Add a relative movement injection adapter using `SendInput`.
- [ ] 2.4 Keep P/Invoke types and constants scoped to `MouseShenanigans.Windows`.
- [ ] 2.5 Report hook or injection setup failures through runtime status instead of leaving the runtime half-enabled.

## 3. Runtime Remapping Coordinator

- [ ] 3.1 Implement a coordinator that enables, disables, and disposes the hook and adapters.
- [ ] 3.2 On eligible target movement, compute raw deltas, apply the active core remapping profile, suppress the original event, and request replacement movement injection.
- [ ] 3.3 Pass through non-target movement without injecting replacement movement.
- [ ] 3.4 Pass through injected movement without applying the remapping profile again.
- [ ] 3.5 Keep hook callback work minimal and move deterministic decisions into testable methods.

## 4. Tray Proof-of-Concept Integration

- [ ] 4.1 Wire the tray host to construct runtime proof-of-concept options using the built-in horizontal inversion profile and one localized target configuration.
- [ ] 4.2 Add minimal tray commands to enable and disable the proof-of-concept runtime.
- [ ] 4.3 Update tray text or menu state to show disabled, enabled, unsupported, or failed runtime status.
- [ ] 4.4 Dispose the runtime when the tray app exits.
- [ ] 4.5 Keep global hotkeys, profile switching UI, profile persistence, and external automation endpoints out of this slice.

## 5. Automated Validation

- [ ] 5.1 Add unit tests for target-match decisions without calling real Win32 APIs.
- [ ] 5.2 Add unit tests for runtime remapping decisions: disabled pass-through, target pass-through, target remap, zero-output suppression, and injected-event pass-through.
- [ ] 5.3 Add unit tests for integer movement conversion at the Windows boundary.
- [ ] 5.4 Run `dotnet restore MouseShenanigans.slnx`.
- [ ] 5.5 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [ ] 5.6 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [ ] 5.7 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.

## 6. Manual Windows Proof-of-Concept Verification

- [ ] 6.1 In a real Windows desktop session, launch the tray app and verify the runtime starts disabled.
- [ ] 6.2 Enable the runtime and verify the low-level hook installs without blocking ordinary desktop input.
- [ ] 6.3 With the configured target foreground, verify horizontal movement is inverted and vertical movement is preserved.
- [ ] 6.4 With the configured target under the cursor, verify remapping applies according to the target gate.
- [ ] 6.5 With a non-target window active and under the cursor, verify ordinary movement passes through unchanged.
- [ ] 6.6 Disable the runtime and verify no further mouse movement is remapped or injected.
- [ ] 6.7 Verify injected replacement movement does not create repeated remapping or runaway cursor movement.
- [ ] 6.8 Record any limitation involving Raw Input, DirectInput, elevated targets, protected apps, captured cursors, or cursor recentring.
