# Add Local Control Surface Manual Test Plan

## Scope

This plan verifies the remaining manual OpenSpec tasks for `add-local-control-surface`.

Run these checks in a real Windows desktop session. Do not run them from WSL, Docker, or a non-interactive CI session because the tray icon, global input hooks, and Streamer.bot focus behavior require an interactive desktop.

## Build Under Test

Use the published tray executable:

```powershell
C:\Users\James\.codex\worktrees\3ced\stream-jams-mouse-shenanigans\artifacts\manual-test\add-local-control-surface-shutdown-fix\MouseShenanigans.Tray.exe
```

Default local control URL:

```text
http://127.0.0.1:5178
```

Runtime configuration path:

```powershell
$ConfigPath = Join-Path $env:APPDATA "StreamJams\MouseShenanigans\config.json"
```

## 1. Prepare The Desktop Session

1. Close any visible Mouse Shenanigans tray app from the tray menu.
2. Verify no stale headless Mouse Shenanigans process is still running:

```powershell
Get-Process MouseShenanigans.Tray -ErrorAction SilentlyContinue | Select-Object Id,ProcessName,Path
```

Expected result:

- No process is listed.

If a process is listed but no tray icon is visible, stop the stale process before continuing:

```powershell
Get-Process MouseShenanigans.Tray -ErrorAction SilentlyContinue | Stop-Process -Force
```

3. Close any process that may already be listening on `127.0.0.1:5178`.
4. Start Streamer.bot normally.
5. Open PowerShell.
6. Set the base URL:

```powershell
$BaseUrl = "http://127.0.0.1:5178"
```

## 2. Launch The Tray App And Verify Loopback

1. Launch the published executable:

```powershell
Start-Process "C:\Users\James\.codex\worktrees\3ced\stream-jams-mouse-shenanigans\artifacts\manual-test\add-local-control-surface-shutdown-fix\MouseShenanigans.Tray.exe"
```

2. Confirm the app appears in the Windows notification area.
3. Confirm the tray status mentions local control availability or otherwise shows normal runtime status.
4. Confirm the listener responds on loopback:

```powershell
Invoke-RestMethod "$BaseUrl/api/v1/status"
```

Expected result:

- The response is JSON.
- `ok` is `true`.
- `state`, `cursorLockEnabled`, `target`, `activeProfile`, and `profiles` are present.

5. Confirm it does not bind to all interfaces:

```powershell
Get-NetTCPConnection -LocalPort 5178 -State Listen | Select-Object LocalAddress,LocalPort,State,OwningProcess
```

Expected result:

- The listener is on `127.0.0.1` or another loopback address.
- There is no `0.0.0.0` listener for port `5178`.

OpenSpec coverage: 7.1 and 7.2.

## 3. Verify Runtime Command Endpoints

Keep Streamer.bot focused for these checks.

1. Enable runtime remapping:

```powershell
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/runtime/enable"
```

Expected result:

- The response has `ok: true`.
- The response state is `enabled`, unless the desktop/session is unsupported.
- Tray status changes consistently with the enable command.
- With Streamer.bot focused and matching the configured target, visible mouse remapping starts applying. If the response reports `enabled` but movement is not remapped, record this as a failure.

2. Toggle runtime remapping:

```powershell
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/runtime/toggle"
```

Expected result:

- The response has `ok: true`.
- The state changes from the previous enabled/disabled state.
- Tray status changes consistently with the toggle command.
- If toggling turns the runtime on while Streamer.bot is focused and matching the configured target, visible mouse remapping starts applying. If the response reports `enabled` but movement is not remapped, record this as a failure.

3. Disable runtime remapping:

```powershell
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/runtime/disable"
```

Expected result:

- The response has `ok: true`.
- The response state is `disabled`.
- `cursorLockEnabled` may remain `true`; this is acceptable because cursor lock is only applied while runtime remapping is enabled and should remain configured unless explicitly changed.

4. Emergency-disable runtime remapping:

```powershell
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/runtime/emergency-disable"
```

Expected result:

