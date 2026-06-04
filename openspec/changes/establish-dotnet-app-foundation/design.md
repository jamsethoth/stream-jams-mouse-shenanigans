## Context

The repository currently contains project intent documentation and OpenSpec configuration, but no application code. The intended product is a Windows-only tray utility that will eventually use standard Win32 APIs for low-level mouse hooks, hotkeys, target-window detection, input injection, named profiles, and local automation for Streamer.bot.

This change establishes the implementation foundation only. It should create a maintainable .NET/C# solution structure, validation policy, CI workflow, and minimal tests before any risky Win32 behavior is implemented.

## Goals / Non-Goals

**Goals:**

- Establish a .NET 10/C# solution for a Windows-only tray app.
- Separate pure application/domain logic from Windows-specific interop and tray-hosting concerns.
- Add baseline test coverage for initial pure logic introduced by the foundation.
- Add repository-level C# build conventions: nullable reference types, implicit usings, analyzers, warnings-as-errors, central package management, and deterministic builds.
- Update CI so GitHub pull requests run .NET restore, formatting/analyzer validation, build, tests, dependency review, and one CodeQL security analysis path.
- Keep check names aligned with repository branch protection where practical: `validate`, `build`, `dependency-review`, and one CodeQL requirement.
- Document that real desktop-session behavior checks remain manual for now to avoid adding a premature Windows UI/input automation harness.

**Non-Goals:**

- Implement `WH_MOUSE_LL`, `RegisterHotKey`, `SendInput`, target-window detection, or injected-movement feedback-loop handling.
- Implement named profile persistence or runtime profile switching.
- Implement the tray menu beyond the minimum shell required for a compiling Windows app.
- Implement REST, WebSocket, named-pipe, or CLI automation endpoints.
- Add automated desktop UI tests, global input tests, WinAppDriver/Appium/FlaUI tests, or a self-hosted interactive Windows runner.
- Add installer, signing, auto-update, or release packaging.
- Add driver-level or elevated-input approaches.

## Decisions

### Use .NET 10 and C#

Use .NET 10 as the starting target because this is new Windows desktop work and .NET 10 is the current LTS line. Target Windows-specific projects as `net10.0-windows`.

Alternatives considered:

- .NET 8: mature LTS, but its support horizon is shorter for new work.
- .NET 9: STS release with no clear benefit for this project.
- Rust or C++: useful fallback options if managed Win32 interop proves inadequate, but slower for the first maintainable tray-app foundation.

### Use WinForms for the tray host

Use a WinForms executable for the tray app shell because `NotifyIcon` and the message pump are direct fits for a background notification-area app. The shell should be minimal and should delegate application behavior to services so the tray project does not become the domain layer.

Alternatives considered:

- WPF: better for a richer settings UI later, but unnecessary for the first tray shell.
- WinUI 3 / Windows App SDK: modern UI stack, but adds packaging/runtime complexity that does not help the MVP.
- Windows service: incorrect foundation for interactive user-session input hooks.

### Use a multi-project solution

Create a small solution with clear boundaries:

```text
src/
  MouseShenanigans.Core/
  MouseShenanigans.Windows/
  MouseShenanigans.Tray/
tests/
  MouseShenanigans.Core.Tests/
```

- `MouseShenanigans.Core`: pure domain/application logic such as directional delta decomposition and eventually profile models.
- `MouseShenanigans.Windows`: Windows-specific adapters and P/Invoke boundaries. For this change, it may contain only scaffolding.
- `MouseShenanigans.Tray`: Windows executable shell and composition root. It references Core and Windows.
- `MouseShenanigans.Core.Tests`: fast unit tests for Core behavior.

This avoids coupling testable logic to WinForms or P/Invoke from the start.

### Add shared repository build configuration

Use `global.json`, `Directory.Build.props`, and `Directory.Packages.props` so SDK versioning, compiler/analyzer settings, and package versions are explicit. Add C# settings to `.editorconfig` instead of relying on IDE defaults.

Baseline settings should include:

- nullable reference types enabled
- implicit usings enabled
- latest reasonable analyzer level for the target framework
- warnings as errors for project code
- deterministic builds
- central package management

### Validate with Windows CI

Run .NET CI on `windows-latest`. The tray and Windows adapter projects target Windows APIs, so a Windows runner avoids cross-targeting friction and gives the most representative build signal.

The first workflow should include:

- `validate`: restore, formatting/style verification, and analyzer validation.
- `build`: build the solution and run tests.
- `dependency-review`: run on pull requests.
- CodeQL: run C# analysis without introducing duplicate required CodeQL contexts.

The implementation should verify final GitHub check names after the first workflow run because CodeQL check naming can differ from job naming.

### Keep desktop behavior verification manual for now

Do not build a desktop automation harness in this foundation change. Automated tests should cover pure core logic and non-desktop state transitions only. Real tray behavior, global hotkeys, low-level mouse hooks, input injection, target-window gating, and Streamer.bot interaction should remain documented manual checks until the runtime behavior exists and has stabilized.

Alternatives considered:

- Dedicated Windows desktop integration harness with a controlled target window and synthetic input: valuable later, but too much complexity before the runtime hook/injection implementation exists.
- UI Automation, WinAppDriver/Appium, or FlaUI: useful for conventional UI workflows, but they do not directly answer the highest-risk question of whether low-level hook/injection behavior works in the user desktop session.
- Self-hosted interactive Windows runner: possible later, but operationally heavier than the current foundation requires.

## Risks / Trade-offs

- SDK availability risk -> Add `global.json` and document local tooling; CI installs the required SDK explicitly.
- Windows-only build constraints -> Run CI on Windows and avoid pretending Linux validates WinForms/P/Invoke behavior.
- Analyzer strictness can slow initial scaffold -> Keep warnings-as-errors, but avoid adding broad third-party analyzers until the first code shape settles.
- CodeQL check-name mismatch -> Verify check names after the first CI run and adjust branch protection or workflow names as a follow-up if needed.
- Manual desktop verification can miss regressions -> Keep the manual checklist short, explicit, and tied to behavior that cannot be validated meaningfully in ordinary CI yet.
- Foundation scope creep -> Keep this change to shell, project layout, CI, and baseline tests; defer runtime mouse and automation features to dedicated changes.

## Migration Plan

1. Add the .NET solution, projects, shared build props, and central package file.
2. Add the minimal Core behavior and tests needed to make the baseline meaningful.
3. Add the minimal WinForms tray shell that compiles but does not implement runtime mouse behavior.
4. Add or update CI workflows for validate, build/test, dependency review, and CodeQL.
5. Update README with local .NET commands and the manual desktop-verification boundary.
6. Run local validation where tooling is available and verify CI after pushing.

Rollback is straightforward while no runtime behavior exists: revert the scaffold and workflow changes.

## Open Questions

- Should the implementation install the .NET SDK in this environment, or should build verification be performed on Windows/CI only?
- Should the baseline Core test cover only directional delta decomposition, or should it also introduce an identity remapping profile model?
