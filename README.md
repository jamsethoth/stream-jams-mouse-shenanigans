# Stream Jams Mouse Shenanigans

Stream Jams Mouse Shenanigans is an experimental Windows-only companion utility for Stream Jams. Its purpose is to toggle custom mouse movement remapping for a specific third-party application window.

The first target behavior is horizontal inversion, but the broader intention is configurable directional movement transformation. For example, a profile could make left movement slower, right movement faster, up movement become down movement, and down movement remain unchanged.

## Project Intention

This project is intended for existing third-party applications where the app source code cannot be modified. The utility should run quietly in the background, target one configured window or application, and let the user turn custom mouse behavior on or off without disrupting the rest of the desktop.

The utility should also support named, persisted configuration profiles. A streamer should be able to prepare several mouse behavior profiles ahead of time, then switch between them on the fly without restarting the app or editing configuration files mid-stream.

The first useful version should answer a narrow question:

Can a small tray app reliably intercept normal mouse movement, transform it, and re-inject corrected movement only while a chosen target window is active or under the cursor?

## Core Remapping Model

Mouse movement is treated as directional deltas:

```text
dx = horizontal mouse movement
dy = vertical mouse movement

left  = max(-dx, 0)
right = max(dx, 0)
up    = max(-dy, 0)
down  = max(dy, 0)
```

Each direction can then be mapped to a new output vector. A configuration might look like this:

```json
{
  "left":  { "x": -0.5, "y": 0 },
  "right": { "x": 2.0,  "y": 0 },
  "up":    { "x": 0,    "y": 1.0 },
  "down":  { "x": 0,    "y": 1.0 }
}
```

This keeps the initial idea simple while leaving room for presets such as horizontal inversion, directional scaling, axis swapping, or one-direction-only effects.

## App And Feature Set

The app is a small C#/.NET Windows tray utility that:

- Runs in the background.
- Provides a global toggle hotkey (`Ctrl+Alt+F8`).
- Provides an emergency disable hotkey (`Ctrl+Alt+Shift+F8`).
- Persists named configuration profiles.
- Switches between profiles while the app is running.
- Targets a configured window or application by process name, executable path, or window title text.
- Observes mouse movement through standard Windows user-session APIs.
- Applies configured directional remapping only when the target window is active or under the cursor.
- Injects corrected cursor movement through standard Windows cursor/input APIs.
- Ignores its own injected movement to avoid feedback loops.
- Shows basic tray icon status for enabled and disabled states.
- Exposes a local control surface that external tools can call to toggle behavior, switch profiles, or apply selected config changes.
- Applies game-safety guardrails so game targets are blocked unless explicitly allowlisted, protected game deny rules win over allowlist entries, and disallowed-game detection exits MouseShenanigans without touching the game process.

The project is intentionally scoped as a user-session desktop utility, not a service or driver. It should be easy to start, stop, disable in an emergency, and inspect while a stream is live.

## Profiles And External Control

Profiles should be first-class project concepts rather than separate ad hoc config files. Each profile should have a stable name, a directional remapping definition, and any profile-specific targeting or behavior options that prove useful after the first prototype.

The app should be able to switch profiles immediately while running. That matters for streaming workflows where mouse behavior might become part of a scene, channel-point redemption, chat command, or other live interaction.

The project exposes a small loopback-only HTTP JSON control surface that Streamer.bot can invoke as actions. It supports runtime enable, disable, toggle, emergency disable, status, diagnostics, foreground target capture, foreground safety allowlist capture, profile listing, active profile selection, and configuration reload.

The app does not expose a public remote API. WebSockets, named pipes, remote access, and profile editing endpoints are outside the current scope unless a later streaming workflow needs them.

## MVP Scope

The minimum useful version should support:

- One configured target window or application.
- Toggle on/off hotkey (`Ctrl+Alt+F8`).
- Emergency disable hotkey (`Ctrl+Alt+Shift+F8`).
- JSON-based directional remapping config with named profiles.
- Runtime profile switching.
- Horizontal inversion preset.
- Basic tray icon status.
- A minimal local control endpoint or command mechanism for external automation.

## Current Foundation

The repository now contains the .NET app foundation and the current runtime proof of concept. The current implementation includes:

