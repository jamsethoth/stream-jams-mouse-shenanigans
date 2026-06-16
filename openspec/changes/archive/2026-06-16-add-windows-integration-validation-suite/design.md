## Context

The published tray app has a manual Windows checklist because foreground focus, global hotkeys, localhost REST timing, and process lifecycle behavior need a real Windows user session. Unit tests cover policy and command seams, but they cannot prove the published tray app behaves correctly when launched as a real process with visible windows.

This change adds a Windows integration suite after `add-integration-testability-seams` is implemented. The suite should run against the published tray app with isolated config and the stable fixture window provided by that seams change, and it should clearly separate tests that can run in any Windows process from tests that require a desktop session.

## Goals / Non-Goals

**Goals:**
- Launch the published tray app with temp config, test local-control URL, and diagnostics.
- Automate reusable tray, local-control, fixture-window, and desktop-prerequisite checks where a Windows desktop session is available.
- Validate local-control responses return promptly and reflect foreground target capture outcomes.
- Validate global hotkey foreground capture through a controlled foreground test window.
- Validate non-evasive implementation constraints through static and release-artifact checks.
- Report skipped/inconclusive desktop tests distinctly from passed tests.

**Non-Goals:**
- No hosted CI guarantee on GitHub-hosted Windows runners if they lack a real desktop session.
- No use of real games, anti-cheat-protected applications, or user production config.
- No replacement for a final human smoke test in the user's streaming setup.
- No broad UI automation framework for normal product use.

## Decisions

### Gate on testability seams

Do not implement the suite until `add-integration-testability-seams` is completed and present on remote `main`. The suite depends on isolated config, test local-control binding, diagnostics, and the fixture window.

Alternative considered: build the suite directly against production defaults. That would risk mutating user config and make timing/status assertions fragile.

### Split non-desktop and desktop Windows integration tests

Non-desktop tests can launch the tray and call local-control endpoints. Desktop tests require a visible desktop, foreground-window control, and global hotkey delivery. The suite should tag or otherwise separate these groups so a runner without desktop prerequisites can skip the desktop group without reporting a false pass.

Alternative considered: force all tests to require a desktop session. That would make routine validation harder and obscure which behavior is actually desktop-dependent.

### Drive the published app as an external process

Use `dotnet publish` output or an equivalent built artifact as the test target, not in-process tray classes. This validates startup composition, configuration path overrides, local-control host behavior, diagnostics, and process shutdown behavior closer to real use.

Alternative considered: in-process integration tests. Those are easier but duplicate existing unit/integration seams and miss process lifecycle behavior.

### Use category-filtered desktop automation with automatic prerequisite skips

Use the test-window fixture owned by `add-integration-testability-seams` for focus and process identity. This suite owns only the harness helpers that launch, focus, observe, and clean up that fixture during tests. Use Win32 focus/hotkey helpers to exercise the foreground capture hotkey. Keep those dependencies in the test project only. Tag desktop-dependent tests separately and automatically skip or report them as inconclusive when desktop prerequisites are unavailable.

Alternative considered: hand-written polling against arbitrary window titles. That is more brittle and gives worse failure diagnostics.

## Risks / Trade-offs

- Desktop automation is inherently environment-sensitive -> Detect prerequisites up front and mark unsupported tests as skipped/inconclusive with reasons.
- Tray process cleanup failure can poison later tests -> Use unique ports/config paths and force-kill only the tray test process during cleanup.
- A static scan can miss all unsafe behavior -> Combine source-term checks, project/publish artifact inventory, and targeted forbidden API scans.
