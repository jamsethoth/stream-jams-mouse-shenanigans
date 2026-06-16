## 1. Dependency Gates

- [x] 1.1 Confirm `add-game-safety-guardrails` is complete and present in remote `main`, or explicitly scope safety-specific diagnostics to the currently available runtime behavior before implementation.
- [x] 1.2 Confirm no active implementation change already owns the same startup override, diagnostics, or fixture seams.
- [x] 1.3 Finalize override names for runtime config path, local-control URL, diagnostics output, and self-exit sentinel interval before coding.

## 2. Startup Override Options

- [x] 2.1 Add a startup/options model that reads supported override values once during tray startup.
- [x] 2.2 Add runtime configuration path override support while preserving production defaults when unset.
- [x] 2.3 Validate the configuration path override and report invalid values visibly without silently using production config.
- [x] 2.4 Add local-control URL override support while preserving loopback-only HTTP validation.
- [x] 2.5 Add self-exit sentinel interval override support with a safe production default.
- [x] 2.6 Add unit tests for unset overrides, valid overrides, invalid path values, invalid URL values, and invalid interval values.

## 3. Diagnostic Event Surface

- [x] 3.1 Add a bounded diagnostic event model with event type, timestamp, message, and optional captured identity fields.
- [x] 3.2 Add a diagnostic recorder abstraction that tray, local-control, configuration, safety, confirmation, and self-exit code can use without hard-coding test behavior.
- [x] 3.3 Record diagnostic events for configuration load/save errors, local-control startup failure, safety-blocked enable, foreground confirmation request, confirmation accept/cancel, and self-exit request.
- [x] 3.4 Expose recent diagnostic events through a loopback-only local-control diagnostics endpoint.
- [x] 3.5 Add tests for bounded history behavior, diagnostic event shape, recorded safety/confirmation/self-exit events, and diagnostics endpoint JSON.

## 4. Test Window Fixture

- [x] 4.1 Add a Windows-only fixture project or test utility that opens a normal visible window with stable title and process identity.
- [x] 4.2 Add a readiness signal so integration tests can wait until the fixture window exists before sending focus or hotkey actions.
- [x] 4.3 Ensure the fixture remains running when MouseShenanigans exits because of self-exit validation.
- [x] 4.4 Exclude the fixture from normal tray publish output.
- [x] 4.5 Add fixture smoke tests where practical without requiring global hotkey delivery.

## 5. Documentation And Validation

- [x] 5.1 Document each override as a local validation seam, not as a public remote API.
- [x] 5.2 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 5.3 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 5.4 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 5.5 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 5.6 Run `dotnet publish src\MouseShenanigans.Tray\MouseShenanigans.Tray.csproj --configuration Release --no-restore`.
- [x] 5.7 Run `openspec.cmd validate add-integration-testability-seams --strict`.
- [x] 5.8 Run `openspec.cmd validate --specs --strict`.
