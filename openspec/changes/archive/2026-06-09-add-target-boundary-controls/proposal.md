## Why

The runtime proof of concept validated Raw Input observation plus bounded absolute cursor correction, but manual testing exposed a target-safety gap: when the target application remains foreground, remapping can continue after the cursor leaves the target window. This change tightens the Windows-only runtime boundary before adding hotkeys, profile persistence, or Streamer.bot automation, and considers an optional cursor-lock mode for sessions where remapping makes re-entry difficult.

## What Changes

- Add target-window bounds to the runtime target snapshot so the runtime can distinguish a matching foreground process from a cursor position that is actually inside the target window.
- Change target eligibility so remapping pauses when the cursor is outside the matching target window bounds, while the runtime remains enabled and observing input.
- Resume remapping automatically when the cursor re-enters the matching target window.
- Add an optional target-window cursor lock mode that constrains the cursor to the target bounds while remapping is active.
- Expose cursor locking as a minimal tray toggle, default off, rather than adding a full settings UI.
- Preserve the existing standard user-session Win32 approach; no driver, service, elevation workflow, or protected-app support is introduced.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `runtime-remapping-poc`: Target-window gating changes from foreground-or-under-cursor process matching alone to boundary-aware eligibility, with optional cursor locking while the target is active.

## Impact

- `MouseShenanigans.Windows`: target-window snapshot models, Win32 target-window reading, target eligibility decisions, absolute cursor remapping decisions, cursor lock boundaries, and runtime options.
- `MouseShenanigans.Tray`: proof-of-concept composition and a minimal tray toggle for the optional cursor-lock setting without introducing a full settings surface.
- `tests/MouseShenanigans.Windows.Tests`: new unit coverage for inside-bounds eligibility, outside-bounds pause behavior, automatic re-entry, and optional clamped cursor output.
- OpenSpec `runtime-remapping-poc` requirements and manual Windows verification steps.
