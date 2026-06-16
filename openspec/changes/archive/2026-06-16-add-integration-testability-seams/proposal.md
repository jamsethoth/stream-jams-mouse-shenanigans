## Why

The safety guardrails feature needs reliable Windows validation, but the tray app currently relies on fixed user config paths, fixed local-control binding, production timer cadence, and transient UI/status state. Small explicit testability seams will let integration tests exercise real tray behavior without touching the user's normal config or depending on slow/brittle timing.

## What Changes

- Add supported test/runtime overrides for the runtime configuration file path and local-control bind URL.
- Add a stable diagnostics surface for recent safety, confirmation, local-control, and self-exit events that can be queried or read during validation.
- Add a configurable self-exit sentinel interval so integration tests can run quickly while production defaults remain unchanged.
- Add a minimal visible test-window fixture application with stable process name and window title for foreground capture, hotkey, allowlist, and self-exit validation.
- Keep all seams explicit, local-only, and non-invasive; no public remote API, no elevated automation requirement, and no production behavior change unless overrides are provided.

## Capabilities

### New Capabilities
- `integration-testability-seams`: Covers deterministic configuration/location overrides, local-control endpoint overrides, diagnostics, sentinel timing control, and the test-window fixture needed by Windows integration validation.

### Modified Capabilities
- `local-control-surface`: Adds an optional local diagnostics/status surface for validation and troubleshooting.
- `runtime-profile-configuration`: Adds a supported configuration path override for test and scripted validation runs.

## Impact

- Affects tray startup composition, runtime configuration path resolution, local-control host options, safety diagnostic/status reporting, and test utilities.
- Adds a small fixture app or test host under the test tree for interactive Windows validation.
- Establishes a dependency for `add-windows-integration-validation-suite`; that suite should not proceed until these seams are implemented and available on remote `main`.
