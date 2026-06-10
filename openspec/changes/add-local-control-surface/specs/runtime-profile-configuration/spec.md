## ADDED Requirements

### Requirement: Profile commands are externally invokable
Runtime profile configuration commands SHALL be invokable through the local control surface after the local control listener is available.

#### Scenario: External profile selection uses loaded profile configuration
- **GIVEN** runtime profile configuration is loaded
- **AND** the local control surface is running
- **WHEN** a local control client selects an existing profile
- **THEN** the same profile selection behavior used by the tray profile menu is applied

#### Scenario: External configuration reload preserves last known good behavior
- **GIVEN** runtime profile configuration has a last known good configuration
- **AND** the local control surface is running
- **WHEN** a local control client requests configuration reload and the file is invalid
- **THEN** the last known good configuration remains active
- **AND** the local control response reports the reload failure
