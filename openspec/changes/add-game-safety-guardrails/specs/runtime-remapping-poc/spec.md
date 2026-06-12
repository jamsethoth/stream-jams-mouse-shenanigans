## MODIFIED Requirements

### Requirement: Runtime remapping lifecycle
The system SHALL provide a Windows-only proof-of-concept runtime that can be enabled, disabled, and disposed by the tray host without requiring a driver or Windows service, and SHALL only arm mouse observation when game safety permits the enable operation.

#### Scenario: Runtime starts disabled
- **GIVEN** the tray host constructs the runtime
- **WHEN** the runtime has not been explicitly enabled
- **THEN** it does not install a mouse observation boundary
- **AND** it does not write corrected cursor movement

#### Scenario: Runtime is enabled
- **GIVEN** the runtime is configured for one target and one active remapping profile
- **AND** game safety permits runtime remapping to be enabled
- **WHEN** the tray host enables the runtime
- **THEN** the runtime installs the Windows mouse observation boundary needed for the proof of concept
- **AND** it reports an enabled status to the tray host

#### Scenario: Runtime enable is safety-blocked
- **GIVEN** the runtime is configured for one target and one active remapping profile
- **AND** game safety denies runtime remapping
- **WHEN** the tray host attempts to enable the runtime
- **THEN** the runtime remains disabled
- **AND** it does not install a mouse observation boundary
- **AND** it reports the safety-blocked reason to the tray host

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is enabled
- **WHEN** the tray host disables the runtime
- **THEN** the runtime releases the mouse observation boundary
- **AND** later mouse movement is not remapped or corrected by the app

#### Scenario: Runtime is disposed
- **GIVEN** the runtime has been constructed
- **WHEN** the tray host exits
- **THEN** the runtime releases any installed mouse observation boundary or native handle it owns

## ADDED Requirements

### Requirement: Runtime command boundary consults game safety
The proof-of-concept runtime host SHALL route enable-capable commands through game safety before they can arm runtime remapping.

#### Scenario: Tray enable command is safety-gated
- **GIVEN** the tray app is running
- **AND** game safety denies runtime remapping
- **WHEN** the user selects the tray enable command
- **THEN** the shared command boundary does not call the runtime enable path
- **AND** tray-visible runtime status is refreshed with the safety-blocked reason

#### Scenario: Hotkey toggle command is safety-gated
- **GIVEN** the tray app is running
- **AND** the runtime is disabled
- **AND** game safety denies runtime remapping
- **WHEN** a registered toggle hotkey dispatches a command
- **THEN** the shared command boundary does not call the runtime enable path
- **AND** the runtime remains disabled

#### Scenario: Disable command remains available
- **GIVEN** the tray app is running
- **AND** game safety denies runtime remapping
- **WHEN** the user selects disable or emergency disable
- **THEN** the shared command boundary disables runtime remapping
- **AND** any active cursor lock is released
