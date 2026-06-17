# Remapping Profiles

## Purpose

Define pure core remapping profile behavior: profile value objects, profile collections, delta remapping, and the boundary that lets runtime and tray features consume profiles without putting Windows desktop APIs into core profile logic.

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

### Requirement: Runtime integration boundary
The core remapping profile model SHALL remain independent of Windows desktop APIs while runtime, tray, persistence, and local automation layers consume validated profiles.

#### Scenario: Core tests exercise remapping without desktop APIs
- **WHEN** automated tests validate remapping profiles
- **THEN** they execute without requiring Win32 hooks, a Windows desktop session, cursor injection, target-window state, tray UI, or Streamer.bot

#### Scenario: Runtime layers consume profiles
- **WHEN** runtime configuration selects a validated profile
- **THEN** Windows runtime, tray profile switching, profile persistence, hotkeys, and local control endpoints use that profile through the runtime configuration boundary
