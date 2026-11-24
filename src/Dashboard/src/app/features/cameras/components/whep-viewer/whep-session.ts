const CONNECTION_DEADLINE_MARGIN_MS = 15_000;
const ICE_GATHERING_TIMEOUT_MS = 10_000;
const CONNECTION_TIMEOUT_MS = 30_000;
const DISCONNECTED_GRACE_MS = 5_000;
const DELETE_TIMEOUT_MS = 3_000;

export type WhepSessionErrorCode =
  | 'unsupported'
  | 'deadline'
  | 'options'
  | 'signaling'
  | 'connection';

export class WhepSessionError extends Error {
  constructor(readonly code: WhepSessionErrorCode) {
    super(code);
    this.name = 'WhepSessionError';
  }
}

export interface WhepSessionAccess {
  whepUrl: URL;
  bearerTicket: string;
  connectionDeadline: number;
}

export interface WhepSessionCallbacks {
  onLive: () => void;
  onFailure: (error: WhepSessionError) => void;
  onPlaybackBlocked: () => void;
}

export class WhepSession {
  private readonly abortController = new AbortController();
  private mediaStream: MediaStream | null = null;
  private peerConnection: RTCPeerConnection | null = null;
  private sessionUrl: URL | null = null;
  private connectionTimer: ReturnType<typeof setTimeout> | null = null;
  private disconnectedTimer: ReturnType<typeof setTimeout> | null = null;
  private closePromise: Promise<void> | null = null;
  private closing = false;
  private failureReported = false;
  private liveReported = false;

  constructor(
    private readonly video: HTMLVideoElement,
    private readonly callbacks: WhepSessionCallbacks
  ) {}

  async connect(access: WhepSessionAccess): Promise<void> {
    this.assertOpen();
    if (
      typeof RTCPeerConnection === 'undefined' ||
      typeof MediaStream === 'undefined'
    ) {
      throw new WhepSessionError('unsupported');
    }

    this.assertDeadline(access.connectionDeadline);
    this.startConnectionTimer(access.connectionDeadline);
    const authorization = `Bearer ${access.bearerTicket}`;
    const optionsResponse = await this.fetchSignaling(access.whepUrl, {
      method: 'OPTIONS',
      headers: { Authorization: authorization }
    });

    if (!optionsResponse.ok) {
      throw new WhepSessionError(
        optionsResponse.status === 401 ? 'deadline' : 'options'
      );
    }

    this.assertOpen();
    const iceServers = this.parseIceServers(optionsResponse.headers.get('Link'));
    this.assertDeadline(access.connectionDeadline);
    const peerConnection = new RTCPeerConnection({ iceServers });
    this.mediaStream = new MediaStream();
    this.peerConnection = peerConnection;
    peerConnection.addTransceiver('video', { direction: 'recvonly' });
    peerConnection.addTransceiver('audio', { direction: 'recvonly' });
    peerConnection.ontrack = (event) => this.handleTrack(event);
    peerConnection.onconnectionstatechange = () => this.handleConnectionState();

    const offer = await peerConnection.createOffer();
    this.assertOpen();
    await peerConnection.setLocalDescription(offer);
    this.assertOpen();
    await this.waitForIceGathering(peerConnection, access.connectionDeadline);
    this.assertDeadline(access.connectionDeadline);

    const localSdp = peerConnection.localDescription?.sdp;
    if (!localSdp) {
      throw new WhepSessionError('signaling');
    }

    const response = await this.fetchSignaling(access.whepUrl, {
      method: 'POST',
      headers: {
        Authorization: authorization,
        'Content-Type': 'application/sdp'
      },
      body: localSdp
    });

    if (response.status !== 201) {
      throw new WhepSessionError(
        response.status === 401 ? 'deadline' : 'signaling'
      );
    }

    const location = response.headers.get('Location');
    if (!location) {
      throw new WhepSessionError('signaling');
    }

    const sessionUrl = this.resolveSessionUrl(access.whepUrl, location);
    this.sessionUrl = sessionUrl;

    if (this.closing) {
      this.sessionUrl = null;
      await this.deleteSession(sessionUrl, true);
      throw new WhepSessionError('connection');
    }

    const contentType = response.headers.get('Content-Type')?.split(';', 1)[0]?.trim();
    if (contentType?.toLowerCase() !== 'application/sdp') {
      throw new WhepSessionError('signaling');
    }

    const answer = await response.text();
    if (!answer.trim()) {
      throw new WhepSessionError('signaling');
    }

    await peerConnection.setRemoteDescription({ type: 'answer', sdp: answer });
    this.assertOpen();
  }

