# RTSP and PTZ latency measurements

The target platform for this comparison is the Android development build. The development app emits concise `[RTSP_METRIC]` and `[PTZ_METRIC]` log lines. Client and server timers are monotonic but local to their own processes, so compare elapsed durations only within the same log stream; do not compare their timestamps.

## Android baseline

The six Wi-Fi baseline rows in [rtsp-ptz-latency-results.xlsx](rtsp-ptz-latency-results.xlsx) average:

- RTSP first frame: approximately 1.03 seconds.
- Physical PTZ movement: approximately 0.84 seconds.
- PTZ movement visible in the stream: approximately 2.64 seconds.
- Additional display delay inferred from visible minus physical movement: approximately 1.81 seconds.

Direct digital-clock stream-lag measurements are not part of this comparison. Infer the additional display delay by subtracting **PTZ Actual movement** from **PTZ visible response on stream**.

## Android VLC comparison profiles

The active profile is selected by `SELECTED_VLC_RTSP_PROFILE_ID` in `src/ClientApp/features/cameras/vlc-rtsp-config.ts`. Use the exact profile ID as the workbook `Change` value:

| Profile | Transport | Network cache | Purpose |
| --- | --- | ---: | --- |
| `vlc-tcp-500` | TCP | 500 ms | Selected conservative profile with measured latency improvement. |
| `vlc-tcp-300` | TCP | 300 ms | Measured comparison that was slower than the 500 ms profile. |
| `vlc-udp-500` | UDP | 500 ms | Isolates transport behavior on the same cache size. |

Android uses custom LibVLC initialization (`initType: 2`). TCP profiles pass `--rtsp-tcp` to LibVLC, while omitting it selects VLC's default UDP transport. Each profile passes `:network-caching=<milliseconds>` to the media before `MediaPlayer.setMedia()`.

`react-native-vlc-media-player` 1.0.98 drops the final Android initialization and media option. The application-level adapter repeats the final valid option so the duplicate is discarded and every intended option reaches LibVLC. No dependency or generated native file is patched. LibVLC then applies its automatic, non-forced hardware decoding; because the media cache is already explicit, hardware initialization does not replace it with the Android 1500 ms default.

The profiles deliberately retain VLC defaults for clock synchronization, clock jitter, late-frame dropping, and frame skipping. VLC 3.0 already enables late-frame dropping and frame skipping. `rtsp-caching` is obsolete, and `live-caching` is not a VLC 3.0 core option. iOS retains its previous RTSP-over-TCP behavior and is outside this Android comparison.

## Manual procedure

1. Keep the same Android device, camera, channel, Wi-Fi network, and approximate test position for every profile.
2. Mark the run as `Cold` for the first open after idle. Mark repeat opens as `Warm`.
3. **RTSP first frame:** Start a stopwatch when you tap the camera card. Stop it when the first moving live frame appears. `first_usable_frame_proxy` is a helpful internal marker, but the moving frame is authoritative.
4. **PTZ Actual movement:** Start timing when you press the PTZ control. Stop when the camera physically starts moving.
5. **PTZ visible response on stream:** Start timing at the same PTZ press. Stop when the movement first appears in the stream.
6. Record all three measurements in the same `Cold` or `Warm` row. Leave **Stream lag** blank.
7. Compare the client `[PTZ_METRIC] api_response` duration with the manual visible-response result. A fast API response with slow visible movement indicates stream or player lag.
8. Record buffering, freezing, apparent dropped frames, errors, or reconnects in **Notes**. Use the existing metric lines to support the notes. Never record credential-bearing RTSP URLs.

## Results handoff

For each profile, append 6 complete rows to the workbook. Use milliseconds for every time field and keep `Change` identical across all rows:

- Three `Cold` rows: fill **RTSP first frame**, **PTZ Actual movement**, and **PTZ visible response on stream**
- Three `Warm` rows: fill the same three measurements
- **Notes**: record buffering, freezing, dropped-frame symptoms, reconnects, errors, and relevant `[RTSP_METRIC]` or `[PTZ_METRIC]` observations

Each row therefore represents one complete stream and PTZ measurement run. Leave **Stream lag** blank and do not add separate `PTZ` rows.

For the selected profile, enter these sheet values:

- **Change**: `vlc-tcp-500`
- **Network**: `Wi-Fi`
- **Notes**: `Android | TCP | cache=500 ms | hardware decoding=auto, not forced | channel=Sub | observation=`

Replace `channel=Sub` if you test the main channel. Complete `observation=` with the observed behavior, such as `stable`, `buffered once`, `froze`, `dropped frames`, or `reconnected`. For another profile, use its exact profile ID and update the transport or cache value in **Notes**.

Evaluate averages against these improvement goals without hiding regressions:

- Additional display delay toward 0.8 seconds or less.
- PTZ visible response toward 1.6 seconds or less.
- RTSP first frame no worse than approximately 1.2 seconds.
- No material increase in buffering, freezing, or reconnects.
