## 1. Profile Surface Cleanup

- [x] 1.1 Delete `BuiltInRemappingProfiles` and remove the empty-catalog assertion.
- [x] 1.2 Delete `RemappingProfileJsonParser` and its parser-only tests.
- [x] 1.3 Confirm remaining profile tests still cover profile validation, duplicate-name rejection, lookup, and remapping math.

## 2. One-Implementation Seam Cleanup

- [x] 2.1 Replace `IHotkeyBindingProvider`/`DefaultRuntimeHotkeyBindingProvider` with a direct default binding collection or static helper.
- [x] 2.2 Replace `IRuntimeConfigurationPathProvider`/`RuntimeConfigurationPathProvider` with direct resolved configuration-path input on `RuntimeConfigurationFileStore`.
- [x] 2.3 Replace `IConfigurationFolderLauncher`/`ExplorerConfigurationFolderLauncher` with a delegate seam and platform folder launch using `UseShellExecute`.
- [x] 2.4 Replace `IRuntimeClock`/`SystemRuntimeClock` with `TimeProvider` and update tests with a tiny controllable test provider.
- [x] 2.5 Remove unused `AbsoluteCursorRemappingCoordinator` constructor overloads.
- [x] 2.6 Remove diagnostic extension methods that are only exercised by tests, or replace their tests with direct `IDiagnosticRecorder.Record` coverage.

## 3. Matching And CI Simplification

- [x] 3.1 Pin current game process wildcard behavior and runtime target selector semantics with focused tests before simplifying matching code.
- [x] 3.2 Replace the custom game process wildcard matcher with the .NET platform matcher if the pinned behavior is preserved.
- [x] 3.3 Consolidate process/path/title normalization by reusing `ApplicationIdentity` inside `RuntimeTargetSelector` without changing target eligibility outcomes.
- [x] 3.4 Collapse duplicate Windows restore/build work in `.github/workflows/ci.yml` while preserving format, build, test, OpenSpec, dependency review, and CodeQL coverage.
- [x] 3.5 Update documentation or branch-protection notes for any CI check-name changes.

## 4. Validation

- [x] 4.1 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 4.2 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 4.3 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 4.4 Run `openspec.cmd validate simplify-project-structure --strict`.
- [x] 4.5 Run `openspec.cmd validate --specs --strict`.
- [x] 4.6 If target selector, cursor movement, or safety matching changed, run the desktop-gated Windows validation path or manually verify mouse interception, target-window detection, injected-movement feedback-loop avoidance, cursor lock, and emergency disable in a real Windows desktop session.
  - Passed with TRX logger: `dotnet test tests\MouseShenanigans.WindowsIntegration.Tests\MouseShenanigans.WindowsIntegration.Tests.csproj --configuration Release --no-restore --no-build --filter "Category=Desktop" --logger "trx;LogFileName=desktop-validation.trx" --results-directory "TestResults\desktop-validation-rerun"` produced `TestResults\desktop-validation-rerun\desktop-validation.trx`.
