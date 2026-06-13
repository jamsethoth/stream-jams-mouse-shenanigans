## ADDED Requirements

### Requirement: Runtime configuration file loading
The system SHALL load runtime target and profile configuration from one UTF-8 JSON file in a deterministic per-user app data location, with a built-in fallback when the file is absent.

#### Scenario: Config file exists and is valid
- **GIVEN** a runtime configuration file exists at the per-user app data path
- **AND** the file contains a valid target, active profile, cursor-lock default, and optional custom profile collection
- **WHEN** the tray app starts
- **THEN** the runtime is created from that configuration
- **AND** tray-visible status identifies the active target and profile

#### Scenario: Config file is absent
- **GIVEN** no runtime configuration file exists at the per-user app data path
- **WHEN** the tray app starts
- **THEN** the runtime uses the built-in fallback target and horizontal inversion profile
- **AND** when the per-user app data location is writable, a default runtime configuration file is created for editing
- **AND** tray startup succeeds without requiring file creation

#### Scenario: Config file is invalid at startup
- **GIVEN** a runtime configuration file exists but fails validation
- **WHEN** the tray app starts
- **THEN** the runtime falls back to the built-in default configuration
- **AND** tray-visible status reports the configuration error

### Requirement: Runtime configuration validation
The system SHALL validate the runtime configuration before applying target, profile, active-profile, or cursor-lock settings.

#### Scenario: Target process is configured
- **GIVEN** the configuration contains a target process name
- **WHEN** the configuration is loaded
- **THEN** runtime target selection uses that process name

#### Scenario: Target title is configured
- **GIVEN** the configuration contains a target window-title substring
- **WHEN** the configuration is loaded
- **THEN** runtime target selection can use that window-title match

#### Scenario: Target selector is missing
- **GIVEN** the configuration omits both target process name and target window-title substring
- **WHEN** the configuration is loaded
- **THEN** validation fails without applying the configuration

#### Scenario: Active profile exists
- **GIVEN** the configuration names an active profile that exists in the built-in profile catalog or configured custom profile collection
- **WHEN** the configuration is loaded
- **THEN** that profile is selected for runtime remapping

#### Scenario: Built-in profile is always available
- **GIVEN** the configuration omits custom profiles or contains no custom profiles
- **WHEN** the configuration is loaded
- **THEN** the built-in horizontal inversion profile remains available for runtime remapping and tray profile selection

#### Scenario: Custom profiles are additive
- **GIVEN** the configuration contains one or more valid custom profiles
- **WHEN** the configuration is loaded
- **THEN** the loaded profile collection contains the built-in horizontal inversion profile and the configured custom profiles

#### Scenario: Active profile is missing
- **GIVEN** the configuration names an active profile that is absent from both the built-in profile catalog and configured custom profile collection
- **WHEN** the configuration is loaded
- **THEN** validation fails without selecting an arbitrary fallback profile from that file

#### Scenario: Cursor lock default is configured
- **GIVEN** the configuration sets a cursor-lock default
- **WHEN** the runtime is created from the configuration
- **THEN** the runtime starts with that cursor-lock setting before the user changes it from tray controls

### Requirement: Tray profile selection
The tray app SHALL expose a profile submenu that lists loaded profiles and allows the active profile to be changed without restarting the app.

#### Scenario: Tray lists loaded profiles
- **GIVEN** the tray app loaded a valid runtime configuration with multiple profiles
- **WHEN** the user opens the tray menu
- **THEN** the profile submenu lists each available profile name
- **AND** the active profile is indicated

#### Scenario: User selects another profile
- **GIVEN** the tray app is running with multiple loaded profiles
- **WHEN** the user selects a different profile from the tray menu
- **THEN** the runtime switches to that profile for later remapping decisions
- **AND** tray-visible status is refreshed
- **AND** the selected active profile is persisted to the configuration file

#### Scenario: Profile selection happens while enabled
- **GIVEN** the runtime is enabled
- **WHEN** the user selects a different active profile
- **THEN** later eligible mouse movement uses the newly selected profile
- **AND** movement accumulators or re-entry state that depend on the prior profile are reset as needed

### Requirement: Configuration reload
The tray app SHALL provide a reload command that re-reads the runtime configuration file without restarting the tray process.

#### Scenario: Reload succeeds
- **GIVEN** the tray app is running
- **AND** the configuration file has been edited to a valid configuration
- **WHEN** the user selects reload configuration
- **THEN** the loaded target, profile collection, active profile, and cursor-lock default are updated from the file
- **AND** tray-visible status is refreshed

#### Scenario: Reload fails validation
- **GIVEN** the tray app is running with a last known good configuration
- **AND** the configuration file has been edited to invalid JSON or invalid profile data
- **WHEN** the user selects reload configuration
- **THEN** the last known good configuration remains active
- **AND** tray-visible status reports the reload error

### Requirement: Profile command integration
The system SHALL expose profile selection and configuration reload through the same in-process runtime command boundary used by tray and hotkey runtime controls.

#### Scenario: Profile selection command is available in process
- **GIVEN** the shared runtime command boundary exists
- **WHEN** the tray profile submenu selects a profile
- **THEN** the command boundary applies the profile selection

#### Scenario: Reload command is available in process
- **GIVEN** the shared runtime command boundary exists
- **WHEN** the tray reload command is selected
- **THEN** the command boundary reloads configuration or reports the reload failure

### Requirement: Focused target capture
The system SHALL expose a hotkey command that changes the runtime target to the current foreground window without requiring the user to edit the configuration file manually.

#### Scenario: Foreground process is captured
- **GIVEN** the tray app is running with runtime configuration loaded
- **AND** the current foreground window has a readable process name
- **WHEN** the target-capture hotkey is pressed
- **THEN** the runtime target selector is updated to that process name
- **AND** the updated target selector is persisted to the runtime configuration file
- **AND** later eligible movement uses the captured target

#### Scenario: Foreground title is captured when process is unavailable
- **GIVEN** the tray app is running with runtime configuration loaded
- **AND** the current foreground window has no readable process name
- **AND** the current foreground window has a readable title
- **WHEN** the target-capture hotkey is pressed
- **THEN** the runtime target selector is updated to that title match
- **AND** the updated target selector is persisted to the runtime configuration file

#### Scenario: Foreground target cannot be captured
- **GIVEN** the tray app is running with runtime configuration loaded
- **AND** the current foreground window identity is unavailable
- **WHEN** the target-capture hotkey is pressed
- **THEN** the last known good runtime target remains active
- **AND** tray-visible status reports the capture failure
