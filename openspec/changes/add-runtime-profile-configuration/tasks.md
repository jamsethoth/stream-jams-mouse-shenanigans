## 1. Runtime Configuration Model

- [x] 1.1 Add a runtime configuration value model for target selector, active profile name, cursor-lock default, and profile collection.
- [x] 1.2 Add validation for missing target selector, missing active profile, invalid profile collection, duplicate profile names, and invalid cursor-lock settings.
- [x] 1.3 Add a fallback configuration equivalent to the current Streamer.bot horizontal inversion proof-of-concept defaults.
- [x] 1.4 Add unit tests for valid config, absent config fallback, invalid target, missing active profile, and invalid profile collection.

## 2. Configuration File IO

- [x] 2.1 Add a per-user app data config path provider using platform-aware path APIs.
- [x] 2.2 Add UTF-8 JSON load and save boundaries for the runtime configuration document.
- [x] 2.3 Ensure missing config files fall back without requiring file creation.
- [x] 2.4 Ensure invalid startup config reports diagnostics while keeping fallback runtime configuration available.
- [x] 2.5 Add file IO seam tests using temporary directories and explicit UTF-8 encoding expectations.

## 3. Runtime Profile Updates

- [x] 3.1 Extend runtime options or runtime state so the active remapping profile can be updated after startup.
- [x] 3.2 Ensure profile selection resets movement accumulators and target re-entry state as needed.
- [x] 3.3 Add command-boundary operations for select profile and reload configuration, building on `add-runtime-hotkeys` shared commands.
- [x] 3.4 Add unit tests proving later eligible movement uses the newly selected profile while the runtime remains enabled.

## 4. Tray Profile Controls

- [x] 4.1 Add a tray profile submenu that lists loaded profile names and marks the active profile.
- [x] 4.2 Wire profile selection to the shared command boundary and refresh tray status.
- [x] 4.3 Persist selected active profile to the configuration file after tray selection.
- [x] 4.4 Add a tray reload configuration command.
- [x] 4.5 On reload failure, keep the last known good configuration active and show tray-visible error status.
- [x] 4.6 Add tray/controller tests for profile list rendering seams, profile selection, reload success, reload failure, and config-save failure where practical.

## 5. Automated Validation

- [x] 5.1 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 5.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 5.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 5.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 5.5 Run `openspec validate add-runtime-profile-configuration --strict`.

## 6. Manual Windows Proof-Of-Concept Verification

- [ ] 6.1 Start with no config file and verify the tray app still uses the Streamer.bot horizontal inversion fallback.
- [ ] 6.2 Add a valid config file with multiple profiles and verify the tray lists all profiles.
- [ ] 6.3 Switch profiles from the tray while runtime is enabled and verify later eligible movement uses the new profile.
- [ ] 6.4 Verify selected profile persists after tray restart.
- [ ] 6.5 Edit config to a valid new target/profile set, reload from tray, and verify the new configuration applies without process restart.
- [ ] 6.6 Edit config to invalid JSON, reload from tray, and verify the last known good configuration remains active with an error status.
- [ ] 6.7 Record any file permission, path, reload, or profile-switching limitation found during manual testing.
