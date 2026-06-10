## Why

The runtime proof of concept can now safely remap and constrain the cursor for a target window, but it is still controlled only through the tray menu. A streamer needs a fast, focus-independent way to toggle remapping or recover from bad cursor behavior while the target application has focus.

## What Changes

- Add Windows-only global hotkey support using standard user-session Win32 APIs.
- Add a default toggle hotkey for enabling or disabling runtime remapping.
- Add a default emergency-disable hotkey that always disables remapping and releases any cursor lock.
- Route tray clicks and hotkeys through a shared runtime command boundary so later automation can reuse the same command semantics.
- Surface hotkey registration failures through runtime/tray status without preventing tray startup.
- Defer configurable hotkey UI, profile switching hotkeys, Streamer.bot/local automation endpoints, and driver-level input handling.

## Capabilities

### New Capabilities
- `runtime-hotkeys`: Covers global hotkey registration, hotkey command dispatch, default toggle/emergency-disable behavior, failure reporting, and manual verification boundaries for Windows desktop hotkeys.

### Modified Capabilities
- `runtime-remapping-poc`: Adds hotkey-driven runtime control expectations to the existing proof-of-concept runtime and tray surface.

## Impact

- Affects the Windows adapter project with a narrow `RegisterHotKey`/`UnregisterHotKey` boundary and message dispatch seam.
- Affects the tray composition root by registering default hotkeys and routing them through shared runtime commands.
- Affects runtime control tests by covering toggle, emergency disable, cursor-lock release, registration failure, and disposal behavior through pure seams where possible.
- Depends on the completed runtime remapping and target-boundary slices: the emergency-disable command must call the existing disable path that releases cursor lock.
- Creates a reusable runtime command shape that later profile configuration and local control surface slices can extend instead of duplicating runtime state transitions.
