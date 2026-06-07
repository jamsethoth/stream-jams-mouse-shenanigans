## Why

The README's core remapping model is the smallest useful behavior that can be made real before risky Windows mouse hooks or input injection are introduced. Adding pure-core remapping profiles now gives later tray, target-window, hotkey, and Streamer.bot integration work a stable contract to drive and test.

## What Changes

- Add a pure-core profile model for named directional remapping profiles.
- Add directional output-vector mappings for left, right, up, and down movement components.
- Add a remapping engine that transforms raw mouse delta input into output delta values using an active profile.
- Add a built-in horizontal inversion preset as the first target behavior.
- Add JSON profile document parsing and validation for named profiles without adding runtime file persistence yet.
- Add unit coverage for representative remapping behavior and invalid profile definitions.
- Defer Windows mouse hooks, input injection, target-window gating, tray profile controls, hotkeys, and local automation endpoints.

## Capabilities

### New Capabilities

- `remapping-profiles`: Covers named directional remapping profiles, profile validation, built-in presets, JSON profile document parsing, and pure-core transformation of raw mouse deltas into remapped output deltas.

### Modified Capabilities

- None.

## Impact

- Affected code will primarily be `MouseShenanigans.Core` and `MouseShenanigans.Core.Tests`.
- The tray shell, Windows adapter, CI workflow, and app-foundation spec are not expected to change except through normal build/test coverage.
- No breaking changes are expected because current core behavior only exposes directional delta decomposition.
