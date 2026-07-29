---
name: client-device-media
description: Handle SiteWatch media permissions, files, recordings, snapshots, cache, album saves, and cleanup. Use when changing this capability in src/ClientApp.
---

# client-device-media

Request media permission from user action and stop when denied. Use documentDirectory for durable files and cacheDirectory for disposable files. Normalize URIs, verify existence, and clean every temporary artifact. Verify permission/save/album/cleanup paths; run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** Expo File System 19.0.21; Expo Media Library 18.2.1; Expo 54.0.12.  
**Technical validation:** 2026-07.
