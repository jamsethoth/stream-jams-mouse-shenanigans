## ADDED Requirements

### Requirement: Runtime commands are externally invokable
The proof-of-concept runtime SHALL allow enable, disable, toggle, emergency-disable, capture-foreground-target, and status commands to be invoked through the local control surface after the local listener is available.

#### Scenario: External enable command uses runtime command boundary
- **GIVEN** the local control surface is running
- **WHEN** a local control client requests runtime enable
- **THEN** the shared runtime command boundary enables the runtime

#### Scenario: External disable command releases cursor lock
- **GIVEN** the local control surface is running
- **AND** the runtime may have cursor lock active
- **WHEN** a local control client requests runtime disable or emergency disable
- **THEN** the shared runtime command boundary disables the runtime
- **AND** any active cursor lock is released

#### Scenario: External status command reports runtime state
- **GIVEN** the local control surface is running
- **WHEN** a local control client requests runtime status
- **THEN** the response reflects the same runtime state and degraded status visible through the tray app

#### Scenario: External target capture uses runtime command boundary
- **GIVEN** the local control surface is running
- **AND** a foreground window identity is available
- **WHEN** a local control client requests foreground target capture
- **THEN** the shared runtime command boundary persists the foreground window as the runtime target
- **AND** the runtime options are updated without restarting the tray app
