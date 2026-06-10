## ADDED Requirements

### Requirement: Global runtime hotkeys
The system SHALL register Windows-only global hotkeys for toggling runtime remapping and emergency-disabling runtime remapping while the tray app is running in a supported desktop session.

#### Scenario: Toggle hotkey enables disabled runtime
- **GIVEN** the tray app is running in a supported Windows desktop session
- **AND** the runtime is disabled
- **WHEN** the user presses the default toggle hotkey
- **THEN** the runtime is enabled
- **AND** tray-visible runtime status is refreshed

#### Scenario: Toggle hotkey disables enabled runtime
- **GIVEN** the tray app is running in a supported Windows desktop session
- **AND** the runtime is enabled
- **WHEN** the user presses the default toggle hotkey
- **THEN** the runtime is disabled
- **AND** any cursor lock held by the runtime is released
- **AND** tray-visible runtime status is refreshed

#### Scenario: Emergency disable hotkey disables runtime
- **GIVEN** the tray app is running in a supported Windows desktop session
- **AND** the runtime is enabled
- **WHEN** the user presses the default emergency-disable hotkey
- **THEN** the runtime is disabled
- **AND** any cursor lock held by the runtime is released
- **AND** tray-visible runtime status is refreshed

#### Scenario: Emergency disable hotkey is idempotent
- **GIVEN** the tray app is running in a supported Windows desktop session
- **AND** the runtime is already disabled
- **WHEN** the user presses the default emergency-disable hotkey
- **THEN** the runtime remains disabled
- **AND** tray-visible runtime status remains coherent

### Requirement: Hotkey registration lifecycle
The system SHALL register default runtime hotkeys at tray startup and SHALL unregister any registered hotkeys when the tray app exits.

#### Scenario: Hotkeys register at startup
- **GIVEN** the tray app starts in a supported Windows desktop session
- **WHEN** default hotkey registration succeeds
- **THEN** the toggle and emergency-disable hotkeys are available until tray exit

#### Scenario: Hotkey registration partially fails
- **GIVEN** the tray app starts in a supported Windows desktop session
- **WHEN** one or more default hotkeys cannot be registered
- **THEN** the tray app keeps running
- **AND** the failed hotkey is reported through tray-visible status
- **AND** tray menu controls remain usable

#### Scenario: Hotkeys unregister on exit
- **GIVEN** one or more hotkeys were registered by the tray app
- **WHEN** the tray app exits or disposes its hotkey boundary
- **THEN** all successfully registered hotkeys are unregistered

### Requirement: Hotkey manual verification boundary
The system SHALL keep real global hotkey delivery verification manual while covering command dispatch and registration lifecycle through non-desktop seams.

#### Scenario: Automated hotkey tests use seams
- **GIVEN** automated tests run without a Windows desktop session
- **WHEN** hotkey tests execute
- **THEN** they validate default hotkey definitions, command dispatch, registration failure handling, and unregister lifecycle without requiring real global keyboard input

#### Scenario: Desktop hotkey behavior is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** verification covers pressing the toggle hotkey while the target app has focus
- **AND** pressing the emergency-disable hotkey while remapping and cursor lock are active
- **AND** exiting the tray after hotkeys have been registered
