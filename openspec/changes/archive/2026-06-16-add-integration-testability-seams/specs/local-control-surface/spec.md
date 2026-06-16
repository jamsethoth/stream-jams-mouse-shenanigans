## ADDED Requirements

### Requirement: Local diagnostics endpoint
The system SHALL expose recent diagnostic events through the localhost-only local control surface for validation and troubleshooting.

#### Scenario: Diagnostics endpoint returns event history
- **GIVEN** the local control listener is running
- **WHEN** a client requests the diagnostics endpoint
- **THEN** the response is JSON
- **AND** it includes recent diagnostic events in chronological or reverse-chronological order
- **AND** it remains restricted to loopback local control

#### Scenario: Diagnostics endpoint is stable for automation
- **GIVEN** a validation-relevant event occurred
- **WHEN** an integration test queries diagnostics
- **THEN** the response contains stable fields for event type, timestamp, message, and captured identity when available

### Requirement: Local control URL override remains loopback-only
The system SHALL allow a startup override for the local-control bind URL while preserving the loopback-only security boundary.

#### Scenario: Test run overrides local-control URL
- **GIVEN** the tray app is launched with a local-control URL override using an HTTP loopback address
- **WHEN** the local control listener starts
- **THEN** it binds to the overridden URL
- **AND** tray-visible status and diagnostics identify the active URL

#### Scenario: Non-loopback override is rejected
- **GIVEN** the tray app is launched with a non-loopback local-control URL override
- **WHEN** local control options are validated
- **THEN** validation fails
- **AND** the listener does not bind to the non-loopback address
