## Why

Manual Windows validation for the published tray app is repetitive and easy to miss, especially for foreground focus, global hotkeys, local-control timing, and process lifecycle behavior. A Windows integration suite can turn that checklist into repeatable proof while keeping true desktop delivery constraints explicit.

## What Changes

- Add a Windows-only integration test suite that launches the published tray app in an isolated temp configuration environment.
- Automate validation of harness isolation, local-control readiness, diagnostics, foreground target capture, and desktop prerequisite detection.
- Drive the seam-provided desktop test fixture for foreground-window and global-hotkey scenarios when the test runner has a real Windows user session.
- Add automated static/release-artifact checks for non-evasive constraints: no drivers, services, injection, overlays, memory reads, stealth behavior, or unexpected elevated helpers.
- Gate implementation on `add-integration-testability-seams` being completed and present on remote `main`.

## Capabilities

### New Capabilities
- `windows-integration-validation-suite`: Covers Windows-only integration validation of tray runtime, foreground target capture, local-control behavior, desktop prerequisite reporting, process lifecycle behavior, and non-evasive implementation checks.

### Modified Capabilities
None. The suite validates existing behavior through a new validation capability rather than changing runtime, hotkey, or local-control contracts.

## Impact

- Adds a Windows integration test project or script harness, fixture launch/control helpers, and CI/self-hosted runner documentation.
- Requires the `add-integration-testability-seams` change as a prerequisite so tests can isolate config, bind to a test port, observe diagnostics, shorten sentinel timing, and launch a stable test window.
- Feature-specific changes can add their own Windows integration cases on top of this reusable suite.
- Does not replace a final human smoke test for confidence in a normal streaming setup, but it should reduce the manual checklist to exceptional verification.
