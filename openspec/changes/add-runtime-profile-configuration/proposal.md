## Why

The runtime still hard-codes `Streamer.bot.exe` and the built-in horizontal inversion profile even though the core model already supports named JSON remapping profiles. The next useful app slice is to load target/profile configuration from disk and let the user switch active profiles at runtime without editing code or restarting the tray app.

## What Changes

- Add a Windows tray runtime configuration file loaded from a deterministic per-user app data path.
- Define a JSON configuration shape for target selection, active profile, cursor-lock default, and named remapping profiles.
- Keep a built-in fallback configuration equivalent to today's proof of concept when no user config exists.
- Allow the tray app to show available profiles and switch the active profile while the runtime is running.
- Add a tray reload command for re-reading the profile configuration after the user edits JSON.
- Persist active-profile selection when changed from the tray.
- Defer a full settings editor, profile editing UI, hotkey customization, and local automation endpoints.

## Capabilities

### New Capabilities
- `runtime-profile-configuration`: Covers config file location, JSON runtime configuration loading, fallback defaults, active profile selection, tray profile menu behavior, profile reload, persistence of selected profile, and validation/error reporting.

### Modified Capabilities
- `runtime-remapping-poc`: Changes the proof-of-concept runtime from hard-coded target/profile defaults to runtime-loaded target and profile configuration.

## Impact

- Affects core/runtime composition by turning `RuntimeProofOfConceptDefaults` into fallback configuration rather than the only possible runtime options.
- Affects Windows runtime coordinators by requiring active profile updates without rebuilding the whole tray process.
- Affects tray UI with a profile submenu, reload command, active-profile status, and config error reporting.
- Depends on the existing `remapping-profiles` capability for profile validation and JSON profile parsing concepts.
- Depends on `add-runtime-hotkeys` for the shared runtime command boundary; if implemented before hotkeys lands, this change should either wait or include a temporary command abstraction that is reconciled with the hotkey slice before archive.
- Provides required profile selection/reload behavior for the later `add-local-control-surface` slice.
