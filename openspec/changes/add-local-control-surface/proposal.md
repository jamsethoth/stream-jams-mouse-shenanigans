## Why

The tray app can become useful manually, but Streamer.bot and other streaming automation tools need a scriptable local way to enable, disable, toggle, select profiles, reload configuration, and inspect status during a stream. A minimal loopback control surface unlocks automation without turning the app into a public remote service.

## What Changes

- Add a localhost-only HTTP JSON control surface hosted by the tray process.
- Expose commands for status, enable, disable, toggle, emergency disable, select profile, and reload configuration.
- Route all commands through the shared in-process runtime command boundary established by previous slices.
- Return simple JSON responses that Streamer.bot actions can parse reliably.
- Report listener startup failures through tray-visible status while keeping manual tray controls usable.
- Defer WebSockets, remote access, authentication beyond loopback binding, profile editing endpoints, and public API compatibility guarantees.

## Capabilities

### New Capabilities
- `local-control-surface`: Covers the localhost HTTP listener, command endpoints, JSON response contract, command dispatch, listener lifecycle, failure reporting, and Streamer.bot-oriented manual verification.

### Modified Capabilities
- `runtime-profile-configuration`: Exposes profile selection and configuration reload behavior through the local control surface.
- `runtime-remapping-poc`: Exposes enable, disable, toggle, emergency-disable, and status behavior through the local control surface.

## Impact

- Affects tray composition by hosting a local HTTP listener for the lifetime of the tray process.
- Affects runtime command handling by exposing existing command-boundary operations through HTTP endpoints.
- Affects profile configuration by allowing profile selection and reload to be invoked externally.
- Depends on `add-runtime-hotkeys` for the shared runtime command boundary.
- Depends on `add-runtime-profile-configuration` for profile list, active profile, select profile, and reload configuration semantics.
- Treats Streamer.bot integration as local automation only; the listener must bind to loopback and must not expose a public remote API.
