# Windows Integration Tests

These tests launch the published MouseShenanigans tray app as an external process with an isolated runtime config path, a test-selected loopback local-control URL, and a diagnostics JSONL path.

## Non-Desktop Tests

Run from the repository root after restore:

```powershell
dotnet test tests\MouseShenanigans.WindowsIntegration.Tests\MouseShenanigans.WindowsIntegration.Tests.csproj --configuration Release --filter "Category=NonDesktop|Category=NonEvasiveScan"
```

The harness publishes the tray app with `dotnet publish --no-restore` unless `MOUSE_SHENANIGANS_TRAY_ARTIFACT_PATH` points to an existing tray executable.

## Desktop Tests

Desktop tests require a real Windows user session with foreground-window control and keyboard input delivery. They are category-filtered and skipped unless explicitly enabled:

```powershell
$env:MOUSE_SHENANIGANS_RUN_DESKTOP_TESTS = '1'
dotnet test tests\MouseShenanigans.WindowsIntegration.Tests\MouseShenanigans.WindowsIntegration.Tests.csproj --configuration Release --filter "Category=Desktop"
Remove-Item Env:\MOUSE_SHENANIGANS_RUN_DESKTOP_TESTS
```

The fixture app is published automatically unless `MOUSE_SHENANIGANS_TEST_WINDOW_FIXTURE_ARTIFACT_PATH` points to an existing fixture executable.

## GitHub-Hosted Windows Runners

GitHub-hosted Windows runners may not provide the same desktop behavior as a signed-in local user session. Desktop-dependent tests therefore use a separate category and report a skip reason when prerequisites are missing instead of silently passing unsupported automation.

## Cleanup

Each test uses unique temp directories, local-control ports, config paths, and diagnostics paths. Cleanup kills only tray and fixture processes started by the harness and then removes the temp directories. A normal user-running tray instance can still block the app's single-instance guard, so close the normal tray app before running the suite locally.
