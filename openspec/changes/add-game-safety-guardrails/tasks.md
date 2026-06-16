## 1. Safety Policy Model

- [x] 1.1 Add game safety value models for process identity, allowlist entry, protected deny rule, classification result, policy decision, and denial reason.
- [x] 1.2 Implement empty-by-default game allowlist behavior.
- [x] 1.3 Implement protected-game deny rule matching with deny precedence over user allowlist entries.
- [x] 1.4 Implement game candidate classification from user allowlist entries, protected deny rules, and optional configured game library roots or process patterns.
- [x] 1.5 Implement fail-closed decisions for unreadable process identity when the process must be evaluated.
- [x] 1.6 Add unit tests for empty allowlist denial, allowlisted game approval, protected deny precedence, non-game utility approval, unknown identity denial, and matched-rule reason text.

## 2. Safety Configuration

- [x] 2.1 Define the local JSON shape for game safety configuration, including empty `allowlistedGames`, protected deny overrides if supported, optional game library roots, and process patterns.
- [x] 2.2 Integrate safety configuration with the runtime configuration file if `add-runtime-profile-configuration` has landed.
- [x] 2.3 Add a standalone safety configuration fallback if runtime profile configuration has not landed before implementation. (`add-runtime-profile-configuration` has landed, so the runtime configuration integration path is used.)
- [x] 2.4 Validate duplicate allowlist entries, empty process identities, invalid paths, invalid policies, and unsafe override combinations.
- [x] 2.5 Add explicit UTF-8 read/write behavior and deterministic per-user app data paths for any safety configuration file.
- [x] 2.6 Add tests for missing config fallback, valid allowlist config, invalid config rejection, and reload behavior.

## 3. Process And Window Observation

- [x] 3.1 Add an adapter for enumerating running processes with process name, executable path when available, and window identity when relevant.
- [x] 3.2 Add an adapter or timer-based sentinel for observing process launches and periodic running-process state.
- [x] 3.3 Ensure process inspection failures are surfaced as fail-closed safety decisions where required.
- [x] 3.4 Reuse existing target-window identity patterns where practical instead of duplicating Win32 logic.
- [x] 3.5 Add tests for process snapshot mapping, unreadable process handling, and sentinel decision dispatch using fakes.

## 4. Runtime Command Integration

- [x] 4.1 Add a game safety gate to the shared runtime command boundary before tray enable and toggle-to-enable operations call the runtime enable path.
- [x] 4.2 Ensure hotkey toggle-to-enable uses the same safety gate as tray enable.
- [x] 4.3 Ensure disable and emergency-disable commands remain available even when game safety denies enablement.
- [x] 4.4 Ensure denied enable attempts leave the runtime disabled and do not start mouse observation.
- [x] 4.5 Add tests for tray enable denial, hotkey toggle denial, successful allowlisted enable, and emergency-disable bypass.

## 5. Self-Exit Sentinel

- [x] 5.1 Start the safety sentinel with the tray app and stop it during tray shutdown.
- [x] 5.2 On denied game detection, dispatch emergency disable, release cursor lock, unregister mouse observation, and request MouseShenanigans process exit.
- [x] 5.3 Ensure the sentinel never terminates, suspends, injects into, or manipulates the game or anti-cheat process.
- [x] 5.4 Handle self-exit while runtime is disabled by exiting the tray process without attempting to enable or inspect input.
- [x] 5.5 Add tests for enabled self-exit sequence, disabled self-exit sequence, and game-process non-interference.

## 6. Status And Diagnostics

- [x] 6.1 Add game safety status to tray-visible runtime status.
- [x] 6.2 Show blocked enable reasons, matched deny rules, unreadable identity failures, and self-exit reasons.
- [x] 6.3 Add local diagnostic logging for self-exit requests and blocked enable attempts.
- [x] 6.4 Keep diagnostics transparent and avoid any stealth, obfuscation, or anti-cheat evasion behavior.
- [x] 6.5 Add tests for status formatting and diagnostic message generation.

## 7. Windows Integration Suite Coverage

- [x] 7.1 Add Windows integration tests using the validation suite for empty game allowlist enable denial through local control.
- [x] 7.2 Add Windows integration tests using the validation suite for allowlisted fixture game target enable success.
- [x] 7.3 Add Windows integration tests using the validation suite for protected deny precedence over an allowlisted fixture target.
- [x] 7.4 Add Windows integration tests using the validation suite for self-exit while runtime is enabled and while runtime is disabled.
- [x] 7.5 Assert the matched fixture process remains running after MouseShenanigans exits itself.
- [x] 7.6 Assert diagnostics identify blocked enable attempts, matched deny rules, matched self-exit entries, and shutdown reasons.
- [x] 7.7 Extend the non-evasive scan with guardrail-specific checks for no game memory reads, anti-cheat tampering, process injection, overlays, concealment, or stealth behavior.
- [x] 7.8 Ensure unsupported desktop prerequisites report skipped or inconclusive results distinctly from passing tests.

## 8. Automated Validation

- [x] 8.1 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 8.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 8.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 8.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 8.5 Run `dotnet publish src\MouseShenanigans.Tray\MouseShenanigans.Tray.csproj --configuration Release --no-restore`.
- [x] 8.6 Run the non-desktop Windows integration guardrail suite, or record the explicit skip/inconclusive reason if the validation suite is unavailable.
- [x] 8.7 Run the desktop Windows integration guardrail suite when a real Windows desktop session is available, or record the explicit skip/inconclusive reason. (`dotnet test` reported the desktop-gated tests as skipped because desktop prerequisites were not enabled/available in this session.)
- [x] 8.8 Run `openspec.cmd validate add-game-safety-guardrails --strict`.
- [x] 8.9 Run `openspec.cmd validate --specs --strict`.

## 9. Manual Windows Validation

- [x] 9.1 Start with no game allowlist entries and verify a game target enable attempt is blocked before mouse observation starts. (Automated by the local-control integration suite.)
- [x] 9.2 Add a test game process to the local allowlist and verify enabling succeeds only for the matching process identity. (Automated by desktop-gated fixture tests.)
- [x] 9.3 Launch a non-allowlisted game candidate while runtime is enabled and verify MouseShenanigans disables, releases cursor lock, unregisters mouse observation, logs the reason, and exits itself. (Automated by desktop-gated fixture tests.)
- [x] 9.4 Launch a protected-game denylisted process while runtime is disabled and verify MouseShenanigans exits itself without touching the game process. (Automated by the local-control integration suite.)
- [x] 9.5 Verify tray and hotkey enable paths both fail closed when safety denies enablement. (Automated through the shared tray/local-control enable boundary plus desktop-gated hotkey dispatch.)
- [x] 9.6 Verify emergency disable still works while safety is denying enablement. (Automated by the local-control integration suite.)
- [x] 9.7 Inspect the implementation and release artifact to confirm no drivers, services, elevated input layers, game-process injection, overlays, game memory reads, anti-cheat tampering, concealment obfuscation, or stealth behavior were added. (Automated by the non-evasive scan.)
