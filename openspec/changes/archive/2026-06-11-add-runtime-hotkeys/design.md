## Context

The current tray app can enable, disable, and lock the cursor through tray menu commands. Manual validation showed the runtime can recover from target-boundary edge cases, but a tray-only control path is awkward when the target application has focus or cursor behavior becomes uncomfortable. The project README and OpenSpec config both list toggle and emergency-disable hotkeys as MVP safety controls.

This change follows the existing Windows boundary style: native Win32 calls stay in `MouseShenanigans.Windows`, while the tray project composes them into the WinForms message loop. It depends on the completed runtime remapping and target-boundary slices because the emergency-disable behavior must reuse the runtime disable path that releases cursor lock and stops remapping.

## Goals / Non-Goals

**Goals:**
- Add a default global toggle hotkey for runtime enable/disable.
- Add a default global emergency-disable hotkey that disables runtime remapping and releases any active cursor lock.
- Keep hotkey registration Windows-only and based on standard user-session APIs.
- Route tray commands and hotkey commands through one in-process runtime command boundary.
- Represent hotkeys as semantic command bindings so future app configuration can replace the default binding source without changing registration or dispatch behavior.
- Report hotkey registration failure through tray-visible status while keeping the tray app usable.
- Cover command dispatch and registration lifecycle through unit-testable seams where possible.

**Non-Goals:**
- No configurable hotkey UI or persisted hotkey settings.
- No profile switching hotkeys.
- No Streamer.bot/local automation endpoint.
- No elevated input layer, driver, or Windows service.
- No automated desktop-session test harness for real global hotkey delivery.

## Decisions

### Use `RegisterHotKey` with the tray message loop
Use the standard Win32 `RegisterHotKey`/`UnregisterHotKey` API and dispatch `WM_HOTKEY` from the tray app's message loop. This matches the user-session utility constraint and avoids low-level keyboard hooks for simple global shortcuts.

Alternative considered: a low-level keyboard hook. That would be more flexible but increases risk, permissions sensitivity, and feedback-loop concerns. For two fixed commands, registered hotkeys are the smaller and safer boundary.

### Introduce a runtime command controller
Add a small command controller above `IRuntimeRemappingController` with operations such as `Enable`, `Disable`, `Toggle`, and `EmergencyDisable`. Tray menu handlers and hotkey handlers should call this controller instead of each encoding runtime state transitions separately.

This prepares later slices: profile configuration can add profile selection/reload commands, and the local control surface can expose the same commands over localhost without duplicating runtime state logic.

### Keep default hotkeys fixed, but model them as bindings
Use documented fixed defaults for this slice. The implementation should define hotkeys as data, for example a `HotkeyBinding` containing a semantic runtime command, modifiers, and key. The registrar should accept a collection of bindings rather than owning hard-coded key constants.

Add a default binding provider for this slice. Do not add settings UI, file-backed hotkey configuration, or persistence yet, but keep the provider boundary narrow enough that a later app-config slice can replace the default provider with a config-backed provider.

Dispatch should be keyed by semantic runtime command names, not by raw registration IDs or key chords. The registration boundary can allocate native hotkey IDs internally, but the tray message dispatch should resolve them back to commands such as `ToggleRuntime` and `EmergencyDisable`.

Validate duplicate chords and unknown command bindings before registration, even while defaults are the only source. The registration lifecycle should also be able to unregister and re-register a complete binding set later so config reload can swap hotkeys without rebuilding the tray process.

Recommended defaults are `Ctrl+Alt+F8` for toggle and `Ctrl+Alt+Shift+F8` for emergency disable. Manual validation found `Ctrl+Alt+M` unavailable with Win32 error 1409 (`Hot key is already registered.`), so this slice avoids the observed conflict while keeping fixed, non-configurable defaults.

Manual Windows validation later confirmed that the `Ctrl+Alt+F8` toggle hotkey, the `Ctrl+Alt+Shift+F8` emergency-disable hotkey, and the tray single-instance guard all behaved correctly in a real desktop session.

### Treat registration failure as degraded startup
If a default hotkey cannot register, the tray app should keep running and report degraded status. Existing tray menu controls remain usable. Disposal should unregister any hotkey that did register successfully.

## Risks / Trade-offs

- Hotkey conflicts with another app -> Report degraded status and keep tray controls usable; document the limitation for this non-configurable slice.
- Running outside an interactive Windows desktop session -> Hotkey registration fails or is skipped; status should make this visible without crashing startup.
- Duplicate command paths drift over time -> Centralize tray and hotkey behavior in one runtime command controller.
- Future configurable hotkeys require reload semantics -> Keep the binding provider and registration lifecycle separable so a later config-backed provider can atomically replace registered bindings.
- Manual validation is still required -> Unit tests can cover dispatch decisions and registration lifecycle, but real global hotkey delivery needs a Windows desktop session.