- The response has `ok: true`.
- The response state is `disabled`.
- `cursorLockEnabled` may remain `true`; this is acceptable because cursor lock is only applied while runtime remapping is enabled and should remain configured unless explicitly changed.
- Tray status matches the disabled state.

5. Capture the current foreground target:

Run this command, then focus a non-PowerShell test window before the countdown finishes:

```powershell
Write-Host "Focus the target window now..."
Start-Sleep -Seconds 3
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/target/capture-foreground"
```

Expected result:

- The response has `ok: true`.
- The response `target` identifies the focused target process or window title.
- Tray status changes to the captured target without restarting the tray app.
- The target change is written to the runtime configuration file.

If the response captures PowerShell, Windows Terminal, or Streamer.bot unexpectedly, rerun the step with the intended target focused during the countdown.

OpenSpec coverage: 7.3.

## 4. Prepare Profile Configuration

1. Stop the tray app from its tray menu.
2. Back up any existing config:

```powershell
$ConfigPath = Join-Path $env:APPDATA "StreamJams\MouseShenanigans\config.json"
$BackupPath = "$ConfigPath.manual-test-backup"
if (Test-Path $ConfigPath) {
    Copy-Item $ConfigPath $BackupPath -Force
}
```

3. Write a two-profile test config:

```powershell
@'
{
  "target": {
    "processName": "Streamer.bot.exe",
    "windowTitleContains": null
  },
  "activeProfile": "horizontal-inversion",
  "cursorLockEnabled": true,
  "profiles": [
    {
      "name": "horizontal-inversion",
      "left": { "x": 1, "y": 0 },
      "right": { "x": -1, "y": 0 },
      "up": { "x": 0, "y": -1 },
      "down": { "x": 0, "y": 1 }
    },
    {
      "name": "double-right",
      "left": { "x": -1, "y": 0 },
      "right": { "x": 2, "y": 0 },
      "up": { "x": 0, "y": -1 },
      "down": { "x": 0, "y": 1 }
    }
  ]
}
'@ | Set-Content -Path $ConfigPath -Encoding utf8
```

4. Relaunch the tray app.
5. Confirm both profiles are listed:

```powershell
Invoke-RestMethod "$BaseUrl/api/v1/profiles"
```

Expected result:

- `ok` is `true`.
- `activeProfile` is `horizontal-inversion`.
- `profiles` includes `horizontal-inversion` and `double-right`.

## 5. Verify Profile Selection

1. Select `double-right`:

```powershell
Invoke-RestMethod -Method Post -ContentType "application/json" -Body '{"name":"double-right"}' "$BaseUrl/api/v1/profiles/select"
```

Expected result:

- `ok` is `true`.
- `activeProfile` is `double-right`.
- Tray/profile menu reflects `double-right`.
- With Streamer.bot focused and runtime enabled, rightward movement behavior matches the `double-right` profile.

2. Select `horizontal-inversion`:

```powershell
Invoke-RestMethod -Method Post -ContentType "application/json" -Body '{"name":"horizontal-inversion"}' "$BaseUrl/api/v1/profiles/select"
```

Expected result:

- `ok` is `true`.
- `activeProfile` is `horizontal-inversion`.
- Tray/profile menu reflects `horizontal-inversion`.
- With Streamer.bot focused and runtime enabled, horizontal movement is inverted.

3. Try an unknown profile:

```powershell
$MissingProfileBody = '{"name":"missing-profile"}'
try {
    Invoke-WebRequest -Method Post -ContentType "application/json" -Body $MissingProfileBody "$BaseUrl/api/v1/profiles/select"
} catch {
    $Response = $_.Exception.Response
    $Reader = [System.IO.StreamReader]::new($Response.GetResponseStream())
    [pscustomobject]@{
        StatusCode = [int]$Response.StatusCode
        Body = $Reader.ReadToEnd()
    }
    $Reader.Dispose()
}
```

Expected result:

- HTTP status is `400`.
- JSON has `ok: false`.
- `error` is `profile-not-found`.
- The active profile is unchanged.

OpenSpec coverage: 7.4.

## 6. Verify Config Reload

1. Confirm the current active profile:

```powershell
Invoke-RestMethod "$BaseUrl/api/v1/status"
```

