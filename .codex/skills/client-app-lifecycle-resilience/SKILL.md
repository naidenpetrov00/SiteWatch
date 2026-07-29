---
name: client-app-lifecycle-resilience
description: Maintain ClientApp focus, unmount, foreground/background, recovery, stream interruption, and PTZ safety behavior. Use when changing this capability in src/ClientApp.
---

# client-app-lifecycle-resilience

Define resource ownership. On blur/unmount/inactive/background clear timers, release native resources, and issue one idempotent PTZ stop when active. Use bounded visible recovery; remove listeners and guard late completions. Run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** React Native 0.81.4; TanStack Query 5.90.21; Expo Router 6.0.23.  
**Technical validation:** 2026-07.
