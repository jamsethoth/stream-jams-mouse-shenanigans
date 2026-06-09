## ADDED Requirements

### Requirement: Optional target-window cursor lock
The system SHALL provide an opt-in cursor lock mode that constrains the cursor to the active target window bounds while the runtime is enabled, the configured target matches, and target bounds are available.

#### Scenario: Cursor lock starts disabled
- **GIVEN** the tray host constructs the runtime with default proof-of-concept options
- **WHEN** the runtime status is inspected
- **THEN** cursor lock is disabled
- **AND** the cursor is not constrained to a target window by default

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

## MODIFIED Requirements

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

### Requirement: Proof-of-concept tray control
The tray app SHALL expose only the minimal controls and status needed to run the runtime proof of concept manually, including an opt-in cursor-lock toggle for target-boundary validation.

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

### Requirement: Runtime proof-of-concept verification boundary
The system SHALL keep automated coverage focused on pure runtime decisions and SHALL require manual Windows desktop verification for real mouse observation, cursor-output behavior, target-boundary behavior, and cursor-lock behavior.

#### Scenario: Pure runtime decisions are tested
- **GIVEN** automated tests run in CI
- **WHEN** the runtime proof-of-concept tests execute
- **THEN** they validate testable decisions such as target matching, target-boundary eligibility, enablement state, remapping decisions, cursor-position decisions, cursor-lock apply and release decisions, injected-event pass-through, and integer boundary conversion without requiring a desktop mouse observation boundary

#### Scenario: Desktop behavior is manually verified
- **GIVEN** the change is implemented
- **WHEN** manual Windows verification is performed
- **THEN** the verification covers mouse observation registration, target-window gating, outside-bounds pause behavior, automatic re-entry, optional cursor locking, cursor-lock release, horizontal inversion, non-target pass-through, disable behavior, feedback-loop avoidance, process exit, and representative mouse DPI settings

#### Scenario: Driver-level behavior is not introduced
- **GIVEN** the runtime proof of concept is implemented
- **WHEN** the repository is inspected
- **THEN** it uses standard user-session Win32 APIs
- **AND** it does not add a driver, Windows service, or elevated input layer