2. Edit `$ConfigPath` and change `activeProfile` to the other valid profile.
3. Reload configuration:

```powershell
Invoke-RestMethod -Method Post "$BaseUrl/api/v1/config/reload"
```

Expected result:

- `ok` is `true`.
- `activeProfile` matches the edited config file.
- Runtime behavior changes without restarting the tray app.

4. Save invalid JSON:

```powershell
"{" | Set-Content -Path $ConfigPath -Encoding utf8
```

5. Reload configuration:

```powershell
try {
    Invoke-WebRequest -Method Post "$BaseUrl/api/v1/config/reload"
} catch {
    $Response = $_.Exception.Response
    $Reader = [System.IO.StreamReader]::new($Response.GetResponseStream())
    [pscustomobject]@{
        StatusCode = [int]$Response.StatusCode
        Body = $Reader.ReadToEnd()
    }
    $Reader.Dispose()
}
```

Expected result:

- HTTP status is `400`.
- JSON has `ok: false`.
- `error` is `configuration-reload-failed`.
- The app keeps the last-known-good runtime profile.
- Tray/manual controls remain usable.

OpenSpec coverage: 7.5.

## 7. Verify Listener Failure Behavior

1. Stop the tray app from its tray menu.
2. Start a temporary listener on the same port:

```powershell
$Listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Parse("127.0.0.1"), 5178)
$Listener.Start()
```

3. Launch the tray app again.
4. Confirm the tray app still starts.
5. Confirm tray-visible status reports local control failure or degraded local control state.
6. Confirm tray controls and hotkeys still work:

```text
Ctrl+Alt+F8 toggles runtime.
Ctrl+Alt+Shift+F8 emergency-disables runtime.
Tray enable/disable menu items still respond.
```

7. Stop the temporary listener:

```powershell
$Listener.Stop()
```

8. Exit and relaunch the tray app.
9. Confirm `GET /api/v1/status` works again.

OpenSpec coverage: 7.6.

## 8. Verify From Streamer.bot

Import this Streamer.bot bundle into a disposable profile first:

```text
artifacts/manual-test/streamerbot-step8/mouse-shenanigans-local-control.sb
```

It creates these manual actions in the `Mouse Shenanigans Local Control` group:

```text
MSLC - Configure Defaults
MSLC - Get Status
MSLC - Enable Runtime
MSLC - Disable Runtime
MSLC - Toggle Runtime
MSLC - Emergency Disable
MSLC - Capture Foreground Target
MSLC - Get Profiles
MSLC - Select Horizontal Inversion
MSLC - Select Double Right
MSLC - Reload Config
```

Expected result:

- The import preview shows 11 actions and no commands.
- Each imported action has one C# sub-action.
- Each C# sub-action compiles in Streamer.bot.
- `MSLC - Configure Defaults` initializes `mouseShenanigans.localControl.baseUrl` to `http://127.0.0.1:5178` if the global is missing.
- Streamer.bot can call each endpoint.
- Streamer.bot can read the JSON response.
- `MSLC - Capture Foreground Target` captures Streamer.bot when run directly from the Streamer.bot UI; use the delayed PowerShell step or a non-foreground trigger when verifying capture of another app.
- Streamer.bot does not require firewall prompts for loopback-only calls.
- Any Streamer.bot HTTP action limitation is recorded below.

OpenSpec coverage: 7.7.

## 9. Restore Local State

1. Stop the tray app.
2. Restore the config backup if one was created:

```powershell
if (Test-Path $BackupPath) {
    Copy-Item $BackupPath $ConfigPath -Force
}
```

3. Remove the backup after confirming the restored app state is correct:

```powershell
if (Test-Path $BackupPath) {
    Remove-Item $BackupPath
}
```

## Results Record

| Task | Result | Notes |
| --- | --- | --- |
| 7.1 Loopback launch | Passed | |
| 7.2 Status JSON | Passed | |
| 7.3 Runtime commands | Passed | |
| 7.3 Target capture | Passed | |
| 7.4 Profile endpoints | Passed | |
| 7.5 Config reload | Passed | |
| 7.6 Listener failure | Passed | |
| 7.7 Streamer.bot limitations | Passed | |
