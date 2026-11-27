const RECORDING_TIMESLICE_MS = 1_000;
const MAX_RECORDING_DURATION_MS = 300_000;
const MAX_RECORDING_BYTES = 268_435_456;
const FINALIZATION_TIMEOUT_MS = 10_000;

const VIDEO_WITH_AUDIO_MIME_TYPES = [
  'video/mp4;codecs=avc1.42E01E,mp4a.40.2',
  'video/mp4;codecs=avc1.640028,mp4a.40.2',
  'video/mp4',
  'video/webm;codecs=vp8,opus',
  'video/webm;codecs=vp9,opus',
  'video/webm'
] as const;

const VIDEO_ONLY_MIME_TYPES = [
  'video/mp4;codecs=avc1.42E01E',
  'video/mp4;codecs=avc1.640028',
  'video/mp4',
  'video/webm;codecs=vp8',
  'video/webm;codecs=vp9',
  'video/webm'
] as const;

export type RecordingLimit = 'duration' | 'bytes';

export interface LocalCameraRecordingCallbacks {
  onElapsed: (seconds: number) => void;
  onLimitReached: (limit: RecordingLimit) => void;
  onRecorderTerminated: () => void;
}

export interface LocalCameraRecordingStartResult {
  includesAudio: boolean;
}

export interface LocalCameraRecordingStopResult {
  downloaded: boolean;
  failed: boolean;
}

export class LocalCameraRecordingError extends Error {
  constructor() {
    super('recording');
    this.name = 'LocalCameraRecordingError';
  }
}

export class LocalCameraRecording {
  private readonly clonedTracks: MediaStreamTrack[] = [];
  private readonly chunks: Blob[] = [];
  private recorder: MediaRecorder | null = null;
  private mimeType = '';
  private accumulatedBytes = 0;
  private dataEventCount = 0;
  private finalDataAvailable = false;
  private stopSeen = false;
  private recorderFailed = false;
  private outputIncomplete = false;
  private limitRequested = false;
  private terminationReported = false;
  private elapsedTimer: ReturnType<typeof setInterval> | null = null;
  private durationTimer: ReturnType<typeof setTimeout> | null = null;
  private stopPromise: Promise<LocalCameraRecordingStopResult> | null = null;
  private resolveStopped: (() => void) | null = null;
  private readonly stopped = new Promise<void>((resolve) => {
    this.resolveStopped = resolve;
  });

  constructor(
    private readonly sourceStream: MediaStream,
    private readonly cameraNumberId: number,
    private readonly callbacks: LocalCameraRecordingCallbacks
  ) {}

  static isSupported(): boolean {
    if (typeof MediaRecorder === 'undefined') {
      return false;
    }

    return [...VIDEO_WITH_AUDIO_MIME_TYPES, ...VIDEO_ONLY_MIME_TYPES].some(
      (mimeType) => this.isMimeTypeSupported(mimeType)
    );
  }

  start(): LocalCameraRecordingStartResult {
    const sourceVideoTrack = this.sourceStream
      .getVideoTracks()
      .find((track) => track.readyState === 'live');
    if (!sourceVideoTrack || typeof MediaRecorder === 'undefined') {
      throw new LocalCameraRecordingError();
    }

    let videoClone: MediaStreamTrack;
    try {
      videoClone = sourceVideoTrack.clone();
      this.clonedTracks.push(videoClone);
    } catch {
      throw new LocalCameraRecordingError();
    }

    const sourceAudioTrack = this.sourceStream
      .getAudioTracks()
      .find((track) => track.readyState === 'live');
    let audioClone: MediaStreamTrack | null = null;
    if (sourceAudioTrack) {
      try {
        audioClone = sourceAudioTrack.clone();
        this.clonedTracks.push(audioClone);
      } catch {
        audioClone = null;
      }
    }

    let recorder = audioClone
      ? this.createRecorder(
          new MediaStream([videoClone, audioClone]),
          VIDEO_WITH_AUDIO_MIME_TYPES
        )
      : null;
    if (!recorder && audioClone) {
      audioClone.stop();
      this.clonedTracks.splice(this.clonedTracks.indexOf(audioClone), 1);
      audioClone = null;
    }
    recorder ??= this.createRecorder(
      new MediaStream([videoClone]),
      VIDEO_ONLY_MIME_TYPES
    );
    if (!recorder) {
      this.stopClonedTracks();
      throw new LocalCameraRecordingError();
    }

    this.recorder = recorder;
    this.mimeType = recorder.mimeType;
    recorder.ondataavailable = (event) => this.handleDataAvailable(event);
    recorder.onerror = () => {
      this.recorderFailed = true;
      this.reportRecorderTermination();
    };
    recorder.onstop = () => {
      this.stopSeen = true;
      this.finalDataAvailable = this.dataEventCount > 0;
      this.resolveStopped?.();
      this.resolveStopped = null;
      if (!this.stopPromise) {
        this.reportRecorderTermination();
      }
    };

    try {
      recorder.start(RECORDING_TIMESLICE_MS);
    } catch {
      this.cleanupRecorder();
      throw new LocalCameraRecordingError();
    }

    const startedAt = performance.now();
    let lastElapsed = -1;
    this.elapsedTimer = setInterval(() => {
      const elapsed = Math.min(
        Math.floor((performance.now() - startedAt) / 1_000),
        MAX_RECORDING_DURATION_MS / 1_000
      );
      if (elapsed !== lastElapsed) {
        lastElapsed = elapsed;
        this.callbacks.onElapsed(elapsed);
      }
    }, 250);
    this.callbacks.onElapsed(0);
    this.durationTimer = setTimeout(
      () => this.requestLimitStop('duration'),
      MAX_RECORDING_DURATION_MS
    );

    return { includesAudio: audioClone !== null };
  }

