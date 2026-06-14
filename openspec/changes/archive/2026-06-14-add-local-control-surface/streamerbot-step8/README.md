# Mouse Shenanigans Local Control Streamer.bot Import

This folder contains the source manifest and C# action files used to generate the step 8 Streamer.bot `.sb` import.

The generated import creates manual Streamer.bot actions in the `Mouse Shenanigans Local Control` group. Each action calls one local HTTP endpoint on `http://127.0.0.1:5178` and logs the JSON response.

The generated `.sb` is written to:

```text
artifacts/manual-test/streamerbot-step8/mouse-shenanigans-local-control.sb
```

Regenerate it with the current `streamerbot-config` skill tooling:

```powershell
python C:\Users\James\.codex\skills\streamerbot-config\scripts\streamerbot_sb_import_gen.py openspec\changes\add-local-control-surface\streamerbot-step8 artifacts\manual-test\streamerbot-step8\mouse-shenanigans-local-control.sb --stub C:\Users\James\.codex\skills\streamerbot-config\scripts\fixtures\streamerbot-import-stub.sb
```

Import the `.sb` into a disposable Streamer.bot profile first, compile the C# sub-actions, then run each action manually while the tray app is running.
