## REMOVED Requirements

### Requirement: Built-in horizontal inversion preset
The system SHALL provide a built-in horizontal inversion profile that reverses horizontal movement while preserving vertical movement.

#### Scenario: Right movement is inverted
- **WHEN** the horizontal inversion preset remaps a positive horizontal delta
- **THEN** the output horizontal delta is negative with the same magnitude
- **AND** the output vertical delta is zero

#### Scenario: Left movement is inverted
- **WHEN** the horizontal inversion preset remaps a negative horizontal delta
- **THEN** the output horizontal delta is positive with the same magnitude
- **AND** the output vertical delta is zero

#### Scenario: Vertical movement is preserved
- **WHEN** the horizontal inversion preset remaps vertical movement
- **THEN** the output vertical delta has the same sign and magnitude as the input vertical delta
- **AND** the output horizontal delta is zero

## ADDED Requirements

### Requirement: No built-in selectable remapping preset
The system SHALL NOT provide built-in selectable remapping profiles; selectable remapping profiles SHALL come from runtime configuration.

#### Scenario: Built-in catalog is empty
- **WHEN** the built-in profile catalog is inspected
- **THEN** it contains no profiles

#### Scenario: Disabled runtime represents no remapping
- **GIVEN** the user does not want remapping behavior applied
- **WHEN** the runtime is disabled
- **THEN** observed movement is not remapped by a pass-through profile
