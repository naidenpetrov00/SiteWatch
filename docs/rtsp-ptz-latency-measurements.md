# RTSP and PTZ latency measurements

The development app emits concise `[RTSP_METRIC]` and `[PTZ_METRIC]` log lines. Client and server timers are monotonic but local to their own processes, so compare elapsed durations only within the same log stream; do not compare their timestamps.

## Manual procedure

1. **RTSP startup:** Start a stopwatch when the camera card is tapped and stop it when the first moving live frame appears. Record the elapsed value as **RTSP first frame**. `first_usable_frame_proxy` is a helpful internal marker, but the moving frame is authoritative.
2. **Stream lag:** Place a second device displaying a digital stopwatch in the camera's view. Compare its real value with the value visible inside the stream and record the difference as **Stream lag**.
3. **PTZ visible response:** Start timing when the PTZ button is pressed and stop when movement first becomes visible in the stream.
4. Compare the client `[PTZ_METRIC] api_response` duration with the manual press-to-visible-motion result. A fast API response with slow visible movement indicates stream/player lag.
5. After every optimization, perform five cold opens, five warm opens, and five PTZ movements. Keep the same camera, channel, network, and approximate test position for every comparison.

## Results handoff

Use [rtsp-ptz-latency-results.xlsx](rtsp-ptz-latency-results.xlsx) as the shared results workbook. Its `Results` sheet is ready for entry, while its `Metric guide` sheet defines each column. Add one row per measurement, use milliseconds for every time field, and keep the `Change` value identical for all 15 runs of the same optimization.

For each optimization, append five `Cold` rows, five `Warm` rows, and five `PTZ` rows. Include the client `[PTZ_METRIC] api_response` elapsed value in `PTZ API response`; use the manual stopwatch value for `PTZ visible response`.
