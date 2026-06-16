## Context

The application safety guardrails need validation in a real Windows user session, but the tray app currently has production-oriented defaults: deterministic per-user config, a fixed local-control URL, production timer cadence, and transient tray/dialog status. Those defaults are correct for normal use, but they make integration tests brittle and risk touching the user's real configuration.

This change adds narrow testability seams before the Windows integration suite. The seams are not a new product surface; they are explicit startup/configuration hooks and diagnostics that keep validation isolated, observable, and repeatable.

## Goals / Non-Goals

**Goals:**
- Allow test and scripted runs to override the runtime configuration file path.
- Allow test and scripted runs to override the loopback local-control URL/port.
- Expose recent diagnostic events in a stable local-only form suitable for assertions.
- Allow the self-exit sentinel interval to be shortened for integration tests while keeping the production default.
- Provide a visible test-window fixture with stable identity for foreground focus and process matching tests.
- Keep production behavior unchanged when overrides are absent.

**Non-Goals:**
- No public remote API or non-loopback listener.
- No elevated helper, driver, service, input injection into third-party processes, or anti-cheat evasion.
- No replacement for unit tests or the future Windows integration suite.
- No graphical configuration editor.

## Decisions

### Use environment-backed startup overrides

Use explicit environment variables or equivalent host options that are read once during tray startup: configuration path, local-control URL, diagnostics path if file-backed diagnostics are included, and self-exit interval. This keeps tests simple because they can launch the published tray app with a controlled environment.

Alternative considered: command-line flags. That would also work, but environment variables compose better with process-launch APIs and avoid changing the user-facing tray invocation contract.

### Keep overrides constrained and validated

The local-control URL override must still be absolute HTTP and loopback-only. The config-path override must point to a file path, not a directory, and writes must continue using explicit UTF-8 encoding. Invalid overrides should fail visibly in diagnostics or tray status rather than falling back silently.

Alternative considered: accepting arbitrary URLs and paths for flexibility. That would create unnecessary risk and make failed tests harder to diagnose.

### Add a bounded diagnostic event surface

Record recent events for configuration load/save, local-control startup/failure, safety-blocked enable attempts, foreground allowlist confirmations, and self-exit requests. A bounded in-memory ring exposed through local control is sufficient; optional JSONL file output can be added if needed for process-exit assertions.

Alternative considered: relying only on trace output. Trace output is useful but is not stable enough for integration assertions after process shutdown.

### Add a visible test-window fixture

Create a small Windows test fixture app that opens a normal visible window with a stable title and process name, and can optionally remain alive after the tray exits. The fixture gives hotkey, foreground capture, allowlist, and self-exit tests a controlled third-party window without using real games or user applications. The later Windows integration suite should consume this fixture through harness launch/control helpers rather than defining a second fixture application.

Alternative considered: automate Notepad. Notepad is convenient but OS-version differences and localized titles make it less deterministic.

## Risks / Trade-offs

- Test-only overrides could leak into normal use -> Make override names explicit, document them as validation hooks, and keep production defaults unchanged.
- Diagnostics may accidentally become a public support API -> Keep it local-control-only and describe it as local diagnostics rather than a remote/public contract.
- Short sentinel intervals may cause flaky timing if applied globally -> Read the override at startup and scope it to the launched process.
- UI fixture might be mistaken for production functionality -> Place it under tests or a clearly named fixture project and exclude it from normal tray publish output.