- `MouseShenanigans.Core`, a pure C# library for app logic that can be tested without Windows desktop APIs, including directional movement decomposition and pure remapping profile behavior.
- `MouseShenanigans.Windows`, a Windows-specific adapter project for runtime remapping, target-window inspection, cursor output, cursor lock, runtime configuration, global hotkeys, diagnostics, and game-safety policy.
- `MouseShenanigans.Tray`, a WinForms tray executable with runtime enable/disable controls, cursor lock control, profile switching, configuration reload, target capture, safety allowlist capture, fixed global hotkeys, local-control hosting, and tray-visible status.
- `MouseShenanigans.TestWindowFixture`, a Windows-only test fixture app for desktop validation.
- Unit and integration test projects covering core behavior, Windows adapter behavior, tray behavior, local-control behavior, published-app validation, desktop-gated validation, and non-evasive safety scans.
- GitHub Actions validation for restore, formatting, analyzers, build, tests, OpenSpec specs, dependency review, and one CodeQL C# analysis path.

## Constraints And Risks

The first version should avoid driver-level implementation and use standard Windows APIs such as:

- Raw Input mouse observation
- `RegisterHotKey`
- `GetForegroundWindow`
- `WindowFromPoint`
- `GetWindowThreadProcessId`
- `SetCursorPos` or `SendInput`

Some applications may not behave well with this approach, especially:

- Games using Raw Input or DirectInput.
- Apps with anti-cheat protections.
- Apps running as administrator when the utility is not elevated.
- Software that captures or recenters the cursor.

If standard Win32 input interception is not reliable for the target application, a more advanced driver-level approach may be needed later. That is deliberately out of scope for the first pass.

## Tech Stack Decisions

The selected foundation is .NET 10 with C#. This is the current LTS line for new .NET work and gives the project strong Windows desktop support, straightforward Win32 interop options, nullable reference types, analyzers, and a mature test ecosystem.

Windows-specific projects target `net10.0-windows`. The tray host uses WinForms because `NotifyIcon` and the WinForms message loop are a direct fit for a notification-area utility. WPF or WinUI may become useful for a richer settings UI later, but they add complexity before the app needs it.

The solution is split into small projects so core behavior can stay portable and testable:

- Core logic belongs in `MouseShenanigans.Core`.
- Win32 and desktop integration boundaries belong in `MouseShenanigans.Windows`.
- The tray executable and composition root belong in `MouseShenanigans.Tray`.
- Fast non-desktop unit tests belong in `MouseShenanigans.Core.Tests`.

The foundation also uses repository-level .NET configuration:

- `global.json` selects the intended .NET SDK line.
- `Directory.Build.props` centralizes compiler, analyzer, warnings-as-errors, deterministic build, and Windows targeting settings.
- `Directory.Packages.props` centralizes NuGet package versions.
- `.editorconfig` captures formatting and analyzer preferences used by `dotnet format`.

xUnit is used for unit and integration tests. Desktop-sensitive validation is split into explicit Windows integration test categories so normal CI can cover non-desktop logic and local runs can opt into foreground-window and keyboard-input checks from a real Windows desktop session.

## Local Tooling

The app foundation targets .NET 10 and uses a Windows-oriented C# solution. Local development should use the .NET 10 SDK or the official .NET 10 SDK Docker image.

Common validation commands:

```bash
dotnet restore MouseShenanigans.slnx
dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore
dotnet build MouseShenanigans.slnx --configuration Release --no-restore
dotnet test MouseShenanigans.slnx --configuration Release --no-build
```

The same commands can be run from WSL through the official `mcr.microsoft.com/dotnet/sdk:10.0` Docker image when the .NET SDK is not installed directly in WSL.

When validating from WSL or another non-Windows environment, Windows-targeted project compilation relies on `EnableWindowsTargeting=true` from the shared build configuration. This validates compilation, analyzers, and non-desktop tests only.

## Local Validation Seams

The tray app has explicit local-only startup overrides for automated validation. They are not public remote API settings, and production defaults remain active when the variables are unset.

| Environment variable | Purpose |
| --- | --- |
| `MOUSE_SHENANIGANS_CONFIG_PATH` | Fully qualified runtime configuration file path for an isolated validation run. |
| `MOUSE_SHENANIGANS_LOCAL_CONTROL_URL` | Absolute HTTP loopback URL for the local-control listener, for example `http://127.0.0.1:6178`. |
| `MOUSE_SHENANIGANS_DIAGNOSTICS_PATH` | Optional fully qualified JSONL diagnostics output path. Diagnostics are also exposed through local control. |
| `MOUSE_SHENANIGANS_SELF_EXIT_SENTINEL_INTERVAL_MS` | Positive integer interval override, in milliseconds, for future self-exit sentinel validation. |

