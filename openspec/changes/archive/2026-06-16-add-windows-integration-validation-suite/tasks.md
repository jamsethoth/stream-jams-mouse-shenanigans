## 1. Dependency Gates

- [x] 1.1 Confirm `add-integration-testability-seams` is complete and present in remote `main` before implementation starts.
- [x] 1.2 Confirm the integration suite can use isolated config path, loopback URL override, diagnostics, and the test-window fixture from the seams change.
- [x] 1.3 Decide that desktop tests will be category-filtered and will skip or report inconclusive when desktop prerequisites are missing.

## 2. Harness Foundation

- [x] 2.1 Add a Windows-only integration test project or scripted harness to the solution.
- [x] 2.2 Add helpers to publish or locate the tray app artifact under test.
- [x] 2.3 Add process launch helpers that set temp config path, test local-control URL, and diagnostics settings.
- [x] 2.4 Add readiness polling for local-control status and diagnostics.
- [x] 2.5 Add cleanup that stops only tray and seam-provided fixture processes launched by the harness.
- [x] 2.6 Add tests proving harness isolation does not read or write the user's production configuration path.

## 3. Non-Desktop Published Tray Validation

- [x] 3.1 Automate published tray startup and local-control status readiness.
- [x] 3.2 Automate diagnostics endpoint readiness and stable response shape.
- [x] 3.3 Add failure diagnostics that include tray stdout/stderr, diagnostics endpoint output, and config file path on test failure.

## 4. Desktop Validation

- [x] 4.1 Add prerequisite detection for real Windows desktop, foreground-window control, and global hotkey delivery.
- [x] 4.2 Add helpers to start the seam-provided fixture window and make it the foreground window.
- [x] 4.3 Add helpers to send `Ctrl+Alt+F9` using a real keyboard input path.
- [x] 4.4 Automate local-control foreground target capture response timing and persisted target shape.
- [x] 4.5 Automate hotkey foreground capture and verify target persistence without enabling remapping.
- [x] 4.6 Ensure unsupported desktop prerequisites report skipped or inconclusive results distinctly from passing tests.

## 5. Non-Evasive Scan

- [x] 5.1 Add source scan checks for forbidden invasive APIs and evasive implementation markers.
- [x] 5.2 Add project and publish-output inventory checks for drivers, services, elevated helper executables, overlays, injection helpers, and unexpected artifacts.
- [x] 5.3 Ensure scan failures produce actionable file/path evidence.

## 6. Runner Documentation

- [x] 6.1 Document how to run non-desktop integration tests locally.
- [x] 6.2 Document how to run desktop tests in a real Windows user session.
- [x] 6.3 Document why GitHub-hosted Windows runners may skip desktop-dependent tests.
- [x] 6.4 Document cleanup expectations for tray and fixture processes.

## 7. Automated Validation

- [x] 7.1 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 7.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 7.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 7.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 7.5 Run the non-desktop Windows integration suite.
- [x] 7.6 Run the desktop integration suite when a real Windows desktop session is available, or record the explicit skip/inconclusive reason.
- [x] 7.7 Run the non-evasive source and publish-output scan.
- [x] 7.8 Run `openspec.cmd validate add-windows-integration-validation-suite --strict`.
- [x] 7.9 Run `openspec.cmd validate --specs --strict`.
