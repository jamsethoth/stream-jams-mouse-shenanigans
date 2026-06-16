## 1. Dependency Gates

- [ ] 1.1 Confirm `add-integration-testability-seams` is complete and present in remote `main` before implementation starts.
- [ ] 1.2 Confirm `add-game-safety-guardrails` is complete and present in remote `main` before implementing guardrails validation cases.
- [ ] 1.3 Confirm the integration suite can use isolated config path, loopback URL override, diagnostics, fast self-exit interval, and the test-window fixture from the seams change.
- [ ] 1.4 Decide whether the interactive desktop tests will be opt-in, category-filtered, or skipped automatically when desktop prerequisites are missing.

## 2. Harness Foundation

- [ ] 2.1 Add a Windows-only integration test project or scripted harness to the solution.
- [ ] 2.2 Add helpers to publish or locate the tray app artifact under test.
- [ ] 2.3 Add process launch helpers that set temp config path, test local-control URL, diagnostics settings, and sentinel interval overrides.
- [ ] 2.4 Add readiness polling for local-control status and diagnostics.
- [ ] 2.5 Add cleanup that stops only tray and seam-provided fixture processes launched by the harness.
- [ ] 2.6 Add tests proving harness isolation does not read or write the user's production configuration path.

## 3. Noninteractive Local-Control Validation

- [ ] 3.1 Automate empty allowlist enable denial through `POST /api/v1/runtime/enable`.
- [ ] 3.2 Automate allowlisted fixture target enable success through local control.
- [ ] 3.3 Automate local-control foreground allowed-application capture response timing and `confirmationPending` response shape.
- [ ] 3.4 Automate diagnostics/status assertions for blocked enable and allowed enable.
- [ ] 3.5 Add failure diagnostics that include tray stdout/stderr, diagnostics endpoint output, and config file path on test failure.

## 4. Interactive Desktop Validation

- [ ] 4.1 Add prerequisite detection for real Windows desktop, foreground-window control, global hotkey delivery, and UI Automation support.
- [ ] 4.2 Add helpers to start the seam-provided fixture window and make it the foreground window.
- [ ] 4.3 Add helpers to send `Ctrl+Alt+Shift+F9` using a real keyboard input path.
- [ ] 4.4 Add UI Automation helpers to locate the confirmation prompt, assert captured identity text, and accept or cancel it.
- [ ] 4.5 Automate hotkey capture confirmation accept and verify `allowedApplications` persistence without enabling remapping.
- [ ] 4.6 Automate hotkey capture confirmation cancel and verify `allowedApplications` remains unchanged.
- [ ] 4.7 Ensure unsupported desktop prerequisites report skipped or inconclusive results distinctly from passing tests.

## 5. Self-Exit Validation

- [ ] 5.1 Automate configured self-exit while runtime is enabled and assert the tray process exits.
- [ ] 5.2 Automate configured self-exit while runtime is disabled and assert the tray process exits without enabling runtime.
- [ ] 5.3 Assert the matched fixture process remains running after MouseShenanigans exits.
- [ ] 5.4 Assert diagnostics identify the matched self-exit entry and shutdown reason.

## 6. Non-Evasive Scan

- [ ] 6.1 Add source scan checks for forbidden invasive APIs, game-candidate classifiers, protected-game denylists, and built-in self-exit entries.
- [ ] 6.2 Add project and publish-output inventory checks for drivers, services, elevated helper executables, overlays, injection helpers, and unexpected artifacts.
- [ ] 6.3 Ensure scan failures produce actionable file/path evidence.

## 7. Runner Documentation

- [ ] 7.1 Document how to run noninteractive integration tests locally.
- [ ] 7.2 Document how to run interactive desktop tests in a real Windows user session.
- [ ] 7.3 Document why GitHub-hosted Windows runners may skip desktop-dependent tests.
- [ ] 7.4 Document cleanup expectations for tray and fixture processes.

## 8. Automated Validation

- [ ] 8.1 Run `dotnet restore MouseShenanigans.slnx`.
- [ ] 8.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [ ] 8.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [ ] 8.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [ ] 8.5 Run the noninteractive Windows integration suite.
- [ ] 8.6 Run the interactive desktop integration suite when a real Windows desktop session is available, or record the explicit skip/inconclusive reason.
- [ ] 8.7 Run the non-evasive source and publish-output scan.
- [ ] 8.8 Run `openspec.cmd validate add-windows-integration-validation-suite --strict`.
- [ ] 8.9 Run `openspec.cmd validate --specs --strict`.
