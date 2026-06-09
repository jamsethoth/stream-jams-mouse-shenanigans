## 1. Target Boundary Model

- [ ] 1.1 Add a screen-rectangle value object for target window bounds and containment checks.
- [ ] 1.2 Extend `TargetWindowInfo` and `TargetWindowSnapshot` to carry readable target bounds and the current cursor position needed for boundary decisions.
- [ ] 1.3 Replace boolean target matching with a pure target eligibility result that distinguishes no match, inside bounds, outside bounds, and unreadable bounds.
- [ ] 1.4 Update target selector tests to cover foreground inside bounds, under-cursor inside bounds, foreground outside bounds, unreadable bounds, target mismatch, and re-entry.

## 2. Win32 Boundary Reading

- [ ] 2.1 Extend `TargetWindowReader` to read target window bounds with standard user-session Win32 APIs.
- [ ] 2.2 Keep bounds and cursor coordinates in one documented screen-coordinate policy using the full window rectangle.
- [ ] 2.3 Fail closed when target bounds cannot be read so remapping is paused rather than applied from process match alone.
- [ ] 2.4 Add or update adapter-focused tests where pure seams allow bounds and cursor-position behavior to be verified without a desktop session.

## 3. Runtime Gating And Cursor Lock

- [ ] 3.1 Update `AbsoluteCursorRemappingCoordinator` so it applies remapping only when target eligibility is inside bounds.
- [ ] 3.2 Ensure outside-bounds and unreadable-bounds movements pass through without corrected cursor output while the runtime remains enabled.
- [ ] 3.3 Add a cursor-lock runtime option that defaults to disabled.
- [ ] 3.4 Add a narrow cursor lock boundary for standard Win32 cursor constraint behavior, keeping native calls behind an interface.
- [ ] 3.5 Apply cursor lock only when the configured target is active and readable bounds are available.
- [ ] 3.6 Release cursor lock when the user disables lock, the runtime disables, the runtime fails, the runtime is disposed, target matching is lost, or target bounds become unavailable.
- [ ] 3.7 Add unit tests for lock apply, lock release, lock default-off behavior, and no-lock outside-bounds pass-through.

## 4. Tray Proof-Of-Concept Controls

- [ ] 4.1 Add a minimal checkable tray command for `Lock cursor to target` without introducing a full settings UI.
- [ ] 4.2 Wire the tray command to update the runtime cursor-lock option while preserving existing enable, disable, status, and exit controls.
- [ ] 4.3 Ensure disabling the runtime or exiting the tray releases any active cursor lock.
- [ ] 4.4 Add tray tests for lock toggle state and lock release through shutdown seams where practical.

## 5. Automated Validation

- [ ] 5.1 Run `dotnet restore MouseShenanigans.slnx`.
- [ ] 5.2 Run `dotnet format MouseShenanigans.slnx --verify-no-changes --no-restore`.
- [ ] 5.3 Run `dotnet build MouseShenanigans.slnx --configuration Release --no-restore`.
- [ ] 5.4 Run `dotnet test MouseShenanigans.slnx --configuration Release --no-build`.
- [ ] 5.5 Run `openspec validate add-target-boundary-controls --strict`.

## 6. Manual Windows Proof-Of-Concept Verification

- [ ] 6.1 Launch the tray app in a real Windows desktop session and verify the runtime still starts disabled with cursor lock off.
- [ ] 6.2 Enable remapping with cursor lock off, move the cursor outside the Streamer.bot window, and verify remapping pauses while normal movement can return the cursor to the target.
- [ ] 6.3 Re-enter the Streamer.bot window and verify remapping resumes automatically.
- [ ] 6.4 Enable cursor lock and verify the cursor remains constrained to the target window while the target is active.
- [ ] 6.5 Disable cursor lock and verify the cursor constraint is released immediately.
- [ ] 6.6 Switch focus away from the target and verify cursor lock releases and non-target movement passes through unchanged.
- [ ] 6.7 Disable the runtime and verify no movement is remapped and any cursor lock is released.
- [ ] 6.8 Exit the tray and verify `MouseShenanigans.Tray.exe` terminates and any cursor lock is released.
- [ ] 6.9 Repeat representative movement checks at low, medium, high, and previously tested high-DPI mouse settings.
- [ ] 6.10 Record any limitation involving target bounds, DPI scaling, captured cursors, cursor recentering, or lock release behavior.
