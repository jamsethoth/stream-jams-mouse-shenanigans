# Runtime Remapping Proof Of Concept

## Purpose

Define the Windows-only proof-of-concept runtime that gates mouse remapping to a configured target, observes movement through standard user-session Win32 APIs, applies bounded absolute cursor correction, and exposes minimal tray controls for manual validation.

## Requirements

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

### Requirement: Configured runtime composition
The proof-of-concept runtime SHALL compose target selection, active remapping profile, and cursor-lock default from loaded runtime configuration instead of only using hard-coded proof-of-concept defaults.

#### Scenario: Runtime uses configured target
- **GIVEN** a valid runtime configuration specifies a target process name or title match
- **WHEN** the tray app creates the runtime
- **THEN** target-window gating uses the configured target selector

#### Scenario: Runtime uses configured active profile
- **GIVEN** a valid runtime configuration specifies an active profile
- **WHEN** eligible mouse movement is observed
- **THEN** runtime remapping uses the configured active profile

#### Scenario: Runtime retains fallback defaults
- **GIVEN** no valid runtime configuration is available
- **WHEN** the tray app creates the runtime
- **THEN** the existing Streamer.bot horizontal inversion fallback remains available for proof-of-concept validation

### Requirement: Target-window gating
The system SHALL apply runtime remapping only when one configured third-party target process name or window-title match is foreground or under the cursor and the cursor is inside readable target window bounds.

#### Scenario: Foreground target matches inside bounds
- **GIVEN** the runtime is enabled
- **AND** the configured target matches the foreground window
- **AND** the cursor is inside the foreground target window bounds
- **WHEN** ordinary mouse movement is observed
- **THEN** the movement is eligible for remapping

#### Scenario: Window under cursor matches inside bounds
- **GIVEN** the runtime is enabled
- **AND** the configured target matches the window under the cursor
- **AND** the cursor is inside that target window bounds
- **WHEN** ordinary mouse movement is observed
- **THEN** the movement is eligible for remapping

#### Scenario: Foreground target remains active outside target bounds
- **GIVEN** the runtime is enabled
- **AND** the configured target remains the foreground window
- **AND** cursor lock is disabled
- **WHEN** excessive manual movement moves the cursor outside the target window bounds
- **THEN** remapping is paused
- **AND** the original movement is passed through unchanged
- **AND** no corrected cursor output is written

#### Scenario: Cursor re-enters target bounds
- **GIVEN** the runtime is enabled
- **AND** remapping is paused because the cursor is outside a matching foreground target bounds
- **WHEN** ordinary movement returns the cursor inside the target window bounds
- **THEN** remapping remains paused for a short target re-entry grace period
- **AND** if the cursor remains inside the target window bounds until the grace period expires
- **THEN** later matching movement is eligible for remapping again

#### Scenario: Target bounds are unavailable
- **GIVEN** the runtime is enabled
- **AND** the configured target matches by process name or window title
- **WHEN** the runtime cannot read target window bounds
- **THEN** remapping is paused for that movement
- **AND** no corrected cursor output is written

#### Scenario: Target does not match
- **GIVEN** the runtime is enabled
- **AND** neither the foreground window nor the window under the cursor matches the configured target
- **WHEN** ordinary mouse movement is observed
- **THEN** the original movement is passed through unchanged
- **AND** no corrected cursor output is written

#### Scenario: Runtime is disabled
- **GIVEN** the runtime is disabled
- **WHEN** ordinary mouse movement is observed for a matching target inside bounds
- **THEN** the original movement is passed through unchanged
- **AND** no corrected cursor output is written

### Requirement: Runtime delta remapping and cursor output
The system SHALL apply the active core remapping profile to targeted observed mouse movement and write the corrected cursor output through standard user-session Win32 APIs.

#### Scenario: Targeted movement is remapped
- **GIVEN** the runtime is enabled with an active horizontal inversion profile from runtime configuration
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
- **GIVEN** the runtime is enabled with an active horizontal inversion profile from runtime configuration
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
The system SHALL avoid remapping absolute cursor writes produced by the app as ordinary physical movement.

#### Scenario: Absolute cursor write is observed by Raw Input
- **GIVEN** the runtime writes a corrected absolute cursor position
- **WHEN** Raw Input reports mouse data for that write
- **THEN** absolute mouse movement is not treated as ordinary relative physical movement for remapping

