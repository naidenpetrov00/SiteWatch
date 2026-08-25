export type PtzMetricContext = {
  interactionId: string;
  interactionStartedAtMs: number;
  cameraId: string;
  direction: string;
};

type RtspMetricSession = {
  startedAtMs: number;
  bufferingLogged: boolean;
  errorLogged: boolean;
  firstUsableFrameLogged: boolean;
  hasPlayed: boolean;
  loadedLogged: boolean;
  playingLogged: boolean;
  reconnectionLogged: boolean;
};

const elapsedMs = (startedAtMs: number) => Math.round(performance.now() - startedAtMs);

export const createRtspMetricSession = (): RtspMetricSession => ({
  startedAtMs: performance.now(),
  bufferingLogged: false,
  errorLogged: false,
  firstUsableFrameLogged: false,
  hasPlayed: false,
  loadedLogged: false,
  playingLogged: false,
  reconnectionLogged: false,
});

export const logRtspMetric = (session: RtspMetricSession, event: string) => {
  if (!__DEV__) return;

  console.log(`[RTSP_METRIC] ${event}`, {
    elapsedMs: elapsedMs(session.startedAtMs),
  });
};

export const createPtzMetricContext = (
  cameraId: string,
  direction: string,
): PtzMetricContext => ({
  interactionId: `${Math.round(performance.now())}-${Math.random().toString(36).slice(2, 7)}`,
  interactionStartedAtMs: performance.now(),
  cameraId,
  direction,
});

export const logPtzMetric = (
  context: PtzMetricContext | undefined,
  event: string,
  operation?: string,
) => {
  if (!__DEV__ || !context) return;

  console.log(`[PTZ_METRIC] ${event}`, {
    interactionId: context.interactionId,
    cameraId: context.cameraId,
    direction: context.direction,
    operation,
    elapsedMs: elapsedMs(context.interactionStartedAtMs),
  });
};

export const shouldLogFirstBuffering = (session: RtspMetricSession) => {
  if (session.bufferingLogged) return false;

  session.bufferingLogged = true;
  return true;
};

export const shouldLogFirstError = (session: RtspMetricSession) => {
  if (session.errorLogged) return false;

  session.errorLogged = true;
  return true;
};

export const shouldLogFirstLoad = (session: RtspMetricSession) => {
  if (session.loadedLogged) return false;

  session.loadedLogged = true;
  return true;
};

export const recordPlaying = (session: RtspMetricSession) => {
  const isFirstUsableFrame = !session.firstUsableFrameLogged;
  const isReconnection = session.hasPlayed && session.errorLogged && !session.reconnectionLogged;
  const shouldLogPlaying = !session.playingLogged || isReconnection;

  session.hasPlayed = true;
  session.firstUsableFrameLogged = true;
  session.playingLogged = true;
  session.reconnectionLogged ||= isReconnection;

  return { isFirstUsableFrame, isReconnection, shouldLogPlaying };
};
