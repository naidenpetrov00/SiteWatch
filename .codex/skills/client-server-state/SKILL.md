---
name: client-server-state
description: Maintain SiteWatch Axios and TanStack Query server-state flows. Use when changing this capability in src/ClientApp.
---

# client-server-state

Preserve lib/api-client and query defaults. Use stable hierarchical keys, smallest-prefix invalidation, typed request-plus-hook modules, and no duplicated server state. Add focus/online adapters only at app composition after approval. Never log secrets. Verify lifecycle/error/invalidation; run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** Axios 1.12.2; TanStack Query 5.90.21; Expo 54.0.12.  
**Technical validation:** 2026-07.
