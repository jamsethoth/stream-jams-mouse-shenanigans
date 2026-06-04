## 1. Repository Build Foundation

- [ ] 1.1 Add `global.json` selecting the intended .NET 10 SDK line with an appropriate roll-forward policy.
- [ ] 1.2 Add `Directory.Build.props` with shared C# settings for nullable reference types, implicit usings, analyzers, warnings-as-errors, deterministic builds, and Windows targeting defaults where appropriate.
- [ ] 1.3 Add `Directory.Packages.props` for central NuGet package version management.
- [ ] 1.4 Extend `.editorconfig` with baseline C# formatting and analyzer preferences used by `dotnet format`.

## 2. Solution And Project Layout

- [ ] 2.1 Create `MouseShenanigans.slnx` or `MouseShenanigans.sln` at the repository root.
- [ ] 2.2 Add `src/MouseShenanigans.Core` as the pure core library project.
- [ ] 2.3 Add `src/MouseShenanigans.Windows` as the Windows-specific adapter project targeting a Windows-specific .NET framework moniker.
- [ ] 2.4 Add `src/MouseShenanigans.Tray` as a minimal WinForms tray executable targeting a Windows-specific .NET framework moniker.
- [ ] 2.5 Add project references so the tray shell composes Core and Windows while Core remains independent of WinForms and Win32 interop.

## 3. Baseline App Shell And Core Logic

- [ ] 3.1 Add minimal core directional delta decomposition logic based on the README model.
- [ ] 3.2 Add minimal Windows adapter scaffolding without installing hooks, registering hotkeys, detecting target windows, or injecting input.
- [ ] 3.3 Add a minimal tray app entry point/application context that compiles and exits cleanly without implementing profile persistence, tray menu behavior, or local automation endpoints.

## 4. Tests

- [ ] 4.1 Add `tests/MouseShenanigans.Core.Tests` with the selected .NET test framework and central package versions.
- [ ] 4.2 Add tests for representative directional delta decomposition: horizontal movement, vertical movement, diagonal movement, and zero movement.
- [ ] 4.3 Ensure tests run without requiring a Windows desktop session, mouse hooks, input injection, or target-window state.

## 5. CI And Security Validation

- [ ] 5.1 Add or update GitHub Actions CI so the `validate` job restores dependencies and verifies formatting/style/analyzers.
- [ ] 5.2 Add or update the `build` job so it builds the solution and runs tests on `windows-latest`.
- [ ] 5.3 Add or update the `dependency-review` job for pull requests.
- [ ] 5.4 Add CodeQL C# analysis without introducing duplicate required CodeQL checks.
- [ ] 5.5 Verify workflow job/check names are compatible with existing branch protection expectations.

## 6. Documentation And Verification

- [ ] 6.1 Update README local tooling instructions with the required .NET SDK line and restore/format/build/test commands.
- [ ] 6.2 Run `dotnet restore` for the solution where the .NET SDK is available.
- [ ] 6.3 Run `dotnet format --verify-no-changes` or the agreed equivalent formatting validation.
- [ ] 6.4 Run `dotnet build --no-restore` for the solution.
- [ ] 6.5 Run `dotnet test --no-build` for the solution.
- [ ] 6.6 On Windows, manually launch the tray shell and verify it starts and exits cleanly without installing mouse hooks, injecting input, exposing control endpoints, or requiring a target application.
- [ ] 6.7 Record any environment limitation if local verification cannot be completed in the current WSL workspace.
