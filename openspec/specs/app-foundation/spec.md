## Purpose

Define the repository, build, test, documentation, CI, and tray-app foundation for Stream Jams Mouse Shenanigans.

## Requirements

### Requirement: .NET solution structure
The repository SHALL contain a .NET solution for the app foundation with separate projects for pure core logic, Windows-specific adapters, the tray app shell, and core tests.

#### Scenario: Solution projects are separated by responsibility
- **WHEN** a developer inspects the solution
- **THEN** it contains distinct projects for `MouseShenanigans.Core`, `MouseShenanigans.Windows`, `MouseShenanigans.Tray`, and `MouseShenanigans.Core.Tests`

#### Scenario: Windows-only projects target Windows explicitly
- **WHEN** a developer inspects Windows-specific project files
- **THEN** the Windows adapter and tray shell projects target a Windows-specific .NET framework moniker

### Requirement: Build conventions
The repository SHALL define shared C# build conventions so project settings are consistent across the solution.

#### Scenario: Shared build settings exist
- **WHEN** a developer inspects repository-level build configuration
- **THEN** nullable reference types, implicit usings, analyzer settings, warnings-as-errors, deterministic builds, and central package management are configured explicitly

#### Scenario: SDK selection is explicit
- **WHEN** a developer or CI runner invokes .NET commands
- **THEN** the repository provides an explicit SDK selection policy for the intended .NET major version

### Requirement: App shell boundary
The tray app project SHALL compile as a Windows tray-oriented composition root for runtime controls, configuration, diagnostics, and local automation.

#### Scenario: Tray shell compiles
- **WHEN** the solution is built
- **THEN** the tray shell project compiles
- **AND** it remains separate from pure core remapping logic

### Requirement: Baseline tests
The repository SHALL include automated tests for core behavior, Windows adapter behavior, tray behavior, and integration seams.

#### Scenario: Tests execute in CI
- **WHEN** CI runs the test suite
- **THEN** test projects execute without requiring an interactive desktop session by default

#### Scenario: Core test coverage is meaningful
- **WHEN** the app foundation introduces initial directional mouse logic
- **THEN** tests cover representative directional delta decomposition behavior for horizontal, vertical, and zero movement

### Requirement: Manual desktop behavior boundary
The repository SHALL keep desktop-sensitive validation explicit so normal automated checks do not silently depend on foreground-window control or keyboard input.

#### Scenario: Desktop validation is opt-in
- **WHEN** desktop-sensitive integration tests are run
- **THEN** they require explicit opt-in from a real Windows desktop session

#### Scenario: Manual checks are documented
- **WHEN** a developer reads the repository documentation or implementation checklist
- **THEN** it identifies desktop-session behavior that must be verified manually or through desktop-gated tests

### Requirement: CI validation
The repository SHALL validate the .NET app foundation in GitHub Actions.

#### Scenario: Pull request validation runs .NET checks
- **WHEN** a pull request targets `main`
- **THEN** CI restores dependencies, verifies formatting or style, builds the solution, and runs tests

#### Scenario: Required check names stay aligned
- **WHEN** CI workflows are updated
- **THEN** repository validation provides checks corresponding to `validate`, `build`, `dependency-review`, and one CodeQL security analysis path
- **AND** it avoids adding duplicate required CodeQL checks

### Requirement: Developer documentation
The repository SHALL document the local commands needed to restore, format, build, and test the app foundation.

#### Scenario: Developer can find local validation commands
- **WHEN** a developer reads the repository documentation
- **THEN** it identifies the required .NET SDK line and the commands for restore, format validation, build, and test
