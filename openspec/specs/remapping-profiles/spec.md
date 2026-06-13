# Remapping Profiles

## Purpose

Define pure core remapping profile behavior: profile value objects, profile collections, delta remapping, JSON profile parsing, and the boundary that keeps runtime input hooks and tray controls out of the core profile slice.

## Requirements

### Requirement: Directional remapping profile definition
The system SHALL represent each remapping profile as a stable name plus output vectors for all four directional movement components: left, right, up, and down.

#### Scenario: Valid profile includes all directional mappings
- **WHEN** a profile has a non-empty name and finite output vectors for left, right, up, and down
- **THEN** the profile is accepted as valid

#### Scenario: Profile name is empty
- **WHEN** a profile name is empty or whitespace
- **THEN** the profile is rejected with a validation error

#### Scenario: Directional mapping is missing
- **WHEN** a profile omits left, right, up, or down mapping
- **THEN** the profile is rejected with a validation error identifying the missing direction

#### Scenario: Directional vector is not finite
- **WHEN** any directional vector contains a non-finite x or y value
- **THEN** the profile is rejected with a validation error identifying the invalid direction

### Requirement: Profile collection validation
The system SHALL validate a profile collection before use and SHALL reject duplicate profile names using ordinal case-insensitive comparison.

#### Scenario: Profile names are unique
- **WHEN** a profile collection contains profiles with unique names
- **THEN** the collection is accepted as valid

#### Scenario: Profile names differ only by case
- **WHEN** a profile collection contains `Invert` and `invert`
- **THEN** the collection is rejected with a duplicate-name validation error

#### Scenario: Profile lookup uses configured name
- **WHEN** a caller requests a profile by a name present in the collection
- **THEN** the matching profile is returned for remapping

#### Scenario: Requested profile is absent
- **WHEN** a caller requests a profile name that is not present in the collection
- **THEN** the request fails without selecting an arbitrary fallback profile

### Requirement: No built-in selectable remapping preset
The system SHALL NOT provide built-in selectable remapping profiles; selectable remapping profiles SHALL come from runtime configuration.

#### Scenario: Built-in catalog is empty
- **WHEN** the built-in profile catalog is inspected
- **THEN** it contains no profiles

#### Scenario: Disabled runtime represents no remapping
- **GIVEN** the user does not want remapping behavior applied
- **WHEN** the runtime is disabled
- **THEN** observed movement is not remapped by a pass-through profile

### Requirement: Delta remapping
The system SHALL transform raw mouse deltas by decomposing input into directional components and summing each component multiplied by the active profile's configured output vector.

#### Scenario: Zero input stays zero
- **WHEN** the remapping engine receives `dx = 0` and `dy = 0`
- **THEN** it returns `dx = 0` and `dy = 0`

#### Scenario: Directional scaling is applied
- **WHEN** a profile maps right movement to output vector `{ "x": 2, "y": 0 }`
- **AND** the remapping engine receives `dx = 3` and `dy = 0`
- **THEN** it returns `dx = 6` and `dy = 0`

#### Scenario: Axis swap is applied
- **WHEN** a profile maps right movement to output vector `{ "x": 0, "y": 1 }`
- **AND** the remapping engine receives `dx = 4` and `dy = 0`
- **THEN** it returns `dx = 0` and `dy = 4`

#### Scenario: Diagonal movement combines directions
- **WHEN** a profile maps left movement to `{ "x": -1, "y": 0 }` and down movement to `{ "x": 0, "y": 0.5 }`
- **AND** the remapping engine receives `dx = -2` and `dy = 6`
- **THEN** it returns `dx = -2` and `dy = 3`

### Requirement: JSON profile document parsing
The system SHALL parse UTF-8 JSON profile documents containing named profiles and SHALL validate the parsed profiles before returning them for use.

#### Scenario: Valid JSON profile document is parsed
- **WHEN** a JSON document contains a `profiles` array with valid profile objects
- **THEN** the parser returns a validated profile collection

#### Scenario: JSON document is malformed
- **WHEN** a JSON profile document is not valid JSON
- **THEN** parsing fails with a diagnostic error

#### Scenario: JSON document has no profiles
- **WHEN** a JSON profile document does not contain any profiles
- **THEN** parsing fails with a validation error

#### Scenario: JSON profile has invalid mapping
- **WHEN** a JSON profile contains an invalid directional mapping
- **THEN** parsing fails with a validation error instead of returning a partially valid collection

### Requirement: Runtime integration boundary
The remapping profile slice SHALL remain pure core behavior and SHALL NOT add Windows mouse hooks, input injection, target-window gating, tray profile controls, hotkeys, profile file persistence, or local automation endpoints.

#### Scenario: Core tests exercise remapping without desktop APIs
- **WHEN** automated tests validate remapping profiles
- **THEN** they execute without requiring Win32 hooks, a Windows desktop session, cursor injection, target-window state, tray UI, or Streamer.bot

#### Scenario: Runtime features remain deferred
- **WHEN** this change is implemented
- **THEN** Windows hook installation, input injection, target-window selection, tray profile switching, global hotkeys, profile file persistence, and local control endpoints remain unimplemented
