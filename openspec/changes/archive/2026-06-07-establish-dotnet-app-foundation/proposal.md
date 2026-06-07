## Why

The repository needs a concrete .NET/C# foundation before implementation can proceed safely. Establishing the project layout, build validation, and test baseline now reduces ambiguity around the Windows-only tray app architecture without prematurely implementing mouse interception behavior.

## What Changes

- Introduce a .NET 10 Windows-oriented solution structure for the app foundation.
- Add a C# project layout that separates pure remapping/profile logic, Windows-specific adapters, the tray app shell, and tests.
- Add repository-level .NET build, formatting, analyzer, and test validation.
- Update CI so pull requests validate the .NET solution instead of the earlier placeholder Node-oriented checks.
- Add only shell-level app projects and relevant baseline tests; defer Win32 mouse hook, input injection, profile persistence, tray UI behavior, and Streamer.bot control endpoints to later changes.

## Capabilities

### New Capabilities

- `app-foundation`: Defines the .NET/C# project layout, repo validation, CI checks, and baseline tests for the Windows-only tray utility foundation.

### Modified Capabilities

None.

## Impact

- Adds .NET solution and project files for the initial app shell.
- Adds baseline test project and validation commands.
- Updates GitHub Actions CI to run .NET restore, format/analyzer checks, build, and tests.
- Keeps runtime mouse remapping and local automation protocol behavior out of scope for this change.
