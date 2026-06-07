## ADDED Requirements

### Requirement: Runtime remapping lifecycle
The system SHALL provide a Windows-only proof-of-concept runtime that can be enabled, disabled, and disposed by the tray host without requiring a driver or Windows service.

#### Scenario: Runtime starts disabled
- **GIVEN** the tray host constructs the runtime
- **WHEN** the runtime has not been explicitly enabled
- **THEN** it does not install a low-level mouse hook
- **AND** it does not inject cursor movement

#### Scenario: Runtime is enabled
- **GIVEN** the runtime is configured for one target and one active remapping profile
- **WHEN** the tray host enables the runtime
- **THEN** the runtime installs the Windows mouse observation boundary needed for the proof of concept
- **AND** it reports an enabled status to the tray host

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is enabled
- **WHEN** the tray host disables the runtime
- **THEN** the runtime releases the mouse observation boundary
- **AND** later mouse movement is not remapped or injected by the app

#### Scenario: Runtime is disposed
- **GIVEN** the runtime has been constructed
- **WHEN** the tray host exits
- **THEN** the runtime releases any installed hook or native handle it owns

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

#### Scenario: Target does not match
- **GIVEN** the runtime is enabled
- **AND** neither the foreground window nor the window under the cursor matches the configured target
- **WHEN** ordinary mouse movement is observed
- **THEN** the original movement is passed through unchanged
- **AND** no remapped movement is injected

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is disabled
- **WHEN** ordinary mouse movement is observed for a matching target
- **THEN** the original movement is passed through unchanged
- **AND** no remapped movement is injected

### Requirement: Runtime delta remapping and injection
The system SHALL suppress targeted original mouse movement, apply the active core remapping profile to the observed delta, and inject the corrected relative movement through standard Win32 input APIs.

#### Scenario: Targeted movement is remapped
- **GIVEN** the runtime is enabled with the built-in horizontal inversion profile
- **AND** the configured target matches
- **WHEN** ordinary rightward mouse movement is observed
- **THEN** the original movement is suppressed
- **AND** equivalent leftward relative movement is injected

#### Scenario: Vertical movement is preserved by active profile
- **GIVEN** the runtime is enabled with the built-in horizontal inversion profile
- **AND** the configured target matches
- **WHEN** ordinary vertical mouse movement is observed
- **THEN** the original movement is suppressed
- **AND** equivalent vertical relative movement is injected

#### Scenario: Remapped output is zero
- **GIVEN** the runtime is enabled with an active profile that maps an observed movement to zero output
- **AND** the configured target matches
- **WHEN** that movement is observed
- **THEN** the original movement is suppressed
- **AND** no replacement movement is injected

#### Scenario: Fractional output reaches Windows boundary
- **GIVEN** the active core profile produces fractional remapped output
- **WHEN** the runtime prepares movement for Win32 injection
- **THEN** integer movement is produced at the Windows boundary using a deterministic conversion policy
- **AND** the core remapping profile remains unchanged

### Requirement: Injected movement feedback guard
The system SHALL avoid remapping movement that was injected by the app as part of runtime correction.

#### Scenario: Own injected movement is observed
- **GIVEN** the runtime injected replacement movement
- **WHEN** the low-level hook observes that injected movement
- **THEN** the movement is passed through without applying the active remapping profile again

#### Scenario: Injected movement flag is present
- **GIVEN** the runtime is enabled
- **WHEN** a low-level mouse event is marked as injected by Windows
- **THEN** the runtime does not treat that event as ordinary physical movement for remapping

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

#### Scenario: Unsupported platform
- **GIVEN** the tray app is run outside a supported Windows desktop session
- **WHEN** runtime controls are inspected
- **THEN** the runtime is not enabled
- **AND** the tray status indicates the runtime is unavailable or unsupported

### Requirement: Runtime proof-of-concept verification boundary
The system SHALL keep automated coverage focused on pure runtime decisions and SHALL require manual Windows desktop verification for real hook and input behavior.

#### Scenario: Pure runtime decisions are tested
- **GIVEN** automated tests run in CI
- **WHEN** the runtime proof-of-concept tests execute
- **THEN** they validate testable decisions such as target matching, enablement state, remapping decisions, injected-event pass-through, and integer boundary conversion without requiring a desktop mouse hook

#### Scenario: Desktop behavior is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** the verification covers hook installation, target-window gating, horizontal inversion, non-target pass-through, disable behavior, and feedback-loop avoidance

#### Scenario: Driver-level behavior is not introduced
- **GIVEN** the runtime proof of concept is implemented
- **WHEN** the repository is inspected
- **THEN** it uses standard user-session Win32 APIs
- **AND** it does not add a driver, Windows service, or elevated input layer
