## ADDED Requirements

### Requirement: Runtime remapping lifecycle
The system SHALL provide a Windows-only proof-of-concept runtime that can be enabled, disabled, and disposed by the tray host without requiring a driver or Windows service.

#### Scenario: Runtime starts disabled
- **GIVEN** the tray host constructs the runtime
- **WHEN** the runtime has not been explicitly enabled
- **THEN** it does not install a mouse observation boundary
- **AND** it does not write corrected cursor movement

#### Scenario: Runtime is enabled
- **GIVEN** the runtime is configured for one target and one active remapping profile
- **WHEN** the tray host enables the runtime
- **THEN** the runtime installs the Windows mouse observation boundary needed for the proof of concept
- **AND** it reports an enabled status to the tray host

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is enabled
- **WHEN** the tray host disables the runtime
- **THEN** the runtime releases the mouse observation boundary
- **AND** later mouse movement is not remapped or corrected by the app

#### Scenario: Runtime is disposed
- **GIVEN** the runtime has been constructed
- **WHEN** the tray host exits
- **THEN** the runtime releases any installed mouse observation boundary or native handle it owns

### Requirement: Target-window gating
The system SHALL apply runtime remapping only when one configured third-party target process name or window-title match is foreground or under the cursor.

#### Scenario: Foreground target matches
- **GIVEN** the runtime is enabled
- **AND** the configured target matches the foreground window
- **WHEN** ordinary mouse movement is observed
- **THEN** the movement is eligible for remapping

#### Scenario: Window under cursor matches
- **GIVEN** the runtime is enabled
- **AND** the configured target matches the window under the cursor
- **WHEN** ordinary mouse movement is observed
- **THEN** the movement is eligible for remapping

#### Scenario: Foreground target remains active outside target bounds
- **GIVEN** the runtime is enabled
- **AND** the configured target remains the foreground window
- **WHEN** excessive manual movement moves the cursor outside the target window bounds
- **THEN** remapping remains eligible in this proof of concept
- **AND** automatic pause-on-leave behavior is deferred to a later target-boundary control slice

#### Scenario: Target does not match
- **GIVEN** the runtime is enabled
- **AND** neither the foreground window nor the window under the cursor matches the configured target
- **WHEN** ordinary mouse movement is observed
- **THEN** the original movement is passed through unchanged
- **AND** no corrected cursor output is written

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is disabled
- **WHEN** ordinary mouse movement is observed for a matching target
- **THEN** the original movement is passed through unchanged
- **AND** no corrected cursor output is written

### Requirement: Runtime delta remapping and cursor output
The system SHALL apply the active core remapping profile to targeted observed mouse movement and write the corrected cursor output through standard user-session Win32 APIs.

#### Scenario: Targeted movement is remapped
- **GIVEN** the runtime is enabled with the built-in horizontal inversion profile
- **AND** the configured target matches
- **WHEN** ordinary rightward mouse movement is observed
- **THEN** the runtime applies equivalent leftward cursor output through the active Windows boundary

#### Scenario: Screen movement differs from raw movement
- **GIVEN** Windows pointer acceleration makes the screen cursor move farther than the raw input delta
- **AND** the configured target matches
- **WHEN** ordinary horizontal mouse movement is observed
- **THEN** the runtime keeps the absolute cursor correction bounded to the raw input magnitude
- **AND** it does not amplify a fast movement burst by remapping cursor-position baseline drift

#### Scenario: Mouse DPI requires correction calibration
- **GIVEN** the runtime is configured with a finite positive absolute correction scale
- **AND** the configured target matches
- **WHEN** ordinary mouse movement is observed
- **THEN** the runtime applies the scale to the absolute cursor correction
- **AND** the active remapping profile remains unchanged

#### Scenario: Vertical movement is preserved by active profile
- **GIVEN** the runtime is enabled with the built-in horizontal inversion profile
- **AND** the configured target matches
- **WHEN** ordinary vertical mouse movement is observed
- **THEN** the runtime preserves the final vertical cursor movement