  close(keepalive = false): Promise<void> {
    if (this.closePromise) {
      return this.closePromise;
    }

    this.closePromise = this.closeInternal(keepalive).catch(() => {
      // Cleanup is idempotent and must not surface sensitive browser errors.
    });
    return this.closePromise;
  }

  private async closeInternal(keepalive: boolean): Promise<void> {
    this.closing = true;
    this.abortController.abort();
    this.clearTimers();

    const peerConnection = this.peerConnection;
    this.peerConnection = null;
    if (peerConnection) {
      peerConnection.ontrack = null;
      peerConnection.onconnectionstatechange = null;
      peerConnection.close();
    }

    for (const track of this.mediaStream?.getTracks() ?? []) {
      track.onended = null;
      track.stop();
    }
    this.mediaStream = null;

    this.video.pause();
    this.video.srcObject = null;

    const sessionUrl = this.sessionUrl;
    this.sessionUrl = null;
    if (sessionUrl) {
      await this.deleteSession(sessionUrl, keepalive);
    }
  }

  private async fetchSignaling(
    url: URL,
    request: RequestInit
  ): Promise<Response> {
    try {
      return await fetch(url, {
        ...request,
        cache: 'no-store',
        credentials: 'omit',
        mode: 'cors',
        referrerPolicy: 'no-referrer',
        signal: this.abortController.signal
      });
    } catch {
      throw new WhepSessionError('signaling');
    }
  }

  private parseIceServers(linkHeader: string | null): RTCIceServer[] {
    if (!linkHeader) {
      return [];
    }

    const servers: RTCIceServer[] = [];
    for (const link of linkHeader.split(/,\s*(?=<)/)) {
      if (!/rel="ice-server"/i.test(link)) {
        continue;
      }

      const match = link.match(
        /^<([^>]+)>;\s*rel="ice-server"(?:;\s*username="((?:\\.|[^"])*)";\s*credential="((?:\\.|[^"])*)";\s*credential-type="password")?$/i
      );
      if (!match) {
        throw new WhepSessionError('options');
      }

      const server: RTCIceServer = { urls: [match[1]!] };
      if (match[2] !== undefined && match[3] !== undefined) {
        try {
          server.username = JSON.parse(`"${match[2]}"`) as string;
          server.credential = JSON.parse(`"${match[3]}"`) as string;
          server.credentialType = 'password';
        } catch {
          throw new WhepSessionError('options');
        }
      }

      servers.push(server);
    }

    return servers;
  }

  private waitForIceGathering(
    peerConnection: RTCPeerConnection,
    connectionDeadline: number
  ): Promise<void> {
    if (peerConnection.iceGatheringState === 'complete') {
      return Promise.resolve();
    }

    const availableTime =
      connectionDeadline - Date.now() - CONNECTION_DEADLINE_MARGIN_MS;
    if (availableTime <= 0) {
      return Promise.reject(new WhepSessionError('deadline'));
    }
    if (this.abortController.signal.aborted) {
      return Promise.reject(new WhepSessionError('connection'));
    }

    return new Promise<void>((resolve, reject) => {
      const cleanup = (): void => {
        clearTimeout(timeout);
        peerConnection.removeEventListener('icegatheringstatechange', onStateChange);
        this.abortController.signal.removeEventListener('abort', onAbort);
      };
      const onStateChange = (): void => {
        if (peerConnection.iceGatheringState === 'complete') {
          cleanup();
          resolve();
        }
      };
      const onAbort = (): void => {
        cleanup();
        reject(new WhepSessionError('connection'));
      };
      const timeout = setTimeout(() => {
        cleanup();
        reject(new WhepSessionError('signaling'));
      }, Math.min(ICE_GATHERING_TIMEOUT_MS, availableTime));

      peerConnection.addEventListener('icegatheringstatechange', onStateChange);
      this.abortController.signal.addEventListener('abort', onAbort, { once: true });
    });
  }

