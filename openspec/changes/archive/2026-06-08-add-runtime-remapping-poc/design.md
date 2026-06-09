## Context

The repository now has a .NET app foundation with separate Core, Windows, Tray, and test projects. Core contains named remapping profiles, the horizontal inversion preset, JSON profile parsing, and a deterministic remapping engine. The remaining MVP risk is not the math; it is whether standard user-session Win32 APIs can observe ordinary mouse movement, apply remapped cursor output, and do that only for a chosen third-party application window.

This change is a proof of concept for that runtime path. It should be Windows-only, narrow, manually verifiable, and easy to remove or reshape if standard Win32 behavior is not reliable for the target application.

## Goals / Non-Goals

**Goals:**

- Add a Windows runtime coordinator that can be enabled, disabled, and disposed from the tray host.
- Observe mouse movement through standard user-session Win32 APIs, with the current tray proof of concept using Raw Input.
- Gate remapping to one configured target process name or window-title match when the target is foreground or under the cursor.
- Apply the active core remapping profile to targeted movement and write corrected cursor output through a narrow Windows boundary.
- Prevent injected movement from being remapped again.
- Keep pure decision logic testable without a Windows desktop session.
- Document manual Windows verification for mouse observation, target gating, cursor output, and feedback-loop behavior.

**Non-Goals:**

- No global hotkeys or emergency-disable hotkey registration.
- No profile file persistence, reload behavior, runtime profile switching UI, or profile editing UI.
- No Streamer.bot, REST, WebSocket, named-pipe, or CLI automation endpoint.
- No installer, signing, startup registration, elevation workflow, or auto-update behavior.
- No driver-level input layer and no attempt to support games or protected applications that reject standard user-session hooks.

## Decisions

### Use a small runtime coordinator in the Windows project

`MouseShenanigans.Windows` should own mouse observation, target-window inspection, cursor output, and runtime status. The tray app should compose and dispose the runtime, but it should not contain P/Invoke callbacks or remapping decisions.

Alternative considered: put the proof-of-concept wiring directly in `TrayApplicationContext`. That would be faster for a demo, but it would make the tray shell responsible for low-level input behavior and harder to test or replace.

### Keep Win32 APIs behind narrow adapters

Add small adapters for:

- low-level mouse hooks using `SetWindowsHookEx` with `WH_MOUSE_LL`
- target-window queries using foreground-window, point-window, text, process-id, and process-name APIs
- relative movement injection using `SendInput`
- Raw Input observation and absolute cursor positioning using `SetCursorPos`

The coordinator should depend on these adapters through focused interfaces or small internal abstractions. Pure logic such as target match decisions, enable/disable state, injected-event pass-through, absolute cursor-position decisions, and fractional delta conversion should be testable without installing a real hook.

Alternative considered: expose raw P/Invoke methods directly throughout the runtime code. That would reduce initial files, but it would make tests brittle and would hide the behavioral decisions inside callback plumbing.

### Use `SendInput` relative movement for the first adapter

Core produces remapped deltas, so relative `SendInput` movement was the closest first boundary match. The low-level hook coordinator keeps that adapter isolated so it can remain as a comparison point.

Manual testing showed relative replacement or correction movement can create horizontal feedback loops in the target app. The tray proof of concept now uses Raw Input as an observation boundary and `SetCursorPos` as an absolute output boundary: the raw physical delta is treated as input data, the current post-move cursor position is read, and the runtime writes the intended final screen position.

### Use Raw Input plus absolute cursor positioning for the active tray proof of concept

Raw Input should observe physical relative mouse deltas without suppressing the original OS cursor movement. For a matching target, the runtime remaps the raw delta, computes `correction = (remappedDelta - rawDelta) * absoluteCorrectionScale`, and applies that bounded correction to the current cursor position with `SetCursorPos`. When the active profile preserves an axis, the correction for that axis is zero and no cursor write is needed. The default scale is `1.0`; the explicit scale exists so different mouse DPI and pointer-sensitivity setups can be calibrated without changing the remapping profile math.

