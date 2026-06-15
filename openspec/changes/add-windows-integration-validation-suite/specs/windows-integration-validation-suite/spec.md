## ADDED Requirements

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

### Requirement: Automated local-control safety validation
The integration suite SHALL validate safety-relevant local-control behavior through the published tray app.

#### Scenario: Empty allowlist blocks enable through local control
- **GIVEN** the tray app is running with an empty allowed-applications list
- **WHEN** the harness posts to the runtime enable endpoint
- **THEN** the runtime remains disabled
- **AND** diagnostics or status identify the safety block

#### Scenario: Allowlisted target enables through local control
- **GIVEN** the tray app is running with a target fixture application allowlisted
- **WHEN** the harness posts to the runtime enable endpoint
- **THEN** the runtime becomes enabled
- **AND** diagnostics or status identify the allowed target

#### Scenario: Foreground allowed-application capture returns promptly
- **GIVEN** the test fixture window is foreground
- **WHEN** the harness posts to the foreground allowed-application capture endpoint
- **THEN** the HTTP response returns without waiting for human confirmation
- **AND** the response status is `confirmationPending`
- **AND** the captured identity matches the fixture window

### Requirement: Interactive desktop validation
The integration suite SHALL automate foreground-window, confirmation prompt, and global-hotkey behavior when a real interactive Windows desktop session is available.

#### Scenario: Hotkey capture confirmation is accepted
- **GIVEN** the test fixture window is foreground
- **AND** the runner supports global hotkey delivery and UI Automation
- **WHEN** the harness sends the allowed-application capture hotkey and accepts the confirmation prompt
- **THEN** the fixture identity is persisted to `allowedApplications`
- **AND** runtime remapping remains disabled until explicitly enabled

#### Scenario: Hotkey capture confirmation is canceled
- **GIVEN** the test fixture window is foreground
- **AND** the runner supports global hotkey delivery and UI Automation
- **WHEN** the harness sends the allowed-application capture hotkey and cancels the confirmation prompt
- **THEN** `allowedApplications` remains unchanged

#### Scenario: Unsupported desktop automation is reported distinctly
- **GIVEN** the integration suite runs in a noninteractive Windows session
- **WHEN** interactive desktop prerequisites are checked
- **THEN** desktop-dependent tests are reported as skipped or inconclusive with a clear reason
- **AND** they are not reported as passed

### Requirement: Configured self-exit validation
The integration suite SHALL validate that configured self-exit applications cause MouseShenanigans to exit itself without manipulating the matched process.

#### Scenario: Self-exit while enabled
- **GIVEN** the tray app is running with the fixture process configured as a self-exit application
- **AND** runtime remapping is enabled
- **WHEN** the fixture process is observed
- **THEN** MouseShenanigans disables runtime remapping
- **AND** MouseShenanigans exits its own process
- **AND** the fixture process remains running
- **AND** diagnostics identify the matched self-exit entry

#### Scenario: Self-exit while disabled
- **GIVEN** the tray app is running with the fixture process configured as a self-exit application
- **AND** runtime remapping is disabled
- **WHEN** the fixture process is observed
- **THEN** MouseShenanigans exits its own process
- **AND** it does not enable runtime remapping
- **AND** the fixture process remains running

### Requirement: Non-evasive implementation scan
The integration suite SHALL include automated checks that inspect source and release artifacts for forbidden invasive or evasive behavior.

#### Scenario: Source scan finds no forbidden behavior
- **WHEN** the non-evasive scan runs
- **THEN** it checks source for drivers, services, elevated input layers, process injection, overlays, game memory reads, anti-cheat tampering, concealment behavior, game-candidate classifiers, protected-game denylists, and built-in self-exit entries
- **AND** it fails when forbidden implementation markers are found

#### Scenario: Release artifact inventory finds no forbidden behavior
- **WHEN** the publish output is inspected
- **THEN** it contains no driver artifacts, service installers, overlay injectors, or unexpected elevated helper executables
