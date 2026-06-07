## 1. Core Profile Model

- [x] 1.1 Add immutable core value objects for remapped deltas, directional output vectors, and directional remapping definitions.
- [x] 1.2 Add a `RemappingProfile` model with a stable non-empty name and left/right/up/down mappings.
- [x] 1.3 Add profile validation for empty names, missing directional mappings, and non-finite vector coordinates.
- [x] 1.4 Add a profile collection abstraction that rejects duplicate names using ordinal case-insensitive comparison and supports lookup by configured name without fallback selection.

## 2. Remapping Behavior

- [x] 2.1 Add a remapping engine that reuses `DirectionalMovement.FromDelta(dx, dy)` to decompose raw input.
- [x] 2.2 Combine directional magnitudes with profile output vectors using weighted vector summation.
- [x] 2.3 Add the built-in horizontal inversion profile that reverses horizontal movement and preserves vertical movement.
- [x] 2.4 Preserve zero input as zero output and keep output coordinates as `double` values for later Windows-boundary rounding decisions.

## 3. JSON Profile Documents

- [x] 3.1 Add JSON DTOs and parsing through `System.Text.Json` without adding external dependencies.
- [x] 3.2 Parse supplied JSON profile documents into validated profile collections.
- [x] 3.3 Reject malformed JSON, empty profile documents, duplicate names, missing mappings, empty names, and invalid vector values with diagnostic errors.
- [x] 3.4 Keep this slice free of runtime profile file persistence, reload behavior, and file-location decisions.

## 4. Tests

- [x] 4.1 Add core unit tests for valid and invalid profile definitions.
- [x] 4.2 Add core unit tests for duplicate-name detection and profile lookup behavior.
- [x] 4.3 Add core unit tests for horizontal inversion, directional scaling, axis swapping, diagonal mixed mappings, and zero movement.
- [x] 4.4 Add core unit tests for valid JSON parsing and invalid JSON/profile-document failures.
- [x] 4.5 Ensure all new tests run without Win32 hooks, a Windows desktop session, cursor injection, target-window state, tray UI, or Streamer.bot.

## 5. Documentation And Verification

- [x] 5.1 Update README notes only as needed to describe the implemented pure-core profile/remapping behavior and the still-deferred runtime features.
- [x] 5.2 Run `dotnet restore MouseShenanigans.slnx` where the .NET SDK is available.
- [x] 5.3 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore` or the agreed equivalent formatting validation.
- [x] 5.4 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 5.5 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 5.6 Record any local environment limitation if the current workspace cannot run the .NET SDK commands.
