## REMOVED Requirements

### Requirement: No built-in selectable remapping preset
**Reason**: The built-in catalog is intentionally empty, has no production caller, and only exists to prove emptiness. Selectable profiles already come from runtime configuration.

**Migration**: Delete the catalog and keep runtime configuration as the source of selectable profiles. The disabled runtime continues to represent no remapping.

### Requirement: JSON profile document parsing
**Reason**: The standalone core profile parser has no production caller. Runtime configuration already parses, validates, persists, and loads configured profiles through the supported configuration document.

**Migration**: Use runtime configuration JSON for persisted profiles. Keep profile value-object and collection validation so invalid configured profiles are still rejected.
