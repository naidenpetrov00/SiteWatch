---
name: client-live-camera-streaming
description: Maintain SiteWatch RTSP/VLC streams, fullscreen, overlays, recording, and player lifecycle. Use when changing this capability in src/ClientApp.
---

# client-live-camera-streaming

Keep viewer orchestration thin and VLC behavior in the player. Treat RTSP URLs and credentials as secrets. Restore orientation/status/timers on close or unmount. Verify storage before recording and hand files to client-device-media. Use bounded visible recovery. Verify stream/fullscreen/recording interruptions; run npm run lint.

**Related:** other ClientApp skills as applicable.  
**Validated technologies:** React Native 0.81.4; VLC player 1.0.98; Expo Screen Orientation 9.0.8.  
**Technical validation:** 2026-07.
