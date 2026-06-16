# integration-testability-seams Specification

## Purpose
TBD - created by archiving change add-integration-testability-seams. Update Purpose after archive.
## Requirements
### Requirement: Isolated runtime test overrides
The system SHALL provide explicit startup overrides that allow validation runs to isolate runtime configuration path, local-control URL, diagnostics output, and self-exit sentinel interval without changing production defaults.

#### Scenario: Tray starts with isolated overrides
- **GIVEN** the tray app is launched with supported test override values
- **WHEN** the tray app starts
- **THEN** runtime configuration is read from the overridden configuration path
- **AND** local control binds to the overridden loopback URL
- **AND** diagnostics are available through the configured validation surface
- **AND** the self-exit sentinel uses the overridden interval

#### Scenario: Tray starts without overrides
- **GIVEN** no test override values are configured
- **WHEN** the tray app starts
- **THEN** runtime configuration, local control, diagnostics, and self-exit timing use production defaults

#### Scenario: Invalid overrides fail visibly
- **GIVEN** a test override value is invalid
- **WHEN** the tray app starts or loads the affected subsystem
- **THEN** the tray app reports a visible diagnostic or degraded status
- **AND** it does not silently mutate the user's production configuration

### Requirement: Bounded diagnostic event history
The system SHALL keep a bounded local diagnostic event history for validation-relevant configuration, local-control, safety, confirmation, and self-exit events.

#### Scenario: Validation event is recorded
- **GIVEN** the tray app is running
- **WHEN** a safety-blocked enable attempt, foreground confirmation request, confirmation completion, local-control startup failure, or self-exit request occurs
- **THEN** a diagnostic event records the event type, timestamp, summary message, and relevant captured identity when available

#### Scenario: Diagnostic history remains bounded
- **GIVEN** more diagnostic events occur than the configured history limit
- **WHEN** diagnostics are queried
- **THEN** the newest events are retained
- **AND** old events are discarded without unbounded memory growth

### Requirement: Test window fixture
The system SHALL provide a visible Windows test fixture application with stable process and window identity for integration validation.

#### Scenario: Fixture starts with stable identity
- **WHEN** the fixture application starts
- **THEN** it opens a normal visible window
- **AND** the window title and process identity are stable enough for foreground capture and allowlist matching tests

#### Scenario: Fixture remains available for self-exit validation
- **GIVEN** the fixture application is running
- **WHEN** MouseShenanigans exits because the fixture matches a configured self-exit entry
- **THEN** the fixture process remains running unless the test harness explicitly stops it