#### Scenario: Later physical movement is observed
- **GIVEN** the runtime has written a corrected absolute cursor position
- **WHEN** later ordinary relative physical movement is observed for a matching target
- **THEN** the later physical movement remains eligible for remapping

### Requirement: Optional target-window cursor lock
The system SHALL provide a cursor lock mode that constrains the cursor to the active target window bounds while the runtime is enabled, the configured target matches, and target bounds are available.

#### Scenario: Cursor lock starts enabled by default
- **GIVEN** the tray host constructs the runtime with default proof-of-concept options
- **WHEN** the runtime status is inspected
- **THEN** cursor lock is enabled
- **AND** the cursor will be constrained after a target match is acquired while remapping is enabled

#### Scenario: Cursor lock constrains active target
- **GIVEN** the runtime is enabled
- **AND** cursor lock is enabled
- **AND** the configured target matches with readable target bounds
- **WHEN** ordinary mouse movement is observed while the target is active
- **THEN** the cursor remains constrained to the target window bounds

#### Scenario: Cursor lock prevents target escape
- **GIVEN** the runtime is enabled
- **AND** cursor lock is enabled
- **AND** the configured target matches with readable target bounds
- **WHEN** remapped or physical movement would carry the cursor outside the target window bounds
- **THEN** the cursor remains inside the target window bounds

#### Scenario: Cursor lock releases when disabled
- **GIVEN** cursor lock is enabled and constraining the cursor to the target window bounds
- **WHEN** the user disables cursor lock
- **THEN** the runtime releases the cursor constraint
- **AND** later cursor movement is no longer constrained by the app

#### Scenario: Cursor lock releases when target is lost
- **GIVEN** cursor lock is enabled and constraining the cursor to the target window bounds
- **WHEN** the configured target no longer matches as foreground or under the cursor
- **THEN** the runtime releases the cursor constraint

#### Scenario: Cursor lock releases when runtime stops
- **GIVEN** cursor lock is enabled and constraining the cursor to the target window bounds
- **WHEN** the runtime is disabled, fails, or is disposed
- **THEN** the runtime releases the cursor constraint

### Requirement: Proof-of-concept tray control
The tray app SHALL expose only the minimal controls and status needed to run the runtime proof of concept manually, including a cursor-lock toggle for target-boundary validation.

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
- **AND** any cursor lock held by the runtime is released

#### Scenario: Tray toggles cursor lock
- **GIVEN** the tray app is running on Windows
- **WHEN** the user toggles the proof-of-concept cursor-lock command
- **THEN** the runtime cursor-lock setting is updated
- **AND** the tray menu indicates whether cursor lock is enabled

#### Scenario: Tray exits process
- **GIVEN** the tray app is running from an `ApplicationContext` without a main form
- **WHEN** the user selects the Exit command
- **THEN** the tray app hides the tray icon
- **AND** it disposes the runtime
- **AND** any cursor lock held by the runtime is released
- **AND** it requests the application context thread to exit

#### Scenario: Unsupported platform
- **GIVEN** the tray app is run outside a supported Windows desktop session
- **WHEN** runtime controls are inspected
- **THEN** the runtime is not enabled
- **AND** cursor lock is not enabled
- **AND** the tray status indicates the runtime is unavailable or unsupported

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

### Requirement: Runtime proof-of-concept verification boundary
The system SHALL keep automated coverage focused on pure runtime decisions and SHALL require manual Windows desktop verification for real mouse observation, cursor-output behavior, target-boundary behavior, and cursor-lock behavior.

#### Scenario: Pure runtime decisions are tested
- **GIVEN** automated tests run in CI
- **WHEN** the runtime proof-of-concept tests execute
- **THEN** they validate testable decisions such as target matching, target-boundary eligibility, enablement state, remapping decisions, cursor-position decisions, cursor-lock apply and release decisions, and integer boundary conversion without requiring a desktop mouse observation boundary

#### Scenario: Desktop behavior is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** the verification covers mouse observation registration, target-window gating, outside-bounds pause behavior, automatic re-entry, optional cursor locking, cursor-lock release, horizontal inversion, non-target pass-through, disable behavior, feedback-loop avoidance, process exit, and representative mouse DPI settings

#### Scenario: Driver-level behavior is not introduced
- **GIVEN** the runtime proof of concept is implemented
- **WHEN** the repository is inspected
- **THEN** it uses standard user-session Win32 APIs
- **AND** it does not add a driver, Windows service, or elevated input layer