A cursor-baseline variant was tested after noticing fast-movement jitter: it tracked the previous cursor position, remapped `currentCursor - previousCursor`, and wrote `previousCursor + remappedScreenDelta`. Manual testing showed that model could jump violently during fast movement, likely because queued Raw Input messages and absolute cursor writes are not synchronized tightly enough. The accepted implementation path therefore keeps correction bounded to raw input magnitude, uses an explicit calibration scale for mouse DPI differences, and accepts standard-user-session limitations rather than moving to a driver-level approach in this slice.

Alternative considered: keep using low-level hook suppression and relative `SendInput` correction. Manual testing showed that path can feed the target a rapid left/right fight, so the absolute cursor path is the next smallest experiment before considering driver-level input.

### Treat target configuration as proof-of-concept options

This slice should introduce an options object with one configured process-name match or window-title contains match, plus one active profile. The tray host can construct this from hard-coded values or simple constants for now. File locations, reload behavior, user profile selection, and automation command contracts are separate slices.

Alternative considered: add JSON file persistence now. That would make the demo easier to tune, but it would expand scope into configuration lifecycle decisions before the runtime API has proven useful.

### Pass through injected hook events

Low-level hook events that are injected, including this app's own `SendInput` events, should not be remapped. The proof of concept should use the low-level hook injected flag plus a local injection guard to prevent immediate self-remapping while allowing the injected movement to reach the target.

Alternative considered: suppress injected events that appear to match the last output. That risks blocking the app's own correction movement and makes manual behavior harder to reason about.

### Keep the tray controls minimal

The tray app should expose enough manual control to start and stop the runtime, exit the tray message loop, and show a coarse status label. It should remain a proof-of-concept shell, not a settings UI.

Because the app runs as a tray-only `ApplicationContext`, the Exit menu item should call the context shutdown path (`ExitThread`) rather than process-wide `Application.Exit()`. Manual testing showed `Application.Exit()` can leave the tray process alive because there is no main form to close. The shutdown path should hide the tray icon, dispose the runtime, and then request the message loop exit.

Alternative considered: build a richer tray menu for profile selection and target configuration. That belongs after hook/injection reliability is known.

## Risks / Trade-offs

- Standard hooks may not affect Raw Input, DirectInput, elevated, or protected applications -> Make this a manual proof of concept and keep driver-level approaches out of scope unless the result proves they are needed.
- Low-level suppression and relative injection may create target-visible feedback loops -> Keep that adapter isolated and route the active tray proof of concept through Raw Input plus absolute cursor positioning.
- `SetCursorPos` may still jitter or be interpreted poorly by apps that capture/recenter cursors -> Keep absolute correction bounded to raw input magnitude, expose correction scale for mouse DPI calibration, record remaining jitter explicitly in manual verification, and consider a lower-level input boundary if this is not acceptable.
- Fractional core output must become integer input movement -> Convert at the Windows boundary and keep any remainder policy local and testable.
- Low-level hook callbacks can hurt desktop responsiveness -> Keep callback work minimal and delegate only small, deterministic decisions.
- Target matching by title or process name can be ambiguous -> Support one configured match for the proof of concept and avoid multi-target selection in this slice.
- Foreground-target matching can keep remapping active after the cursor leaves the target window bounds -> Accept for this proof of concept, record it as a follow-up control need, and consider target-boundary gating in a later slice.

## Migration Plan

1. Add Windows runtime abstractions and pure decision tests.
2. Add Win32 hook, Raw Input, target-window, relative injection, and absolute cursor-position adapters.
3. Compose the runtime from the tray app with hard-coded proof-of-concept options, using the Raw Input plus absolute cursor path for the active manual experiment.
4. Add a small tray menu/status surface for enable and disable.
5. Run normal restore, format, build, and test validation.
6. Manually verify mouse observation registration, target gating, cursor output, feedback-loop behavior, disable behavior, non-target pass-through, process exit, and representative mouse DPI settings in a real Windows desktop session.

Rollback is straightforward because this change is additive: disable runtime composition in the tray app or revert the Windows runtime files.

## Open Questions

- What user-facing configuration surface should expose `absoluteCorrectionScale` after the proof of concept moves beyond hard-coded options?
- Should the next runtime slice add target-boundary controls that pause remapping when the cursor leaves the target window and resume it when the cursor re-enters?
- If Raw Input plus bounded `SetCursorPos` correction still jitters on some hardware or protected targets, should a later slice try a lower-level input boundary before adding hotkeys and automation?
