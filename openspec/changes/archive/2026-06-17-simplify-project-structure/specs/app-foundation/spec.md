## MODIFIED Requirements

### Requirement: CI validation
The repository SHALL validate the .NET app foundation in GitHub Actions without duplicate restore/build work.

#### Scenario: Pull request validation runs .NET checks
- **WHEN** a pull request targets `main`
- **THEN** CI restores dependencies, verifies formatting or style, builds the solution, and runs tests

#### Scenario: Required checks stay aligned with validation coverage
- **WHEN** CI workflows are updated
- **THEN** repository validation provides checks for .NET validation, dependency review, and one CodeQL security analysis path
- **AND** it avoids duplicate restore/build jobs for the same .NET solution
- **AND** it avoids adding duplicate required CodeQL checks
