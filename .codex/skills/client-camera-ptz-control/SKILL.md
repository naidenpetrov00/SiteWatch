---
name: client-camera-ptz-control
description: Maintain direct-camera PTZ tap/hold movement, stop pairing, digest requests, and safe diagnostics. Use when changing this capability in src/ClientApp.
---

# client-camera-ptz-control

Short press is relative move; hold starts motion; release, cancellation, unmount, blur, and background issue one matching stop. Centralize active command state and validate commands. Do not expand direct-device access without approval. Never log credentials or URLs. Verify all interruption paths; run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** React Native 0.81.4; TanStack Query 5.90.21; digest-fetch 3.1.1.  
**Technical validation:** 2026-07.