Invalid overrides are reported through tray/local-control status and diagnostics. Invalid configuration-path overrides do not fall back to the user's production configuration path.

### Local Control

The tray app hosts a loopback-only HTTP JSON control surface. It uses the configured `MOUSE_SHENANIGANS_LOCAL_CONTROL_URL` when set, otherwise it defaults to `http://127.0.0.1:5178`.

| Method | Path | Purpose |
| --- | --- | --- |
| `GET` | `/api/v1/status` | Return runtime state, cursor-lock state, target, active profile, profiles, and status message. |
| `GET` | `/api/v1/diagnostics` | Return bounded recent diagnostic events with stable `type`, `timestamp`, `message`, and optional `capturedIdentity` fields. |
| `POST` | `/api/v1/runtime/enable` | Enable runtime remapping when safety permits. |
| `POST` | `/api/v1/runtime/disable` | Disable runtime remapping and release cursor lock. |
| `POST` | `/api/v1/runtime/toggle` | Toggle runtime remapping. |
| `POST` | `/api/v1/runtime/emergency-disable` | Disable runtime remapping and release cursor lock without requiring a positive safety decision. |
| `POST` | `/api/v1/target/capture-foreground` | Persist the current foreground window identity as the runtime target. |
| `POST` | `/api/v1/safety/allowed-applications/capture-foreground` | Start confirmation for adding the foreground window identity to the safety allowlist. |
| `GET` | `/api/v1/profiles` | Return available profiles and the active profile. |
| `POST` | `/api/v1/profiles/select` | Select an active profile using `{ "name": "<profile-name>" }`. |
| `POST` | `/api/v1/config/reload` | Reload runtime configuration from disk. |

Common PowerShell calls:

```powershell
$base = 'http://127.0.0.1:5178'
Invoke-RestMethod "$base/api/v1/status"
Invoke-RestMethod "$base/api/v1/diagnostics"
Invoke-RestMethod "$base/api/v1/runtime/enable" -Method Post
Invoke-RestMethod "$base/api/v1/runtime/disable" -Method Post
Invoke-RestMethod "$base/api/v1/runtime/toggle" -Method Post
Invoke-RestMethod "$base/api/v1/runtime/emergency-disable" -Method Post
Invoke-RestMethod "$base/api/v1/profiles"
Invoke-RestMethod "$base/api/v1/profiles/select" -Method Post -ContentType 'application/json' -Body '{ "name": "horizontal-inversion" }'
Invoke-RestMethod "$base/api/v1/config/reload" -Method Post
```

Foreground capture endpoints require a real interactive desktop session with a usable foreground window. The local control surface is only for loopback automation such as Streamer.bot actions; it is not a public remote API.

The solution also includes `MouseShenanigans.TestWindowFixture`, a Windows-only fixture utility under `tests/`. It opens a normal visible window with a stable process name and default title, and it can write a readiness file when started with `--ready-file <path>`. It is a validation fixture only and is not included when publishing `src\MouseShenanigans.Tray\MouseShenanigans.Tray.csproj`.

## Manual Verification Boundary

From WSL or Docker, stop at restore, format, build, and non-desktop tests. The Windows tray launch check must be performed manually in a real Windows desktop session.

Desktop-session behavior is covered by explicit desktop-gated integration tests and still needs real-session verification when validating a release. This includes tray behavior, global hotkeys, Raw Input mouse observation, cursor output, target-window gating, cursor locking, game-safety self-exit behavior, and Streamer.bot interaction.

## Current Status

This repository now includes the runtime proof of concept on top of the .NET/C# app foundation. The current app supports named directional remapping profiles, a horizontal inversion fallback profile, persisted JSON runtime configuration, runtime profile switching, process/path/title target selection, foreground target capture, tray enable/disable and cursor-lock controls, fixed global hotkeys (`Ctrl+Alt+F8`, `Ctrl+Alt+Shift+F8`, `Ctrl+Alt+F9`, and `Ctrl+Alt+Shift+F9`), loopback HTTP local control, diagnostics, Windows integration validation seams, and game-safety guardrails.
