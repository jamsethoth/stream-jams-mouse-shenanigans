## 1. Shared Runtime Commands

- [x] 1.1 Add a small runtime command controller that wraps `IRuntimeRemappingController` enable, disable, toggle, and emergency-disable operations.
- [x] 1.2 Ensure emergency disable calls the existing runtime disable path so cursor lock and remapping state are released consistently.
- [x] 1.3 Route existing tray enable and disable menu actions through the shared command controller.
- [x] 1.4 Add unit tests for toggle from disabled, toggle from enabled, emergency disable while enabled, and emergency disable while already disabled.

## 2. Hotkey Model And Registration Boundary

- [x] 2.1 Add a `HotkeyBinding` value model that associates a semantic runtime command with modifiers and key.
- [x] 2.2 Add a default hotkey binding provider for toggle and emergency-disable commands without adding settings UI or persistence.
- [x] 2.3 Validate duplicate chords and unknown command bindings before registration.
- [x] 2.4 Add a Windows hotkey registration interface that accepts a binding collection and supports register, unregister, re-register, and dispose semantics suitable for test doubles.
- [x] 2.5 Implement the Win32 `RegisterHotKey` and `UnregisterHotKey` adapter behind the interface, keeping native IDs internal and resolving dispatch back to semantic runtime commands.
- [x] 2.6 Add unit tests for default bindings, duplicate binding rejection, registration success, partial registration failure, unregister-on-dispose, re-register behavior, and duplicate dispose safety through pure seams.

## 3. Tray Message Dispatch

- [x] 3.1 Wire the tray application context to register default hotkeys during startup.
- [x] 3.2 Dispatch toggle hotkey messages to the shared command controller and refresh tray status.
- [x] 3.3 Dispatch emergency-disable hotkey messages to the shared command controller and refresh tray status.
- [x] 3.4 Surface hotkey registration failures in tray-visible status while preserving tray menu usability.
- [x] 3.5 Add tray tests for hotkey dispatch and degraded registration status where practical without a desktop session.

## 4. Automated Validation

- [x] 4.1 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 4.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 4.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 4.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 4.5 Run `openspec validate add-runtime-hotkeys --strict`.

## 5. Manual Windows Proof-Of-Concept Verification

- [x] 5.1 Launch the tray app in a real Windows desktop session and verify startup succeeds with hotkeys registered.
- [x] 5.2 With Streamer.bot focused and runtime disabled, press the toggle hotkey and verify remapping enables.
- [x] 5.3 With Streamer.bot focused and runtime enabled, press the toggle hotkey and verify remapping disables and cursor lock releases if active.
- [x] 5.4 Enable remapping and cursor lock, press the emergency-disable hotkey, and verify remapping disables and cursor lock releases immediately.
- [x] 5.5 Press the emergency-disable hotkey while already disabled and verify status remains coherent.
- [x] 5.6 Exit the tray and verify hotkeys are released and `MouseShenanigans.Tray.exe` terminates.
- [x] 5.7 Record any hotkey conflict, focus, elevation, or unsupported-session limitation found during manual testing.

Manual validation notes:
- `Ctrl+Alt+M` conflicted with an existing registration on this machine (Win32 error 1409), so the fixed defaults were updated to `Ctrl+Alt+F8` and `Ctrl+Alt+Shift+F8`.
- Manual Windows testing confirmed the toggle hotkey, emergency-disable hotkey, and single-instance guard all behaved as expected.
