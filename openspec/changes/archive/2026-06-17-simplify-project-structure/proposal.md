## Why

The current implementation still carries several planning-era seams and duplicate validation paths that no longer buy enough flexibility. Removing them now keeps the Windows tray utility easier to maintain before more runtime behavior is added on top.

## What Changes

- Remove the unused built-in remapping profile catalog and its test-only empty-catalog assertion.
- Remove the standalone core JSON profile parser when runtime configuration remains the supported profile-loading path.
- Collapse one-implementation seams where a delegate, direct value, or .NET platform type is enough:
  - hotkey binding provider
  - runtime configuration path provider
  - configuration folder launcher
  - custom runtime clock in favor of `TimeProvider`
- Remove unused `AbsoluteCursorRemappingCoordinator` constructor overloads and keep the one constructor used by production composition and tests.
- Replace hand-rolled wildcard matching for game process patterns with a .NET platform matcher only if existing safety tests prove the same behavior.
- Consolidate duplicated process/path/title normalization between runtime target selection and application safety identity without changing target matching semantics.
- Reduce duplicate GitHub Actions restore/build work while preserving formatting, build, test, OpenSpec, dependency review, and CodeQL validation.

## Capabilities

### New Capabilities

- None.

### Modified Capabilities

- `remapping-profiles`: remove requirements for a built-in profile catalog and standalone JSON profile document parser when profiles are supplied through runtime configuration.
- `app-foundation`: allow CI to provide the same validation coverage without requiring separate duplicate .NET restore/build jobs or fixed duplicate check names.

## Impact

- Affected code: `src/MouseShenanigans.Core`, `src/MouseShenanigans.Windows`, `src/MouseShenanigans.Tray`, and matching unit tests.
- Affected CI: `.github/workflows/ci.yml` may change job names or required-check alignment.
- Affected specs: `openspec/specs/remapping-profiles/spec.md` and `openspec/specs/app-foundation/spec.md`.
- Runtime behavior should stay unchanged: profile selection, local control, hotkeys, diagnostics, safety guardrails, and Windows integration seams should continue to work.
