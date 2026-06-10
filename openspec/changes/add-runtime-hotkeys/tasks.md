## 1. Shared Runtime Commands

- [ ] 1.1 Add a small runtime command controller that wraps `IRuntimeRemappingController` enable, disable, toggle, and emergency-disable operations.
- [ ] 1.2 Ensure emergency disable calls the existing runtime disable path so cursor lock and remapping state are released consistently.
- [ ] 1.3 Route existing tray enable and disable menu actions through the shared command controller.
- [ ] 1.4 Add unit tests for toggle from disabled, toggle from enabled, emergency disable while enabled, and emergency disable while already disabled.

## 2. Hotkey Model And Registration Boundary

- [ ] 2.1 Add a `HotkeyBinding` value model that associates a semantic runtime command with modifiers and key.
- [ ] 2.2 Add a default hotkey binding provider for toggle and emergency-disable commands without adding settings UI or persistence.
- [ ] 2.3 Validate duplicate chords and unknown command bindings before registration.
- [ ] 2.4 Add a Windows hotkey registration interface that accepts a binding collection and supports register, unregister, re-register, and dispose semantics suitable for test doubles.
- [ ] 2.5 Implement the Win32 `RegisterHotKey` and `UnregisterHotKey` adapter behind the interface, keeping native IDs internal and resolving dispatch back to semantic runtime commands.
- [ ] 2.6 Add unit tests for default bindings, duplicate binding rejection, registration success, partial registration failure, unregister-on-dispose, re-register behavior, and duplicate dispose safety through pure seams.

## 3. Tray Message Dispatch

- [ ] 3.1 Wire the tray application context to register default hotkeys during startup.
- [ ] 3.2 Dispatch toggle hotkey messages to the shared command controller and refresh tray status.
- [ ] 3.3 Dispatch emergency-disable hotkey messages to the shared command controller and refresh tray status.
- [ ] 3.4 Surface hotkey registration failures in tray-visible status while preserving tray menu usability.
- [ ] 3.5 Add tray tests for hotkey dispatch and degraded registration status where practical without a desktop session.

## 4. Automated Validation

- [ ] 4.1 Run `dotnet restore MouseShenanigans.slnx`.
- [ ] 4.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [ ] 4.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [ ] 4.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [ ] 4.5 Run `openspec validate add-runtime-hotkeys --strict`.

## 5. Manual Windows Proof-Of-Concept Verification

- [ ] 5.1 Launch the tray app in a real Windows desktop session and verify startup succeeds with hotkeys registered.
- [ ] 5.2 With Streamer.bot focused and runtime disabled, press the toggle hotkey and verify remapping enables.
- [ ] 5.3 With Streamer.bot focused and runtime enabled, press the toggle hotkey and verify remapping disables and cursor lock releases if active.
- [ ] 5.4 Enable remapping and cursor lock, press the emergency-disable hotkey, and verify remapping disables and cursor lock releases immediately.
- [ ] 5.5 Press the emergency-disable hotkey while already disabled and verify status remains coherent.
- [ ] 5.6 Exit the tray and verify hotkeys are released and `MouseShenanigans.Tray.exe` terminates.
- [ ] 5.7 Record any hotkey conflict, focus, elevation, or unsupported-session limitation found during manual testing.