  stopAndDownload(): Promise<LocalCameraRecordingStopResult> {
    this.stopPromise ??= this.stopAndDownloadInternal();
    return this.stopPromise;
  }

  private async stopAndDownloadInternal(): Promise<LocalCameraRecordingStopResult> {
    let downloaded = false;
    try {
      this.clearTimers();
      const recorder = this.recorder;
      if (!recorder) {
        return { downloaded: false, failed: true };
      }

      if (recorder.state !== 'inactive') {
        try {
          recorder.stop();
        } catch {
          this.recorderFailed = true;
        }
      }

      const completed = await this.waitForFinalization();
      if (
        completed &&
        this.stopSeen &&
        this.finalDataAvailable &&
        !this.recorderFailed &&
        !this.outputIncomplete &&
        this.accumulatedBytes > 0
      ) {
        downloaded = await this.downloadRecording();
      }

      return { downloaded, failed: !downloaded };
    } finally {
      this.cleanupRecorder();
    }
  }

  private handleDataAvailable(event: BlobEvent): void {
    this.dataEventCount += 1;
    if (event.data.size === 0) {
      return;
    }

    const nextSize = this.accumulatedBytes + event.data.size;
    if (nextSize > MAX_RECORDING_BYTES) {
      this.outputIncomplete = true;
      this.requestLimitStop('bytes');
      return;
    }

    this.chunks.push(event.data);
    this.accumulatedBytes = nextSize;
    if (this.accumulatedBytes === MAX_RECORDING_BYTES) {
      this.requestLimitStop('bytes');
    }
  }

  private requestLimitStop(limit: RecordingLimit): void {
    if (this.limitRequested) {
      return;
    }

    this.limitRequested = true;
    this.callbacks.onLimitReached(limit);
  }

  private reportRecorderTermination(): void {
    if (this.terminationReported) {
      return;
    }

    this.terminationReported = true;
    this.callbacks.onRecorderTerminated();
  }

  private async waitForFinalization(): Promise<boolean> {
    if (this.stopSeen) {
      return true;
    }

    return new Promise<boolean>((resolve) => {
      let settled = false;
      const finish = (completed: boolean): void => {
        if (settled) {
          return;
        }
        settled = true;
        clearTimeout(timeout);
        resolve(completed);
      };
      const timeout = setTimeout(
        () => finish(false),
        FINALIZATION_TIMEOUT_MS
      );
      void this.stopped.then(() => finish(true));
    });
  }

  private async downloadRecording(): Promise<boolean> {
    const baseMimeType = this.mimeType.split(';', 1)[0]?.trim().toLowerCase();
    const extension = baseMimeType === 'video/webm'
      ? 'webm'
      : baseMimeType === 'video/mp4'
        ? 'mp4'
        : null;
    if (!extension) {
      return false;
    }

    let url: string | null = null;
    let anchor: HTMLAnchorElement | null = null;
    try {
      const blob = new Blob(this.chunks, { type: this.mimeType });
      if (blob.size === 0) {
        return false;
      }

      url = URL.createObjectURL(blob);
      anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = `camera-${this.cameraNumberId}-recording-${this.timestamp()}.${extension}`;
      anchor.hidden = true;
      document.body.appendChild(anchor);
      anchor.click();
      await new Promise<void>((resolve) => setTimeout(resolve));
      return true;
    } catch {
      return false;
    } finally {
      anchor?.remove();
      if (url !== null) {
        URL.revokeObjectURL(url);
      }
    }
  }

  private createRecorder(
    stream: MediaStream,
    candidates: readonly string[]
  ): MediaRecorder | null {
    for (const mimeType of candidates) {
      if (!LocalCameraRecording.isMimeTypeSupported(mimeType)) {
        continue;
      }

      try {
        const recorder = new MediaRecorder(stream, { mimeType });
        const baseMimeType = recorder.mimeType
          .split(';', 1)[0]
          ?.trim()
          .toLowerCase();
        if (baseMimeType === 'video/webm' || baseMimeType === 'video/mp4') {
          return recorder;
        }
      } catch {
        // Try the next browser-supported container/codec combination.
      }
    }

    return null;
  }

  private cleanupRecorder(): void {
    this.clearTimers();
    if (this.recorder) {
      this.recorder.ondataavailable = null;
      this.recorder.onerror = null;
      this.recorder.onstop = null;
    }
    this.recorder = null;
    this.chunks.length = 0;
    this.accumulatedBytes = 0;
    this.stopClonedTracks();
  }

  private stopClonedTracks(): void {
    for (const track of this.clonedTracks.splice(0)) {
      track.stop();
    }
  }

  private clearTimers(): void {
    if (this.elapsedTimer !== null) {
      clearInterval(this.elapsedTimer);
      this.elapsedTimer = null;
    }
    if (this.durationTimer !== null) {
      clearTimeout(this.durationTimer);
      this.durationTimer = null;
    }
  }

  private timestamp(): string {
    return new Date().toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z$/, 'Z');
  }

  private static isMimeTypeSupported(mimeType: string): boolean {
    try {
      return MediaRecorder.isTypeSupported(mimeType);
    } catch {
      return false;
    }
  }
}
