# SiteWatch
CLean Architecture ASP.NET API with Custom Bulletproof React Native App

## Operations

- [Production runbook](docs/production-runbook.md)

## Local MediaMTX gateway

The local Docker Compose stack includes a pinned MediaMTX gateway for the
dashboard live-camera work. The API registers on-demand paths lazily and issues
short-lived, administrator-only WHEP bearer tickets. No camera path or source
URL is committed to the repository.

Local defaults are:

```text
WHEP signaling: http://127.0.0.1:8889/<path>/whep
WebRTC media:   127.0.0.1:8189/udp
Control API:    http://127.0.0.1:9997
```

The following non-secret Compose variables can override the defaults:

| Variable | Purpose | Default |
| --- | --- | --- |
| `MEDIAMTX_WEBRTC_SIGNALING_PORT` | Loopback signaling host port | `8889` |
| `MEDIAMTX_CONTROL_PORT` | Loopback Control API host port | `9997` |
| `MEDIAMTX_WEBRTC_MEDIA_PORT` | Matching container and host UDP media port | `8189` |
| `MEDIAMTX_WEBRTC_BIND_ADDRESS` | Host interface for UDP media | `127.0.0.1` |
| `MEDIAMTX_WEBRTC_ADVERTISED_HOSTS` | Comma-separated ICE candidate hosts | `127.0.0.1` |
| `MEDIAMTX_WEBRTC_ALLOWED_ORIGINS` | Comma-separated dashboard origins | Local Angular origins |
| `MEDIAMTX_AUTH_HTTP_ADDRESS` | MediaMTX-to-API authorization callback | `http://host.docker.internal:5293/internal/mediamtx/auth` |

When testing from another machine, set the bind address and advertised host to
values reachable by that browser, and override the API setting
`MediaMtx__PublicWhepBaseUrl` with the browser-reachable signaling origin. The
Control API remains loopback-only. The host and container UDP media ports must
remain identical unless the ICE candidate port strategy is changed.

The local API must be running on its established development listener at port
`5293`. Compose maps `host.docker.internal` through Docker's `host-gateway` so
the Linux container can call the private authorization endpoint. Control API
requests are authenticated with 30-second internal tickets; viewer tickets are
valid for initiating a connection for two minutes and are never placed in a
URL.
