# Local Control Surface

## Purpose

Define the localhost-only HTTP JSON control surface hosted by the tray process for local streaming automation tools.

## Requirements

### Requirement: Local HTTP listener lifecycle
The system SHALL host a localhost-only HTTP JSON control surface for the lifetime of the tray process when listener startup succeeds.

#### Scenario: Listener starts on loopback
- **GIVEN** the tray app starts in a supported desktop session
- **WHEN** the local control listener can bind its configured or default URL
- **THEN** the listener accepts requests only from loopback addresses
- **AND** tray-visible status can identify that local control is available

#### Scenario: Listener startup fails
- **GIVEN** the tray app starts
- **WHEN** the local control listener cannot bind its configured or default URL
- **THEN** the tray app keeps running
- **AND** tray and hotkey controls remain usable
- **AND** tray-visible status reports the local control listener failure

#### Scenario: Listener stops on tray exit
- **GIVEN** the local control listener is running
- **WHEN** the tray app exits
- **THEN** the listener stops accepting requests
- **AND** runtime disposal still releases remapping and cursor-lock resources

### Requirement: Runtime command endpoints
The system SHALL expose local HTTP endpoints for status, enable, disable, toggle, emergency-disable, and foreground target capture commands.

#### Scenario: Status endpoint returns runtime snapshot
- **GIVEN** the local control listener is running
- **WHEN** a client requests `GET /api/v1/status`
- **THEN** the response is JSON
- **AND** it includes runtime enabled state, cursor-lock setting, active profile, available profiles, and any degraded status message available to the tray
- **AND** it includes the current target display name when runtime configuration is available

#### Scenario: Enable endpoint enables runtime
- **GIVEN** the local control listener is running
- **WHEN** a client posts to `/api/v1/runtime/enable`
- **THEN** the shared runtime command boundary enables the runtime
- **AND** the response is a successful JSON runtime snapshot

#### Scenario: Disable endpoint disables runtime
- **GIVEN** the local control listener is running
- **WHEN** a client posts to `/api/v1/runtime/disable`
- **THEN** the shared runtime command boundary disables the runtime
- **AND** any active cursor lock is released
- **AND** the response is a successful JSON runtime snapshot

#### Scenario: Toggle endpoint toggles runtime
- **GIVEN** the local control listener is running
- **WHEN** a client posts to `/api/v1/runtime/toggle`
- **THEN** the shared runtime command boundary toggles the runtime enabled state
- **AND** the response is a successful JSON runtime snapshot

#### Scenario: Emergency disable endpoint disables runtime
- **GIVEN** the local control listener is running
- **WHEN** a client posts to `/api/v1/runtime/emergency-disable`
- **THEN** the shared runtime command boundary emergency-disables the runtime
- **AND** any active cursor lock is released
- **AND** the response is a successful JSON runtime snapshot

#### Scenario: Capture foreground target endpoint retargets runtime
- **GIVEN** the local control listener is running
- **AND** the foreground window identity is readable
- **WHEN** a client posts to `/api/v1/target/capture-foreground`
- **THEN** the shared runtime command boundary captures the foreground window as the current target
- **AND** the response is a successful JSON runtime snapshot containing the new target display name

#### Scenario: Capture foreground target endpoint reports capture failure
- **GIVEN** the local control listener is running
- **AND** no foreground window identity is available
- **WHEN** a client posts to `/api/v1/target/capture-foreground`
- **THEN** the response is JSON with `ok` set to `false`
- **AND** the active runtime target is unchanged

### Requirement: Profile and configuration endpoints
The system SHALL expose local HTTP endpoints for profile listing, active profile selection, and configuration reload after runtime profile configuration exists.

#### Scenario: Profiles endpoint lists profiles
- **GIVEN** the local control listener is running
- **WHEN** a client requests `GET /api/v1/profiles`
- **THEN** the response is JSON
- **AND** it includes available profile names and the active profile name

#### Scenario: Select profile endpoint selects an existing profile
- **GIVEN** the local control listener is running
- **AND** a profile named `horizontal-inversion` is loaded
- **WHEN** a client posts `{ "name": "horizontal-inversion" }` to `/api/v1/profiles/select`
- **THEN** the shared profile command boundary selects that profile
- **AND** the response is a successful JSON runtime snapshot

#### Scenario: Select profile endpoint rejects missing profile
- **GIVEN** the local control listener is running
- **WHEN** a client posts a profile name that is not loaded to `/api/v1/profiles/select`
- **THEN** the response is JSON with `ok` set to `false`
- **AND** the active runtime profile is unchanged

#### Scenario: Reload endpoint reloads configuration
- **GIVEN** the local control listener is running
- **WHEN** a client posts to `/api/v1/config/reload`
- **THEN** the shared profile command boundary reloads runtime configuration
- **AND** the response reports success or validation failure as JSON

### Requirement: Local control verification boundary
The system SHALL cover local control routing and command dispatch with automated tests and SHALL require manual Streamer.bot-oriented verification for external tool integration.

#### Scenario: Automated local control tests do not require Streamer.bot
- **GIVEN** automated tests run
- **WHEN** local control tests execute
- **THEN** they validate endpoint routing, JSON response shape, command dispatch, target capture handling, invalid profile handling, and listener lifecycle without requiring Streamer.bot

#### Scenario: Streamer.bot integration is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** verification covers calling enable, disable, toggle, emergency disable, capture foreground target, select profile, reload configuration, and status endpoints from Streamer.bot or equivalent local HTTP tooling
