## 1. Command Response Contract

- [x] 1.1 Add JSON response DTOs for successful runtime snapshots and command errors.
- [x] 1.2 Include runtime state, cursor-lock state, active profile, available profiles, and degraded status message in status snapshots.
- [x] 1.3 Map validation failures such as missing profile names and unknown profile names to stable error codes.
- [x] 1.4 Add unit tests for success response shape, error response shape, and profile-not-found response behavior.

## 2. Local Listener Lifecycle

- [x] 2.1 Add a local control host boundary that starts and stops with the tray process.
- [x] 2.2 Bind only to loopback addresses using a documented default local URL.
- [x] 2.3 Ensure listener startup failure is reported through tray-visible degraded status without preventing tray startup.
- [x] 2.4 Ensure listener disposal stops accepting requests before runtime disposal completes.
- [x] 2.5 Add tests for successful startup, startup failure, loopback-only configuration, and disposal behavior through seams.

## 3. Runtime Command Endpoints

- [x] 3.1 Add `GET /api/v1/status` endpoint returning the runtime snapshot response.
- [x] 3.2 Add `POST /api/v1/runtime/enable` endpoint routed through the shared runtime command boundary.
- [x] 3.3 Add `POST /api/v1/runtime/disable` endpoint routed through the shared runtime command boundary.
- [x] 3.4 Add `POST /api/v1/runtime/toggle` endpoint routed through the shared runtime command boundary.
- [x] 3.5 Add `POST /api/v1/runtime/emergency-disable` endpoint routed through the shared runtime command boundary.
- [x] 3.6 Add endpoint tests for command dispatch, cursor-lock release through disable/emergency disable, and status responses.

## 4. Profile And Configuration Endpoints

- [x] 4.1 Add `GET /api/v1/profiles` endpoint returning loaded profile names and active profile.
- [x] 4.2 Add `POST /api/v1/profiles/select` endpoint accepting a JSON profile name.
- [x] 4.3 Reject missing or unknown profile names without changing the active profile.
- [x] 4.4 Add `POST /api/v1/config/reload` endpoint routed through the shared reload command.
- [x] 4.5 Add endpoint tests for profile list, select profile success, select profile failure, reload success, and reload validation failure.

## 5. Tray Integration

- [x] 5.1 Compose the local control host in `TrayApplicationContext` after runtime command/profile services exist.
- [x] 5.2 Include local control availability or failure in tray-visible status.
- [x] 5.3 Dispose the local control host during tray shutdown before or alongside runtime disposal.
- [x] 5.4 Add tray/controller tests for listener degraded status and shutdown disposal ordering where practical.

## 6. Automated Validation

- [x] 6.1 Run `dotnet restore MouseShenanigans.slnx`.
- [x] 6.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [x] 6.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [x] 6.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [x] 6.5 Run `openspec validate add-local-control-surface --strict`.

## 7. Manual Windows And Streamer.bot Verification

- [ ] 7.1 Launch the tray app in a real Windows desktop session and verify the local control URL is available on loopback only.
- [ ] 7.2 Call `GET /api/v1/status` from PowerShell, curl, or Streamer.bot and verify JSON status is parseable.
- [ ] 7.3 Call enable, disable, toggle, and emergency-disable endpoints while Streamer.bot is focused and verify runtime behavior matches tray/hotkey commands.
- [ ] 7.4 Call profile list and select profile endpoints after profile configuration exists and verify remapping switches profiles.
- [ ] 7.5 Call config reload endpoint with valid and invalid config files and verify success/error responses and last-known-good behavior.
- [ ] 7.6 Verify listener failure behavior by forcing a port conflict or invalid URL and confirming tray/manual controls remain usable.
- [ ] 7.7 Record any Streamer.bot HTTP action, local firewall, loopback, port, or JSON parsing limitation found during manual testing.