#### Scenario: Remapped output is zero
- **GIVEN** the runtime is enabled with an active profile that maps an observed movement to zero output
- **AND** the configured target matches
- **WHEN** that movement is observed
- **THEN** the runtime applies cursor output that returns to the pre-movement position

#### Scenario: Fractional output reaches Windows boundary
- **GIVEN** the active core profile produces fractional remapped output
- **WHEN** the runtime prepares cursor output for a Win32 boundary
- **THEN** integer movement is produced at the Windows boundary using a deterministic conversion policy
- **AND** the core remapping profile remains unchanged

### Requirement: Cursor output feedback guard
The system SHALL avoid remapping movement that was written by the app as part of runtime correction.

#### Scenario: Own corrected movement is observed by a relative-input boundary
- **GIVEN** the runtime writes replacement movement through a relative-input boundary
- **WHEN** the low-level hook observes that injected movement
- **THEN** the movement is passed through without applying the active remapping profile again

#### Scenario: Injected movement flag is present
- **GIVEN** the runtime is enabled
- **WHEN** a low-level mouse event is marked as injected by Windows
- **THEN** the runtime does not treat that event as ordinary physical movement for remapping

#### Scenario: Absolute cursor write is observed by Raw Input
- **GIVEN** the runtime writes a corrected absolute cursor position
- **WHEN** Raw Input reports mouse data for that write
- **THEN** absolute mouse movement is not treated as ordinary relative physical movement for remapping

#### Scenario: Later physical movement is observed
- **GIVEN** the runtime has passed through an injected movement event
- **WHEN** later ordinary physical movement is observed for a matching target
- **THEN** the later physical movement remains eligible for remapping

### Requirement: Proof-of-concept tray control
The tray app SHALL expose only the minimal controls and status needed to run the runtime proof of concept manually.

#### Scenario: Tray enables runtime
- **GIVEN** the tray app is running on Windows
- **WHEN** the user selects the proof-of-concept enable command
- **THEN** the tray app enables the runtime
- **AND** the tray status indicates the runtime is enabled

#### Scenario: Tray disables runtime
- **GIVEN** the runtime is enabled
- **WHEN** the user selects the proof-of-concept disable command
- **THEN** the tray app disables the runtime
- **AND** the tray status indicates the runtime is disabled

#### Scenario: Tray exits process
- **GIVEN** the tray app is running from an `ApplicationContext` without a main form
- **WHEN** the user selects the Exit command
- **THEN** the tray app hides the tray icon
- **AND** it disposes the runtime
- **AND** it requests the application context thread to exit

#### Scenario: Unsupported platform
- **GIVEN** the tray app is run outside a supported Windows desktop session
- **WHEN** runtime controls are inspected
- **THEN** the runtime is not enabled
- **AND** the tray status indicates the runtime is unavailable or unsupported

### Requirement: Runtime proof-of-concept verification boundary
The system SHALL keep automated coverage focused on pure runtime decisions and SHALL require manual Windows desktop verification for real mouse observation and cursor-output behavior.

#### Scenario: Pure runtime decisions are tested
- **GIVEN** automated tests run in CI
- **WHEN** the runtime proof-of-concept tests execute
- **THEN** they validate testable decisions such as target matching, enablement state, remapping decisions, cursor-position decisions, injected-event pass-through, and integer boundary conversion without requiring a desktop mouse observation boundary

#### Scenario: Desktop behavior is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** the verification covers mouse observation registration, target-window gating, horizontal inversion, non-target pass-through, disable behavior, feedback-loop avoidance, process exit, and representative mouse DPI settings

#### Scenario: Driver-level behavior is not introduced
- **GIVEN** the runtime proof of concept is implemented
- **WHEN** the repository is inspected
- **THEN** it uses standard user-session Win32 APIs
- **AND** it does not add a driver, Windows service, or elevated input layer
