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

## Proposed App And Feature Set

The proposed app is a small C#/.NET Windows tray utility that:

- Runs in the background.
- Provides a global toggle hotkey (`Ctrl+Alt+F8`).
- Provides an emergency disable hotkey (`Ctrl+Alt+Shift+F8`).
- Persists named configuration profiles.
- Switches between profiles while the app is running.
- Targets a configured window by process name, window title, or selected window handle.
- Hooks low-level mouse movement through standard Windows APIs.
- Applies configured directional remapping only when the target window is active or under the cursor.
- Injects corrected cursor movement through standard Windows cursor/input APIs.
- Ignores its own injected movement to avoid feedback loops.
- Shows basic tray icon status for enabled and disabled states.
- Exposes a local control surface that external tools can call to toggle behavior, switch profiles, or apply selected config changes.

The project is intentionally scoped as a user-session desktop utility, not a service or driver. It should be easy to start, stop, disable in an emergency, and inspect while a stream is live.

## Profiles And External Control

Profiles should be first-class project concepts rather than separate ad hoc config files. Each profile should have a stable name, a directional remapping definition, and any profile-specific targeting or behavior options that prove useful after the first prototype.

The app should be able to switch profiles immediately while running. That matters for streaming workflows where mouse behavior might become part of a scene, channel-point redemption, chat command, or other live interaction.

The project should also explore a small local integration protocol that Streamer.bot could invoke as an action. Possible shapes include:

- A localhost REST API for simple commands such as enable, disable, toggle, select profile, and reload profiles.
- A localhost WebSocket API for low-latency commands and status updates.
- Another local IPC mechanism if it fits the Windows tray app model better.

The first implementation does not need to commit to a public remote API. The useful goal is a local, scriptable command surface with enough stability that Streamer.bot or another automation tool can drive it reliably during a stream.

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

The repository now contains the first .NET app foundation rather than only project notes. The current scaffold includes:

- `MouseShenanigans.Core`, a pure C# library for app logic that can be tested without Windows desktop APIs, including directional movement decomposition and pure remapping profile behavior.
- `MouseShenanigans.Windows`, a Windows-specific adapter project for Win32 integration boundaries including runtime remapping and global hotkey registration.
- `MouseShenanigans.Tray`, a WinForms tray executable with runtime enable/disable controls, cursor lock control, fixed global hotkeys, and tray-visible status.
- `MouseShenanigans.Core.Tests`, an xUnit test project covering directional delta decomposition and pure remapping profile behavior.
- GitHub Actions validation for restore, formatting, analyzers, build, tests, dependency review, and one CodeQL C# analysis path.

Profile file persistence, profile switching UI, and Streamer.bot control endpoints remain planned features. The current implementation is still intentionally narrow so the runtime remapping, tray control flow, and Windows boundary behavior can settle before broader configuration and automation work lands.

## Constraints And Risks

The first version should avoid driver-level implementation and use standard Windows APIs such as:

- `SetWindowsHookEx` with `WH_MOUSE_LL`
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

xUnit is used for the initial unit tests because the first automated coverage is pure domain behavior. Automated desktop UI, global input, and Streamer.bot integration tests are intentionally deferred until those runtime features exist and the right test harness is clearer.

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

## Manual Verification Boundary

From WSL or Docker, stop at restore, format, build, and non-desktop tests. The Windows tray launch check must be performed manually in a real Windows desktop session.

Actual desktop-session behavior remains manual for now. This includes tray behavior, global hotkeys, low-level mouse hooks, input injection, target-window gating, and Streamer.bot interaction. Automated coverage should stay focused on pure core logic, build validation, formatting, analyzers, and non-desktop tests until the runtime behavior exists and is stable.

## Current Status

This repository now includes the initial runtime proof of concept on top of the .NET/C# app foundation. The current slices define named directional remapping profiles, a horizontal inversion preset, JSON profile document parsing, a `Streamer.bot.exe`-targeted runtime, tray enable/disable and cursor-lock controls, and fixed global hotkeys (`Ctrl+Alt+F8` and `Ctrl+Alt+Shift+F8`). The next refinements should focus on persisted profile configuration, profile switching, and the preferred local control protocol for Streamer.bot integration.
