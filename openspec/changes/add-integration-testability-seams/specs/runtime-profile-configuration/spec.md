## ADDED Requirements

### Requirement: Runtime configuration path override
The system SHALL support an explicit startup override for the runtime configuration file path so validation runs can isolate configuration from the user's production app data.

#### Scenario: Configuration path override is provided
- **GIVEN** the tray app is launched with a supported configuration path override
- **WHEN** runtime configuration is loaded or saved
- **THEN** the overridden file path is used
- **AND** the per-user production configuration path is not read or written by that tray process

#### Scenario: Configuration path override is absent
- **GIVEN** the tray app is launched without a configuration path override
- **WHEN** runtime configuration is loaded or saved
- **THEN** the deterministic per-user app data path remains the source of truth

#### Scenario: Configuration path override preserves encoding
- **GIVEN** the tray app is using an overridden configuration path
- **WHEN** runtime configuration is saved
- **THEN** the configuration file is written with the same explicit UTF-8 and line-ending behavior as the production configuration store
