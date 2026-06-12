## ADDED Requirements

### Requirement: Empty user-managed game allowlist
The system SHALL ship with an empty local game allowlist and SHALL require explicit user configuration before a game process can be enabled for runtime remapping.

#### Scenario: No game allowlist entries exist
- **GIVEN** the tray app is running with default safety configuration
- **AND** no user game allowlist entries have been configured
- **WHEN** the user attempts to enable remapping for a game target
- **THEN** the enable attempt is denied
- **AND** no mouse observation boundary is started
- **AND** tray-visible status identifies that the game is not allowlisted

#### Scenario: User allowlists a game process
- **GIVEN** the user has added a game allowlist entry for a process identity
- **AND** no protected-game deny rule matches that process
- **WHEN** the user attempts to enable remapping for that matching game target
- **THEN** game safety permits the enable attempt

#### Scenario: Non-game utility target
- **GIVEN** the configured runtime target is a non-game utility target
- **AND** no disallowed or protected game process is detected
- **WHEN** the user enables remapping
- **THEN** game safety does not require a game allowlist entry for that non-game utility target

### Requirement: Protected and disallowed game policy
The system SHALL maintain a protected-game deny policy for known high-risk games and SHALL treat matching processes as denied by default.

#### Scenario: Protected game is running
- **GIVEN** a running process matches a protected-game deny rule
- **WHEN** game safety evaluates whether runtime remapping may be enabled
- **THEN** the decision is denied
- **AND** the denial reason identifies the protected-game rule

#### Scenario: Allowlisted game also matches protected deny rule
- **GIVEN** a game process matches both a user allowlist entry and a protected-game deny rule
- **WHEN** game safety evaluates the process
- **THEN** the protected-game deny rule takes precedence
- **AND** the decision is denied

#### Scenario: Game identity is unavailable
- **GIVEN** game safety must identify a process to decide whether remapping may be enabled
- **WHEN** the process identity cannot be read
- **THEN** the decision is denied
- **AND** the denial reason identifies the unreadable process identity

### Requirement: Safety-gated runtime enabling
The system SHALL evaluate game safety before any command arms runtime mouse observation or cursor output.

#### Scenario: Tray enable is denied
- **GIVEN** the tray app is running
- **AND** game safety denies enabling remapping
- **WHEN** the user selects the tray enable command
- **THEN** the runtime remains disabled
- **AND** no mouse observation boundary is started
- **AND** tray-visible status shows the safety denial

#### Scenario: Hotkey toggle-to-enable is denied
- **GIVEN** the tray app is running
- **AND** the runtime is disabled
- **AND** game safety denies enabling remapping
- **WHEN** the toggle hotkey dispatches an enable transition
- **THEN** the runtime remains disabled
- **AND** no mouse observation boundary is started
- **AND** hotkey dispatch status remains visible

#### Scenario: Emergency disable bypasses safety gating
- **GIVEN** the runtime may be enabled or failed
- **WHEN** the emergency disable command is dispatched
- **THEN** the command disables runtime remapping without requiring a positive game-safety decision
- **AND** any active cursor lock is released

### Requirement: Runtime safety sentinel self-exit
The system SHALL monitor for disallowed game processes while the tray app is running and SHALL exit MouseShenanigans when a denied game is detected.

#### Scenario: Non-allowlisted game launches while runtime is enabled
- **GIVEN** the runtime is enabled
- **AND** a launched process is classified as a game
- **AND** the process does not match a user allowlist entry
- **WHEN** the safety sentinel observes the process
- **THEN** the app disables runtime remapping
- **AND** any active cursor lock is released
- **AND** the mouse observation boundary is unregistered
- **AND** MouseShenanigans exits its own process

#### Scenario: Protected game launches while runtime is disabled
- **GIVEN** the tray app is running
- **AND** the runtime is disabled
- **WHEN** the safety sentinel observes a protected-game process
- **THEN** MouseShenanigans exits its own process
- **AND** it does not attempt to terminate the game process

#### Scenario: Sentinel cannot classify an armed process
- **GIVEN** the runtime is enabled
- **WHEN** the safety sentinel cannot classify a process that must be evaluated
- **THEN** the app disables runtime remapping
- **AND** MouseShenanigans exits its own process

### Requirement: Safety status and diagnostics
The system SHALL expose game safety state and denial reasons through tray-visible status and local diagnostics.

#### Scenario: Enable attempt is blocked
- **GIVEN** game safety denies an enable attempt
- **WHEN** tray status is refreshed
- **THEN** the status identifies that remapping is safety-blocked
- **AND** the status includes the matched rule or fail-closed reason

#### Scenario: Self-exit is requested
- **GIVEN** the safety sentinel requests self-exit
- **WHEN** the app begins shutdown
- **THEN** a local diagnostic record identifies the process and rule that caused shutdown

### Requirement: Non-evasive safety constraints
The system SHALL implement game safety without adding anti-cheat evasion or invasive integration techniques.

#### Scenario: Safety implementation is inspected
- **WHEN** the repository is inspected after implementation
- **THEN** the app does not add drivers, Windows services, elevated input layers, game-process injection, overlays, game memory reads, anti-cheat tampering, obfuscation for concealment, or stealth behavior

#### Scenario: Disallowed game is detected
- **GIVEN** a disallowed or protected game process is detected
- **WHEN** game safety responds
- **THEN** MouseShenanigans only disables and exits its own process
- **AND** it does not terminate or manipulate the game or anti-cheat process
