---
name: client-mobile-testing
description: Add focused Jest/Jest-Expo behavior tests for ClientApp UI, routing, lifecycle, media, and PTZ. Use when changing this capability in src/ClientApp.
---

# client-mobile-testing

Test behavior rather than implementation. Mock native boundaries and use minimal providers. Assert PTZ stop after release/unmount/blur/background and media cleanup on close/failure. npm test is watch mode; only run one-shot tests with authorization. Run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** Jest 29.7.0; Jest Expo 54.0.17; TypeScript 5.9.3.  
**Technical validation:** 2026-07.
