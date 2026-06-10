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
