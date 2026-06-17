## Context

The repo now has the full Windows tray utility foundation: core profile math, runtime configuration, Win32 runtime adapters, tray controls, loopback local control, diagnostics, safety guardrails, and Windows integration tests. A repo-wide ponytail audit found several leftover planning-era pieces that are either unused, test-only, or replaceable by simpler .NET/platform constructs.

Current evidence:
- `BuiltInRemappingProfiles.All` is empty and only asserted empty by one test.
- `RemappingProfileJsonParser` has no production callers; runtime profile persistence uses `RuntimeConfigurationJsonSerializer`.
- `IHotkeyBindingProvider` has one implementation and two call sites.
- `IRuntimeConfigurationPathProvider` only supplies a string path to `RuntimeConfigurationFileStore`.
- `IConfigurationFolderLauncher` wraps one Explorer launcher and test fakes.
- `IRuntimeClock` and `SystemRuntimeClock` duplicate `TimeProvider.System`.
- Three `AbsoluteCursorRemappingCoordinator` constructor overloads are unused; production and tests call the full constructor.
- CI runs two Windows jobs that both restore and build the same solution.

## Goals / Non-Goals

**Goals:**
- Delete unused profile parser/catalog code and update specs so the deletion is intentional.
- Replace single-implementation seams with direct values, delegates, or .NET platform APIs.
- Keep runtime behavior and local-control API responses stable.
- Keep safety guardrail behavior covered by tests before simplifying matching code.
- Reduce duplicated CI work without removing validation coverage.

**Non-Goals:**
- No driver-level input work.
- No new profile format or public remote API.
- No behavior changes to hotkeys, tray commands, local control, diagnostics, safety guardrails, or desktop validation opt-in.
- No new dependencies.

## Decisions

1. Treat runtime configuration as the only profile JSON loading path.
   - Remove `RemappingProfileJsonParser` and its tests instead of wiring it into current runtime code.
   - Alternative: keep parser as a future import API. Rejected because there is no caller and runtime configuration already validates persisted profiles.

2. Delete the empty built-in profile catalog.
   - Keep configured profiles as the source of selectable profiles.
   - Alternative: move horizontal inversion into `MouseShenanigans.Core`. Rejected because current defaults live in runtime configuration and the catalog is explicitly empty.

3. Collapse simple seams only where tests stay cheap.
   - Replace the hotkey binding provider with a static/default binding collection.
   - Replace the configuration path provider with a resolved path string or static path helper.
   - Replace the configuration folder launcher interface with an `Action<string>`/delegate seam and `UseShellExecute`.
   - Replace the custom runtime clock with `TimeProvider`.
   - Keep Win32/Kestrel/process seams that protect tests from desktop or network side effects.

4. Simplify safety matching only behind existing tests.
   - Replace the custom wildcard matcher with `System.IO.Enumeration.FileSystemName.MatchesSimpleExpression` only if the existing game-process-pattern tests still pass or are expanded to pin current semantics first.
   - Consolidate target identity normalization by reusing `ApplicationIdentity` inside `RuntimeTargetSelector`, preserving current process/path/title matching outcomes.

5. Collapse duplicate CI restore/build work.
   - Prefer one Windows .NET validation job that restores, checks formatting, builds, and tests.
   - Keep OpenSpec, dependency review, and CodeQL jobs.
   - Update branch protection/required-check documentation if check names change.

## Risks / Trade-offs

- Removing public-ish core parser/catalog types could break external consumers if the library is being used outside this repo. Mitigation: this app is repo-owned, no package publication is documented, and specs will record the removal.
- Changing wildcard matching could alter safety behavior. Mitigation: pin current wildcard cases before replacement and keep deny-rule precedence untouched.
- CI check-name changes can break branch protection. Mitigation: either keep a compatible `validate` job name or update required checks with the CI change.
- Replacing interfaces with delegates can make call sites less nominal. Mitigation: apply only to tiny one-method seams where the delegate signature is clearer than the interface.
