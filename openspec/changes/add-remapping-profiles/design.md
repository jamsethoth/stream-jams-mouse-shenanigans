## Context

The repository now has a .NET app foundation with `MouseShenanigans.Core` for pure logic, `MouseShenanigans.Windows` for future Win32 integration, `MouseShenanigans.Tray` for the WinForms notification-area shell, and core xUnit tests. The README defines the central remapping model: decompose raw mouse deltas into left, right, up, and down magnitudes, then map each direction to a configured output vector.

This change should make that model executable in the core library before any desktop-session behavior is added. Later changes can feed real mouse deltas from Windows hooks into the same core remapping API.

## Goals / Non-Goals

**Goals:**

- Represent named directional remapping profiles in `MouseShenanigans.Core`.
- Transform raw `(dx, dy)` input into remapped `(dx, dy)` output using an active profile.
- Provide the horizontal inversion preset as the first concrete behavior from the README.
- Parse and validate JSON profile documents supplied by callers.
- Cover behavior with fast unit tests that do not require Windows desktop APIs.

**Non-Goals:**

- No low-level mouse hooks, cursor/input injection, target-window gating, global hotkeys, tray profile controls, profile file persistence, or Streamer.bot/local automation endpoints.
- No commitment to the final persisted file location or runtime reload strategy.
- No Windows-specific project changes beyond compile/test compatibility if the core API shape requires it.

## Decisions

### Keep remapping in the core library

The remapping model belongs in `MouseShenanigans.Core` because it is deterministic domain behavior. `MouseShenanigans.Windows` should later adapt platform mouse events into core inputs and apply core outputs, but it should not own the profile math.

Alternative considered: implement the remapping inside the Windows adapter first. That would make the first proof of concept faster to wire to hooks, but it would blur the boundary between math, validation, and Win32 concerns.

### Use small immutable value objects

Add core value objects for remapped deltas, output vectors, directional mappings, profiles, and profile collections. These should be immutable records or readonly structs where practical, matching the existing `DirectionalMovement` style and keeping test expectations straightforward.

Profile names should be validated as non-empty text. Output vector coordinates should be finite `double` values. Profile collections should use ordinal case-insensitive duplicate-name validation so streaming automation commands can avoid ambiguous profile selection.

### Define remapping as weighted vector summation

The engine should reuse `DirectionalMovement.FromDelta(dx, dy)` and compute:

```text
output = leftMagnitude  * profile.Left
       + rightMagnitude * profile.Right
       + upMagnitude    * profile.Up
       + downMagnitude  * profile.Down
```

This directly matches the README and supports horizontal inversion, scaling, axis swapping, and mixed mappings without special cases.

### Provide presets in core

The horizontal inversion preset should be a named built-in profile in core. Its mappings should reverse left/right movement and preserve up/down movement. If an identity preset is useful during implementation or tests, it can be added as a helper, but the required built-in behavior for this change is horizontal inversion.

### Parse JSON without runtime persistence

Use `System.Text.Json` from the base platform to parse supplied JSON profile documents into DTOs, then convert DTOs into validated domain objects. This keeps the slice dependency-free and avoids deciding where profiles live on disk.

The initial JSON shape should stay close to the README:

```json
{
  "profiles": [
    {
      "name": "horizontal-inversion",
      "left": { "x": 1, "y": 0 },
      "right": { "x": -1, "y": 0 },
      "up": { "x": 0, "y": -1 },
      "down": { "x": 0, "y": 1 }
    }
  ]
}
```

The parser should reject malformed JSON, empty profile sets, missing directional mappings, duplicate names, empty names, and non-finite vector values. Returning partially valid collections would make runtime profile switching harder to reason about, so parsing should fail the whole document when any profile is invalid.

## Risks / Trade-offs

- JSON shape may evolve once runtime persistence and tray editing exist -> Keep this parser focused and document the shape through tests rather than treating it as a final public API.
- Exception-based validation can become awkward for UI diagnostics -> Keep validation errors specific enough that later UI or automation layers can translate them into useful messages.
- Floating-point output can produce fractional deltas that later Windows injection must handle -> Preserve `double` in core for fidelity; defer rounding/clamping policy to the future input-injection slice.
- Built-in preset names could become user-visible contracts -> Use stable kebab-case names and avoid renaming once tests depend on them.

## Migration Plan

This is additive. Existing `DirectionalMovement` behavior and tests should remain valid. New core APIs and tests can be introduced without changing the tray shell or Windows adapter behavior.

## Open Questions

- Should future profile persistence store only user-defined profiles, or include built-in presets in the same document?
- Should runtime profile selection use exact names only, or allow aliases for streaming commands?
- Should the future injection layer round fractional output per event, accumulate fractional remainders, or let Windows APIs handle integer conversion at the boundary?
