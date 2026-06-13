## Context

The runtime proof of concept currently composes `RuntimeRemappingOptions` from `RuntimeProofOfConceptDefaults`, which hard-codes `Streamer.bot.exe` and a horizontal inversion profile. Core already supports named remapping profiles and JSON profile parsing, but the tray app has no file-backed configuration or profile selection surface.

This slice turns the POC into a configurable utility without jumping to a full settings editor or external automation API. It should land after `add-runtime-hotkeys` so it can extend the shared runtime command boundary instead of creating a second control path.

## Goals / Non-Goals

**Goals:**
- Load runtime configuration from a deterministic per-user app data JSON file.
- Preserve a fallback config equivalent to today's Streamer.bot horizontal inversion behavior without adding pass-through profile modes.
- Validate target selection, active profile name, profile definitions, and cursor-lock default before applying a config.
- Show available profiles in a tray submenu and allow active profile switching while the runtime is running.
- Persist the selected active profile when changed from the tray.
- Add a reload command to re-read configuration after manual JSON edits.
- Keep the last known good runtime configuration if reload fails.

**Non-Goals:**
- No profile editor UI.
- No configurable hotkeys.
- No local HTTP/IPC automation endpoint.
- No multi-target configuration.
- No remote/cloud configuration sync.
- No migration framework beyond this initial config shape.

## Decisions

### Store one runtime config file under per-user app data
Use a deterministic Windows per-user path such as `%APPDATA%\StreamJams\MouseShenanigans\config.json`. This avoids requiring installation-directory write access and keeps stream-specific configuration local to the Windows user session.

Alternative considered: load from the current working directory. That is easier for development but fragile for a tray app launched from shortcuts or startup entries.

### Use one JSON document for target, active profile, cursor lock default, and profiles
The runtime needs these values together to validate startup. A single document avoids ambiguous partial state across multiple files.

Suggested shape:

```json
{
  "target": {
    "processName": "Streamer.bot.exe",
    "windowTitleContains": null
  },
  "activeProfile": "horizontal-inversion",
  "cursorLockEnabled": false,
  "profiles": [
    {
      "name": "horizontal-inversion",
      "left": { "x": 1, "y": 0 },
      "right": { "x": -1, "y": 0 },
      "up": { "x": 0, "y": -1 },
      "down": { "x": 0, "y": 1 }
    }
  ]
}
```

Keep the document shape extensible for a later top-level `hotkeys` section, but do not parse or apply configurable hotkeys in this slice. The hotkey slice should provide the binding/provider boundary; a later configuration slice can decide the exact JSON grammar, validation rules, and reload behavior for user-defined chords.

### Keep fallback behavior editable without pass-through modes
If the config file does not exist, the tray should still start with the existing default target and horizontal inversion behavior. The app should not provide a built-in `normal` pass-through profile because non-remapped movement is already represented by disabling the runtime. Horizontal inversion should be part of the fallback/default configured profile data written to the editable JSON file. The implementation may create a default config file for discoverability, but startup must not depend on that write succeeding.

### Apply profile changes through runtime commands
Profile selection and config reload should extend the shared runtime command boundary from `add-runtime-hotkeys`. That keeps tray, hotkey, and later localhost commands aligned around one state-transition path.

### Keep invalid reload fail-safe
If reloading the JSON file fails validation, the app should keep using the last known good configuration, report the error in tray-visible status, and avoid partially applying target/profile changes.

## Risks / Trade-offs

- Invalid JSON during a stream -> Keep the last known good config active and show a tray-visible error.
- Profile switch while remapping is active -> Update the runtime profile atomically and reset any movement accumulator/re-entry state that depends on the previous profile.
- Config write failure when persisting selected profile -> Keep the runtime selection active, report the write failure, and let the user resolve file permissions.
- Concurrent manual file edits -> Treat reload as explicit; do not add file watchers in this slice.
- Conflict with `add-runtime-hotkeys` changes -> Implement after hotkeys or rebase carefully because both slices touch tray composition and runtime command control.