  private resolveSessionUrl(whepUrl: URL, location: string): URL {
    let sessionUrl: URL;
    try {
      sessionUrl = new URL(location, whepUrl);
    } catch {
      throw new WhepSessionError('signaling');
    }

    const sessionPrefix = `${whepUrl.pathname}/`;
    const childSegment = sessionUrl.pathname.startsWith(sessionPrefix)
      ? sessionUrl.pathname.slice(sessionPrefix.length)
      : '';
    let decodedChild: string;
    try {
      decodedChild = decodeURIComponent(childSegment);
    } catch {
      throw new WhepSessionError('signaling');
    }

    if (
      sessionUrl.origin !== whepUrl.origin ||
      sessionUrl.username.length > 0 ||
      sessionUrl.password.length > 0 ||
      sessionUrl.search.length > 0 ||
      sessionUrl.hash.length > 0 ||
      childSegment.length === 0 ||
      childSegment.includes('/') ||
      decodedChild.includes('/')
    ) {
      throw new WhepSessionError('signaling');
    }

    return sessionUrl;
  }

  private handleTrack(event: RTCTrackEvent): void {
    const mediaStream = this.mediaStream;
    if (this.closing || !mediaStream) {
      return;
    }

    mediaStream.addTrack(event.track);
    event.track.onended = () => {
      if (event.track.kind === 'video') {
        this.reportFailure('connection');
      }
    };
    this.video.srcObject = mediaStream;
    this.video.muted = true;
    void this.video.play().catch(() => this.callbacks.onPlaybackBlocked());
    this.tryReportLive();
  }

  private handleConnectionState(): void {
    const connectionState = this.peerConnection?.connectionState;
    if (!connectionState || this.closing) {
      return;
    }

    if (connectionState === 'connected') {
      this.clearDisconnectedTimer();
      this.tryReportLive();
      return;
    }

    if (connectionState === 'disconnected') {
      this.clearDisconnectedTimer();
      this.disconnectedTimer = setTimeout(() => {
        if (this.peerConnection?.connectionState === 'disconnected') {
          this.reportFailure('connection');
        }
      }, DISCONNECTED_GRACE_MS);
      return;
    }

    if (connectionState === 'failed' || connectionState === 'closed') {
      this.reportFailure('connection');
    }
  }

  private tryReportLive(): void {
    if (
      this.liveReported ||
      this.peerConnection?.connectionState !== 'connected' ||
      !this.mediaStream ||
      this.mediaStream.getVideoTracks().length === 0
    ) {
      return;
    }

    this.liveReported = true;
    this.clearConnectionTimer();
    this.callbacks.onLive();
  }

  private reportFailure(code: WhepSessionErrorCode): void {
    if (this.closing || this.failureReported) {
      return;
    }

    this.failureReported = true;
    this.clearTimers();
    this.callbacks.onFailure(new WhepSessionError(code));
  }

  private assertDeadline(connectionDeadline: number): void {
    if (
      !Number.isFinite(connectionDeadline) ||
      connectionDeadline - Date.now() <= CONNECTION_DEADLINE_MARGIN_MS
    ) {
      throw new WhepSessionError('deadline');
    }
  }

  private startConnectionTimer(connectionDeadline: number): void {
    const availableTime =
      connectionDeadline - Date.now() - CONNECTION_DEADLINE_MARGIN_MS;
    if (availableTime <= 0) {
      throw new WhepSessionError('deadline');
    }

    this.connectionTimer = setTimeout(() => {
      if (this.closing || this.liveReported) {
        return;
      }

      this.abortController.abort();
      this.reportFailure('connection');
      void this.close();
    }, Math.min(CONNECTION_TIMEOUT_MS, availableTime));
  }

  private assertOpen(): void {
    if (this.closing) {
      throw new WhepSessionError('connection');
    }
  }

  private async deleteSession(sessionUrl: URL, keepalive: boolean): Promise<void> {
    const deleteController = new AbortController();
    const timeout = setTimeout(() => deleteController.abort(), DELETE_TIMEOUT_MS);
    try {
      await fetch(sessionUrl, {
        method: 'DELETE',
        cache: 'no-store',
        credentials: 'omit',
        keepalive,
        mode: 'cors',
        referrerPolicy: 'no-referrer',
        signal: deleteController.signal
      });
    } catch {
      // The browser resources are already closed; session deletion is best effort.
    } finally {
      clearTimeout(timeout);
    }
  }

  private clearTimers(): void {
    this.clearConnectionTimer();
    this.clearDisconnectedTimer();
  }

  private clearConnectionTimer(): void {
    if (this.connectionTimer === null) {
      return;
    }

    clearTimeout(this.connectionTimer);
    this.connectionTimer = null;
  }

  private clearDisconnectedTimer(): void {
    if (this.disconnectedTimer === null) {
      return;
    }

    clearTimeout(this.disconnectedTimer);
    this.disconnectedTimer = null;
  }
}
