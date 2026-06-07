## Context

The repository now has a .NET app foundation with separate Core, Windows, Tray, and test projects. Core contains named remapping profiles, the horizontal inversion preset, JSON profile parsing, and a deterministic remapping engine. The remaining MVP risk is not the math; it is whether standard user-session Win32 APIs can intercept ordinary mouse movement, suppress the original movement, inject remapped movement, and do that only for a chosen third-party application window.

This change is a proof of concept for that runtime path. It should be Windows-only, narrow, manually verifiable, and easy to remove or reshape if standard Win32 behavior is not reliable for the target application.

## Goals / Non-Goals

**Goals:**

- Add a Windows runtime coordinator that can be enabled, disabled, and disposed from the tray host.
- Observe mouse movement through `WH_MOUSE_LL`.
- Gate remapping to one configured target process name or window-title match when the target is foreground or under the cursor.
- Suppress targeted original movement, apply the active core remapping profile, and inject corrected relative movement.
- Prevent injected movement from being remapped again.
- Keep pure decision logic testable without a Windows desktop session.
- Document manual Windows verification for hook, target, injection, and feedback-loop behavior.

**Non-Goals:**

- No global hotkeys or emergency-disable hotkey registration.
- No profile file persistence, reload behavior, runtime profile switching UI, or profile editing UI.
- No Streamer.bot, REST, WebSocket, named-pipe, or CLI automation endpoint.
- No installer, signing, startup registration, elevation workflow, or auto-update behavior.
- No driver-level input layer and no attempt to support games or protected applications that reject standard user-session hooks.

## Decisions

### Use a small runtime coordinator in the Windows project

`MouseShenanigans.Windows` should own hook installation, target-window inspection, input injection, and runtime status. The tray app should compose and dispose the runtime, but it should not contain P/Invoke callbacks or remapping decisions.

Alternative considered: put the proof-of-concept wiring directly in `TrayApplicationContext`. That would be faster for a demo, but it would make the tray shell responsible for low-level input behavior and harder to test or replace.

### Keep Win32 APIs behind narrow adapters

Add small adapters for:

- low-level mouse hooks using `SetWindowsHookEx` with `WH_MOUSE_LL`
- target-window queries using foreground-window, point-window, text, process-id, and process-name APIs
- relative movement injection using `SendInput`

The coordinator should depend on these adapters through focused interfaces or small internal abstractions. Pure logic such as target match decisions, enable/disable state, injected-event pass-through, and fractional delta conversion should be testable without installing a real hook.

Alternative considered: expose raw P/Invoke methods directly throughout the runtime code. That would reduce initial files, but it would make tests brittle and would hide the behavioral decisions inside callback plumbing.

### Use `SendInput` relative movement for injected deltas

Core produces remapped deltas, so relative `SendInput` movement is the closest boundary match. The runtime should suppress the original targeted movement and inject the remapped relative movement.

Alternative considered: use `SetCursorPos` with absolute cursor coordinates. That may be useful if relative injection behaves poorly for a target application, but it mixes cursor-position tracking with output application and is less direct for the current core model.

### Treat target configuration as proof-of-concept options

This slice should introduce an options object with one configured process-name match or window-title contains match, plus one active profile. The tray host can construct this from hard-coded values or simple constants for now. File locations, reload behavior, user profile selection, and automation command contracts are separate slices.

Alternative considered: add JSON file persistence now. That would make the demo easier to tune, but it would expand scope into configuration lifecycle decisions before the runtime API has proven useful.

### Pass through injected hook events

Low-level hook events that are injected, including this app's own `SendInput` events, should not be remapped. The proof of concept should use the low-level hook injected flag plus a local injection guard to prevent immediate self-remapping while allowing the injected movement to reach the target.

Alternative considered: suppress injected events that appear to match the last output. That risks blocking the app's own correction movement and makes manual behavior harder to reason about.

### Keep the tray controls minimal

The tray app should expose enough manual control to start and stop the runtime and show a coarse status label. It should remain a proof-of-concept shell, not a settings UI.

Alternative considered: build a richer tray menu for profile selection and target configuration. That belongs after hook/injection reliability is known.

## Risks / Trade-offs

- Standard hooks may not affect Raw Input, DirectInput, elevated, or protected applications -> Make this a manual proof of concept and keep driver-level approaches out of scope unless the result proves they are needed.
- Suppressing the original event may not prevent all target-visible movement in every app -> Capture this explicitly in manual verification instead of hiding it behind a more complex abstraction.
- `SendInput` may be rejected or interpreted differently by some targets -> Keep injection behind an adapter so a later slice can compare `SetCursorPos` or another boundary.
- Fractional core output must become integer input movement -> Convert at the Windows boundary and keep any remainder policy local and testable.
- Low-level hook callbacks can hurt desktop responsiveness -> Keep callback work minimal and delegate only small, deterministic decisions.
- Target matching by title or process name can be ambiguous -> Support one configured match for the proof of concept and avoid multi-target selection in this slice.

## Migration Plan

1. Add Windows runtime abstractions and pure decision tests.
2. Add Win32 hook, target-window, and injection adapters.
3. Compose the runtime from the tray app with hard-coded proof-of-concept options.
4. Add a small tray menu/status surface for enable and disable.
5. Run normal restore, format, build, and test validation.
6. Manually verify hook, target gating, injection, feedback-loop behavior, disable behavior, and non-target pass-through in a real Windows desktop session.

Rollback is straightforward because this change is additive: disable runtime composition in the tray app or revert the Windows runtime files.

## Open Questions

- Which specific third-party target process or window title should be the first manual verification target?
- Should the first manual test prefer foreground-window matching, under-cursor matching, or require either one to match?
- If relative `SendInput` works only partially, should the next slice compare `SetCursorPos` before adding hotkeys and automation?
