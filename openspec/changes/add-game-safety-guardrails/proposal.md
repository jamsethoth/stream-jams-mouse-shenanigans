## Why

The runtime can observe mouse input and alter cursor behavior, which is anti-cheat-sensitive even when it is intended for viewer-controlled streaming chaos rather than cheating. The app needs fail-closed safety controls so it cannot be casually enabled against games unless the user has explicitly allowed that game locally, and so it exits itself when non-allowed or protected game processes appear.

## What Changes

- Add a deny-by-default game safety policy with an empty user-managed allowlist for game targets.
- Add a protected-game denylist concept for known anti-cheat-protected or online competitive titles, with block or self-exit behavior when detected.
- Add pre-enable safety checks for tray commands, hotkey toggles, and later local automation commands before any mouse observation boundary is armed.
- Add a runtime safety sentinel that disables remapping, releases cursor lock, unregisters input observation, and exits the tray process when a disallowed game process is detected.
- Add tray-visible safety status so blocked enable attempts and auto-exit reasons are diagnosable.
- Keep the app transparent and non-evasive: no drivers, services, process injection, overlays, memory reads, stealth behavior, or anti-cheat bypass techniques.
- Defer a graphical allowlist editor, cloud-managed denylist updates, and publisher-specific anti-cheat integrations.

## Capabilities

### New Capabilities
- `game-safety-guardrails`: Covers the local game allowlist, protected-game denylist policy, fail-closed enable gating, runtime process monitoring, self-exit behavior, safety status, and non-evasive operating constraints.

### Modified Capabilities
- `runtime-remapping-poc`: Changes runtime enable/toggle behavior so remapping cannot be armed unless game safety checks allow the current target and no denied game process is detected.

## Impact

- Affects Windows tray command routing for enable, toggle, hotkey toggle, emergency disable, and exit behavior.
- Affects runtime lifecycle by adding a safety decision before mouse observation starts and a live monitor that can force disable and self-exit.
- Adds local configuration for an initially empty user game allowlist and a built-in protected-game denylist.
- Adds process/window inspection code for game-safety decisions, reusing existing target-window identity patterns where practical.
- Adds unit tests for pure safety policy decisions and command gating, plus manual Windows validation for process-launch auto-exit behavior.
- Should integrate with the runtime configuration file from `add-runtime-profile-configuration` if that change lands first; otherwise this change may introduce a small safety configuration boundary that is reconciled during archive.
