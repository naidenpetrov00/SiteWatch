---
name: client-auth-session
description: Maintain SiteWatch auth forms, sessions, guarded routes, logout, and secure-persistence decisions. Use when changing this capability in src/ClientApp.
---

# client-auth-session

Keep schemas/API/forms/context/layout boundaries. Clear protected query data on identity change. In-memory session is current behavior; request approval before adding expo-secure-store and never use AsyncStorage for tokens. Never expose credentials. Verify auth flows and guards; run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** Expo Router 6.0.23; React Hook Form 7.71.2; TanStack Query 5.90.21.  
**Technical validation:** 2026-07.
