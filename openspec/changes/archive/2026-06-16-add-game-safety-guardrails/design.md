## Context

The tray app is a Windows-only user-session utility that can observe mouse movement and write corrected cursor positions for a configured target window. That behavior is legitimate for local streaming experiments, but it is also the kind of input-observation and input-alteration surface that game anti-cheat systems can treat as sensitive.

The current runtime already has target-window gating, hotkeys, emergency disable, and a shared runtime command boundary. The in-progress profile configuration change adds file-backed target/profile configuration. This change adds a separate game safety layer whose goal is risk reduction: prevent accidental arming near games unless the user explicitly allowed that game, and terminate MouseShenanigans itself when a disallowed game appears.

## Goals / Non-Goals

**Goals:**
- Ship with an empty user-managed game allowlist.
- Require explicit local user configuration before a game target can be enabled.
- Keep a protected-game denylist for known anti-cheat-protected or online competitive titles.
- Fail closed before enabling mouse observation if game safety cannot prove the target is allowed.
- Monitor running processes while the tray app is running and self-exit when a non-allowed or protected game process is detected.
- Disable remapping, release cursor lock, and tear down mouse observation before process exit.
- Keep safety behavior transparent through tray status and logs.
- Preserve the non-evasive architecture: standard user-session Win32 APIs only, no drivers, no injection, no overlays, no game memory reads, no stealth behavior.

**Non-Goals:**
- No guarantee that any game or anti-cheat vendor will consider the app allowed.
- No attempt to bypass, hide from, tamper with, or evade anti-cheat software.
- No game-process termination; the app only exits its own process.
- No graphical allowlist editor in this slice.
- No cloud-managed protected-game list in this slice.
- No publisher-specific anti-cheat integrations or approvals.

## Decisions

### Use an empty allowlist as the only positive game authorization

Game behavior is denied by default. The shipped configuration contains no allowed games. A game can become eligible only when the user adds a local allowlist entry with process identity such as process name and, when practical, executable path or window-title constraints.

Alternative considered: ship a curated safe-game allowlist. That creates false confidence and will drift as games update anti-cheat behavior.

### Keep a separate protected-game denylist

The allowlist says what the user wants to permit; the denylist says what the app considers too risky by default. If a process matches the protected-game denylist, the default behavior is to block enable attempts or self-exit even if another target is configured.

Alternative considered: rely only on the allowlist. That protects against accidental unknown targets but does not protect the common failure mode where the tray app is left running while a known protected competitive game is launched.

### Classify game candidates conservatively but transparently

The MVP should support three classification sources:
- Built-in protected-game process names for high-risk known titles.
- User allowlist entries for games the user explicitly permits.
- Optional user-configured game library roots or process patterns for detecting non-whitelisted game launches.

If the app cannot read a process identity needed for a safety decision, it should fail closed for enable attempts and self-exit while armed.

Alternative considered: try to automatically detect every installed game. That is unreliable and adds noisy filesystem scanning. The safer MVP is explicit configuration with conservative fail-closed behavior where the app has enough evidence.

### Put safety checks before runtime arming

Tray enable, hotkey toggle-to-enable, and later local automation enable commands should call a safety gate before `Enable` starts mouse observation. A denied safety decision leaves the runtime disabled and records a status reason.

Alternative considered: start observation first and suppress output until a target matches. That still leaves the process observing input in contexts where it should not.

### Add a live self-exit sentinel

The sentinel monitors process start/running-process state on a timer or Windows process-start notification. On a denied game detection it calls the same emergency disable path used by hotkeys, releases cursor lock, unregisters mouse observation, writes a diagnostic status/log entry, and then requests tray shutdown.

Alternative considered: only disable remapping. Self-exit is stronger for the "I forgot the tray utility was running" case and reduces the chance that anti-cheat sees the utility continue to run next to a protected game.

### Keep the safety layer testable without desktop input

Policy evaluation, allowlist matching, denylist matching, command gating, and sentinel decisions should be pure or adapter-backed enough for automated tests. Real Windows process-monitoring and tray self-exit should remain manually validated.

## Risks / Trade-offs

- Protected-game list drift -> Treat the list as conservative and editable; document that it is not exhaustive.
- False positives from process names -> Prefer path constraints where available and show the matched rule in status/logs.
- False negatives for unknown games -> Allow user-configured game library roots or process patterns; do not claim universal game detection.
- Safety checks may interrupt legitimate streams -> Make the reason visible and keep allowlist entries explicit.
- Self-exit can look surprising -> Disable runtime and release cursor lock first, then log the exact reason before exiting.
- Implementing before profile configuration lands -> Keep the safety config boundary small and reconcile it with the runtime config file when both changes are active.
