## ADDED Requirements

### Requirement: Shared runtime command boundary
The proof-of-concept runtime host SHALL route tray controls and hotkey controls through one in-process command boundary for enable, disable, toggle, and emergency-disable behavior.

#### Scenario: Tray enable uses shared command boundary
- **GIVEN** the tray app is running
- **WHEN** the user selects the tray enable command
- **THEN** the shared command boundary enables the runtime
- **AND** tray-visible runtime status is refreshed

#### Scenario: Tray disable uses shared command boundary
- **GIVEN** the tray app is running
- **WHEN** the user selects the tray disable command
- **THEN** the shared command boundary disables the runtime
- **AND** any cursor lock held by the runtime is released
- **AND** tray-visible runtime status is refreshed

#### Scenario: Hotkey command uses shared command boundary
- **GIVEN** the tray app is running
- **WHEN** a registered runtime hotkey dispatches a command
- **THEN** the same command boundary used by tray controls applies the runtime state transition

#### Scenario: Emergency disable releases safety state
- **GIVEN** the runtime is enabled
- **AND** cursor lock may be enabled or active
- **WHEN** the shared command boundary receives emergency disable
- **THEN** the runtime is disabled
- **AND** any active cursor lock is released
- **AND** later mouse movement is not remapped by the app
