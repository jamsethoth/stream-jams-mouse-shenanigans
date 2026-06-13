## MODIFIED Requirements

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

## ADDED Requirements

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
