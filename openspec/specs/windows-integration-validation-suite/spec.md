# windows-integration-validation-suite Specification

## Purpose
TBD - created by archiving change add-windows-integration-validation-suite. Update Purpose after archive.
## Requirements
### Requirement: Windows integration harness isolation
The system SHALL provide a Windows-only integration harness that launches MouseShenanigans with isolated configuration, local-control binding, diagnostics, and timing.

#### Scenario: Harness starts isolated tray process
- **GIVEN** the integration suite is running on Windows
- **AND** the required testability seams are available
- **WHEN** the harness starts the tray app
- **THEN** the tray process uses a temporary runtime configuration path
- **AND** it binds local control to a test-selected loopback URL
- **AND** it exposes diagnostics for assertions
- **AND** cleanup stops only processes launched by the harness

#### Scenario: Missing prerequisite seams block suite execution
- **GIVEN** the required testability seams are unavailable
- **WHEN** the integration suite starts
- **THEN** affected tests fail fast or skip with a clear prerequisite message
- **AND** they do not touch the user's production configuration

### Requirement: Published tray local-control validation
The integration suite SHALL validate local-control readiness through the published tray app.

#### Scenario: Local-control status becomes ready
- **GIVEN** the tray app is running with an isolated configuration path
- **WHEN** the harness polls the local-control status endpoint
- **THEN** the endpoint returns a successful runtime snapshot
- **AND** the snapshot includes the runtime enabled state
- **AND** the snapshot does not require access to the user's production configuration

#### Scenario: Diagnostics endpoint returns stable shape
- **GIVEN** the tray app is running with diagnostics enabled
- **WHEN** the harness polls the diagnostics endpoint
- **THEN** the endpoint returns a successful diagnostics response
- **AND** the response contains an events collection

### Requirement: Desktop validation
The integration suite SHALL automate foreground-window and global-hotkey behavior when a real Windows desktop session is available.

#### Scenario: Local-control foreground target capture returns promptly
- **GIVEN** the test fixture window is foreground
- **AND** the runner supports foreground-window control
- **WHEN** the harness posts to the foreground target capture endpoint
- **THEN** the HTTP response returns a successful runtime snapshot
- **AND** the captured target identity matches the fixture window

#### Scenario: Hotkey capture persists the foreground target
- **GIVEN** the test fixture window is foreground
- **AND** the runner supports global hotkey delivery
- **WHEN** the harness sends the foreground target capture hotkey
- **THEN** the fixture identity is persisted as the runtime target
- **AND** runtime remapping remains disabled until explicitly enabled

#### Scenario: Unsupported desktop automation is reported distinctly
- **GIVEN** the integration suite runs in a Windows session without desktop prerequisites
- **WHEN** desktop prerequisites are checked
- **THEN** desktop-dependent tests are reported as skipped or inconclusive with a clear reason
- **AND** they are not reported as passed

### Requirement: Non-evasive implementation scan
The integration suite SHALL include automated checks that inspect source and release artifacts for forbidden invasive or evasive behavior.

#### Scenario: Source scan finds no forbidden behavior
- **WHEN** the non-evasive scan runs
- **THEN** it checks source for drivers, services, elevated input layers, process injection, overlays, game memory reads, anti-cheat tampering, and concealment behavior
- **AND** it fails when forbidden implementation markers are found

#### Scenario: Release artifact inventory finds no forbidden behavior
- **WHEN** the publish output is inspected
- **THEN** it contains no driver artifacts, service installers, overlay injectors, or unexpected elevated helper executables
