## ADDED Requirements

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
The tray app project SHALL compile as a minimal Windows tray-oriented shell without implementing mouse interception, input injection, profile persistence, or external automation endpoints in this change.

#### Scenario: Tray shell compiles without runtime mouse behavior
- **WHEN** the solution is built
- **THEN** the tray shell project compiles
- **AND** it does not install mouse hooks, inject input, persist profiles, or expose local control endpoints

### Requirement: Baseline tests
The repository SHALL include automated tests for the pure core behavior introduced by the app foundation.

#### Scenario: Core tests execute in CI
- **WHEN** CI runs the test suite
- **THEN** the core test project executes and validates the initial pure core behavior without requiring Win32 hooks or a desktop session

#### Scenario: Core test coverage is meaningful
- **WHEN** the app foundation introduces initial directional mouse logic
- **THEN** tests cover representative directional delta decomposition behavior for horizontal, vertical, and zero movement

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
