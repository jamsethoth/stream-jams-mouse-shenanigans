## 1. Repository Build Foundation

- [x] 1.1 Add `global.json` selecting the intended .NET 10 SDK line with an appropriate roll-forward policy.
- [x] 1.2 Add `Directory.Build.props` with shared C# settings for nullable reference types, implicit usings, analyzers, warnings-as-errors, deterministic builds, and Windows targeting defaults where appropriate.
- [x] 1.3 Add `Directory.Packages.props` for central NuGet package version management.
- [x] 1.4 Extend `.editorconfig` with baseline C# formatting and analyzer preferences used by `dotnet format`.

## 2. Solution And Project Layout

- [x] 2.1 Create `MouseShenanigans.slnx` or `MouseShenanigans.sln` at the repository root.
- [x] 2.2 Add `src/MouseShenanigans.Core` as the pure core library project.
- [x] 2.3 Add `src/MouseShenanigans.Windows` as the Windows-specific adapter project targeting a Windows-specific .NET framework moniker.
- [x] 2.4 Add `src/MouseShenanigans.Tray` as a minimal WinForms tray executable targeting a Windows-specific .NET framework moniker.
- [x] 2.5 Add project references so the tray shell composes Core and Windows while Core remains independent of WinForms and Win32 interop.

## 3. Baseline App Shell And Core Logic

- [x] 3.1 Add minimal core directional delta decomposition logic based on the README model.
- [x] 3.2 Add minimal Windows adapter scaffolding without installing hooks, registering hotkeys, detecting target windows, or injecting input.
- [x] 3.3 Add a minimal tray app entry point/application context that compiles and exits cleanly without implementing profile persistence, tray menu behavior, or local automation endpoints.

## 4. Tests

- [x] 4.1 Add `tests/MouseShenanigans.Core.Tests` with the selected .NET test framework and central package versions.
- [x] 4.2 Add tests for representative directional delta decomposition: horizontal movement, vertical movement, diagonal movement, and zero movement.
- [x] 4.3 Ensure tests run without requiring a Windows desktop session, mouse hooks, input injection, or target-window state.
- [x] 4.4 Do not add automated desktop UI/input tests, WinAppDriver/Appium/FlaUI tests, or a self-hosted interactive Windows runner in this foundation change.

## 5. CI And Security Validation

- [x] 5.1 Add or update GitHub Actions CI so the `validate` job restores dependencies and verifies formatting/style/analyzers.
- [x] 5.2 Add or update the `build` job so it builds the solution and runs tests on `windows-latest`.
- [x] 5.3 Add or update the `dependency-review` job for pull requests.
- [x] 5.4 Add CodeQL C# analysis without introducing duplicate required CodeQL checks.
- [x] 5.5 Verify workflow job/check names are compatible with existing branch protection expectations.

## 6. Documentation And Verification

- [x] 6.1 Update README local tooling instructions with the required .NET SDK line and restore/format/build/test commands.
- [x] 6.2 Document that actual tray behavior, global hotkeys, low-level mouse hooks, input injection, target-window gating, and Streamer.bot interaction remain manual verification items for now.
- [x] 6.3 Run `dotnet restore` for the solution where the .NET SDK is available.
- [x] 6.4 Run `dotnet format --verify-no-changes` or the agreed equivalent formatting validation.
- [x] 6.5 Run `dotnet build --no-restore` for the solution.
- [x] 6.6 Run `dotnet test --no-build` for the solution.
- [x] 6.7 On Windows, manually launch the tray shell and verify it starts and exits cleanly without installing mouse hooks, injecting input, exposing control endpoints, or requiring a target application.
- [x] 6.8 Record any environment limitation if local verification cannot be completed in the current WSL workspace.
