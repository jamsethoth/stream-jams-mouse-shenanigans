## Why

Manual Windows validation for application safety guardrails is repetitive and easy to miss, especially for foreground focus, global hotkeys, confirmation prompts, local-control timing, and self-exit behavior. A Windows integration suite can turn that checklist into repeatable proof while keeping true desktop delivery constraints explicit.

## What Changes

- Add a Windows-only integration test suite that launches the published tray app in an isolated temp configuration environment.
- Automate validation of empty allowlist blocking, allowed target enablement, local-control command behavior, foreground allowlist capture, confirmation accept/cancel, and configured self-exit behavior.
- Drive an interactive desktop test fixture for foreground-window and global-hotkey scenarios when the test runner has a real Windows user session.
- Add automated static/release-artifact checks for non-evasive constraints: no drivers, services, injection, overlays, memory reads, stealth behavior, game classifier, protected-game denylist, or built-in self-exit entries.
- Gate implementation on `add-integration-testability-seams` being completed and present on remote `main`.

## Capabilities

### New Capabilities
- `windows-integration-validation-suite`: Covers Windows-only integration validation of tray runtime, safety guardrails, foreground capture confirmation, local-control behavior, self-exit behavior, and non-evasive implementation checks.

### Modified Capabilities
None. The suite validates existing behavior through a new validation capability rather than changing runtime, hotkey, or local-control contracts.

## Impact

- Adds a Windows integration test project or script harness, test fixtures, and CI/self-hosted runner documentation.
- Requires the `add-integration-testability-seams` change as a prerequisite so tests can isolate config, bind to a test port, observe diagnostics, shorten sentinel timing, and launch a stable test window.
- Requires `add-game-safety-guardrails` behavior to be implemented before the suite can validate those guardrails end to end.
- Does not replace a final human smoke test for confidence in a normal streaming setup, but it should reduce the manual checklist to exceptional verification.
