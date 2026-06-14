## Context

The project goal includes Streamer.bot automation, but the app currently has only in-process tray controls. The two preceding roadmap slices should establish a shared runtime command boundary and runtime profile configuration. This slice exposes those existing commands over a localhost-only HTTP JSON surface so Streamer.bot can invoke them from actions.

The control surface is deliberately local automation, not a public API. It should bind only to loopback, start and stop with the tray app, and keep manual tray controls usable if the listener cannot start.

## Goals / Non-Goals

**Goals:**
- Host a local HTTP JSON control surface inside the tray process.
- Bind only to loopback addresses.
- Expose status, enable, disable, toggle, emergency disable, profile list, select profile, and reload configuration commands.
- Route endpoint behavior through the shared runtime command boundary from earlier slices.
- Return predictable JSON responses for Streamer.bot scripts.
- Report listener startup failure through tray-visible status while leaving tray/hotkey controls usable.
- Cover endpoint routing and command dispatch through automated tests without requiring Streamer.bot.

**Non-Goals:**
- No WebSocket push channel.
- No remote network binding.
- No profile editing endpoints.
- No public API compatibility promise beyond this local MVP.
- No authentication beyond loopback-only binding in this slice.
- No Streamer.bot export package or `.sb` action bundle yet.

## Decisions

### Use localhost HTTP JSON first
Streamer.bot can call HTTP endpoints easily from actions, and JSON responses are simple to inspect. This is a better first integration target than named pipes or WebSockets because it is visible, scriptable, and easy to manually test with `curl` or PowerShell.

Alternative considered: WebSocket. It may become useful for live status updates later, but request/response commands are enough for enable/disable/profile selection.

### Prefer an embedded Kestrel-style listener over `HttpListener`
The implementation should prefer a .NET-hosted loopback listener that does not require Windows URL ACL reservations. If the implementation uses ASP.NET Core/Kestrel, it should rely on framework references rather than extra package downloads where possible.

Alternative considered: `HttpListener`. It is small but can be awkward because HTTP.sys URL reservations can surprise non-admin desktop utilities.

### Keep responses envelope-shaped
Every endpoint should return a predictable JSON envelope with success/error information plus the current runtime snapshot when useful. Streamer.bot actions should not need to scrape text.

Suggested success response shape:

```json
{
  "ok": true,
  "state": "enabled",
  "cursorLockEnabled": false,
  "activeProfile": "horizontal-inversion",
  "profiles": ["horizontal-inversion"],
  "message": null
}
```

Suggested error response shape:

```json
{
  "ok": false,
  "error": "profile-not-found",
  "message": "Profile 'chaos' was not found."
}
```

### Make listener failure degraded, not fatal
If the configured/default local control URL cannot be bound, the tray app should keep running and report the failure. Hotkeys and tray controls remain usable.

## Risks / Trade-offs

- Port conflict -> Report degraded listener status and keep manual controls available.
- Security concerns -> Bind only to loopback and do not accept remote connections in this slice.
- Endpoint behavior drifts from tray/hotkeys -> Route every endpoint through the shared runtime/profile command boundary.
- Streamer.bot scripts depend on response shape -> Keep the response envelope small and stable within the local MVP.
- Hosting adds lifecycle complexity -> Start with tray process lifetime and dispose listener before runtime disposal on exit.
