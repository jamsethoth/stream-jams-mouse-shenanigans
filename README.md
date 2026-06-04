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

## Initial Implementation Direction

The likely first implementation is a small C#/.NET Windows tray app that:

- Runs in the background.
- Provides a global toggle hotkey.
- Provides an emergency disable hotkey.
- Persists named configuration profiles.
- Switches between profiles while the app is running.
- Targets a configured window by process name, window title, or selected window handle.
- Hooks low-level mouse movement through standard Windows APIs.
- Applies configured directional remapping only when the target window is active or under the cursor.
- Injects corrected cursor movement through standard Windows cursor/input APIs.
- Ignores its own injected movement to avoid feedback loops.
- Shows basic tray icon status for enabled and disabled states.
- Exposes a local control surface that external tools can call to toggle behavior, switch profiles, or apply selected config changes.

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
- Toggle on/off hotkey.
- Emergency disable hotkey.
- JSON-based directional remapping config with named profiles.
- Runtime profile switching.
- Horizontal inversion preset.
- Basic tray icon status.
- A minimal local control endpoint or command mechanism for external automation.

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

## Current Status

This repository is at the project-intent stage. The next refinements should define the first target application, the default hotkeys, the initial profile/config schema, the preferred local control protocol for Streamer.bot integration, and the smallest proof of concept that can validate whether normal Win32 mouse interception is enough.
