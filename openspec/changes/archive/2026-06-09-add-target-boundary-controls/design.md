## Context

The runtime proof of concept now uses Raw Input observation plus bounded absolute cursor correction for the active horizontal-inversion profile. Manual testing showed this path is smooth enough to continue, but also exposed a target-boundary gap: if Streamer.bot remains the foreground window, remapping can continue after excessive mouse movement pushes the cursor outside the target window.

The current runtime target model only captures process name and title for the foreground window and the window under the cursor. It does not capture screen bounds, cursor containment, or a richer eligibility state. The current selector therefore answers only whether a target matched, not whether the cursor is safely inside the intended target surface.

This change keeps the standard user-session Win32 approach and tightens the runtime boundary before larger usability slices such as hotkeys, profile persistence, profile switching, or Streamer.bot automation.

## Goals / Non-Goals

**Goals:**

- Add target-window bounds and cursor containment to runtime target snapshots.
- Pause remapping while the cursor is outside the matching target window bounds, without disabling the runtime observation boundary.
- Resume remapping automatically when the cursor re-enters the matching target window.
- Add an optional cursor-lock mode that keeps the cursor constrained to the active target bounds while the target is active.
- Expose cursor lock as a minimal tray toggle, default off, suitable for manual proof-of-concept validation.
- Keep pure eligibility and lock decisions testable without a desktop session.

**Non-Goals:**

- No profile persistence, target persistence, settings UI, or runtime profile switching.
- No global hotkeys or emergency-disable hotkey registration.
- No Streamer.bot, REST, WebSocket, named-pipe, or CLI automation endpoint.
- No driver-level input layer, Windows service, elevation workflow, installer, signing, or auto-start behavior.
- No attempt to support protected, elevated, Raw Input, DirectInput, anti-cheat, or cursor-recentering applications beyond the existing proof-of-concept boundary.

## Decisions

### Represent target bounds in screen coordinates

Extend target-window reading so a target window can include a screen-space rectangle. The first implementation should use standard Win32 APIs that live in `MouseShenanigans.Windows`, such as `GetWindowRect`, and compare those bounds with the cursor position from `GetCursorPos`.

Bounds should be part of the target snapshot data passed into pure decision logic. That keeps P/Invoke in adapters and keeps target-boundary behavior unit-testable.

Alternative considered: infer containment only from `WindowFromPoint`. That works for the under-cursor case, but it cannot distinguish a matching foreground target from a cursor that has left that foreground window. Explicit bounds are needed for the known failure mode.

### Replace boolean target matching with an eligibility decision

The current selector returns a boolean. This change should introduce a richer target eligibility result that can distinguish at least:

- no matching target
- matching target with cursor inside bounds
- matching foreground target with cursor outside bounds
- matching target with unreadable bounds

The runtime should apply remapping only for the inside-bounds state. Outside-bounds and unreadable-bounds states should pass through physical movement without writing corrected cursor output.

Alternative considered: disable the runtime when the cursor leaves the target. That creates a re-entry problem because the runtime would need a separate polling mechanism to know when to re-enable. Pausing application while keeping observation active is smaller and safer.

### Fail closed when target bounds are unavailable

If a matching target window cannot provide reliable bounds, the runtime should pass through movement rather than apply remapping based on process match alone. This avoids reintroducing the exact outside-window behavior this slice is meant to remove.

Alternative considered: fall back to the old foreground-or-under-cursor behavior when bounds are unavailable. That would be more permissive, but it preserves the known bad behavior and makes safety depend on an invisible adapter failure.

### Use an opt-in cursor lock with explicit release rules

Cursor lock should be disabled by default. When enabled and the configured target is active, the runtime should constrain the cursor to the current target bounds using a narrow Windows boundary. The natural Win32 adapter for this is `ClipCursor`, though the behavior should remain behind an interface so tests can verify lock and release decisions without calling desktop APIs.

The runtime must release the cursor constraint when any of these happen:

- the user disables cursor lock
- the runtime is disabled
- the runtime is disposed
- runtime setup or movement handling fails
- the configured target no longer matches as foreground or under cursor
- target bounds are unavailable

Alternative considered: clamp only the app absolute cursor correction target. That is less invasive, but the OS cursor can still escape before the correction runs, especially under fast movement or queued Raw Input messages. A real cursor lock better addresses the user problem, as long as release behavior is strict.

### Keep the tray surface minimal

The tray app should gain one checkable command, such as `Lock cursor to target`, in the proof-of-concept menu. This is enough to manually validate lock behavior and to escape the lock by unchecking it. The tray should not become a settings surface in this slice.

Alternative considered: hard-code lock mode in `RuntimeProofOfConceptDefaults`. That would be simpler, but manual validation would require rebuilding to compare lock-on and lock-off behavior, which is poor feedback for this specific risk.

## Risks / Trade-offs

- Cursor locking affects the whole desktop cursor while active -> Keep it opt-in, visible in the tray, and always release it on disable, dispose, failure, and target loss.
- Bounds from standard Win32 APIs may include invisible borders or differ by DPI/window styles -> Use one documented screen-coordinate policy, test pure containment logic, and manually verify with the Streamer.bot target window at representative DPI settings.
- If the app crashes while the cursor is locked, the user could be temporarily constrained -> Prefer Win32-managed locking with strict release on all runtime-controlled exit paths, and keep lock disabled by default until manual testing proves it is safe enough.
- Outside-bounds pass-through means remapping stops abruptly at the target edge -> This is the safer default and lets the user move normally back into the target window.
- Target lock may make it harder to interact with other windows while the target remains active -> The tray disable and lock toggle are the escape paths for this proof of concept; global emergency hotkeys remain a later slice.
- Apps that capture or recenter the cursor may not respect this model -> Keep this within the standard user-session proof-of-concept boundary and do not introduce a driver-level approach in this slice.

## Migration Plan

1. Extend target-window snapshot data with bounds and cursor containment information.
2. Add pure target eligibility decisions for inside, outside, no match, and unreadable bounds.
3. Add a narrow cursor lock boundary and coordinator logic that applies and releases the lock according to runtime state.
4. Update the absolute cursor remapping coordinator to apply remapping only when the eligibility result is inside bounds.
5. Add the minimal tray lock toggle and status updates needed for manual proof-of-concept validation.
6. Add automated tests for target-boundary decisions, paused outside-bounds behavior, lock release behavior, and tray lock composition where practical.
7. Manually verify outside-boundary pause, re-entry, lock-on containment, lock-off release, disable release, target loss release, process exit release, and representative DPI settings.

Rollback is straightforward because the change is additive to the runtime proof-of-concept path: disable the lock toggle and restore the previous boolean target selector if boundary-aware gating proves unusable.

## Open Questions

None for implementation.

Resolved scope notes:

- Cursor lock activates whenever the configured target is active as foreground or under cursor and readable bounds are available.
- Target bounds use the full window rectangle for this slice. Client-area targeting can be considered later if Streamer.bot borders or toolbars prove problematic.
- Tray status remains coarse in this slice. The lock checkbox communicates lock state; a richer `enabled but paused outside target` status can be considered in a later UX pass.
