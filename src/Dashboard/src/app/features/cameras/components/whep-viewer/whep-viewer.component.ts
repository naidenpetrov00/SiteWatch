import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  output,
  signal,
  viewChild
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import {
  CameraStreamAccessResponse,
  CameraStreamProfile,
  DashboardSiteCamera,
  WhepAudioState,
  WhepRecordingState,
  WhepViewerState
} from '../../models/camera-stream.models';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';
import {
  LocalCameraRecording,
  LocalCameraRecordingError,
  RecordingLimit
} from './local-camera-recording';
import {
  WhepSession,
  WhepSessionAccess,
  WhepSessionError
} from './whep-session';

const CONNECTION_DEADLINE_MARGIN_MS = 15_000;

@Component({
  selector: 'app-whep-viewer',
  imports: [MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './whep-viewer.component.html',
  styleUrl: './whep-viewer.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WhepViewerComponent {
  private readonly dashboardCamerasService = inject(DashboardCamerasService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly videoElement = viewChild<ElementRef<HTMLVideoElement>>('video');
  private session: WhepSession | null = null;
  private remoteStream: MediaStream | null = null;
  private recording: LocalCameraRecording | null = null;
  private recordingStopPromise: Promise<void> | null = null;
  private viewerStopPromise: Promise<void> | null = null;
  private teardownChain: Promise<void> = Promise.resolve();
  private connectionGeneration = 0;
  private destroyed = false;

  readonly siteId = input.required<string>();
  readonly camera = input.required<DashboardSiteCamera>();
  readonly closed = output<void>();
  readonly recordingPendingChange = output<boolean>();

  readonly selectedProfile = linkedSignal<CameraStreamProfile>(() => {
    this.camera().id;
    return 'sub';
  });
  readonly liveProfile = signal<CameraStreamProfile | null>(null);
  readonly state = signal<WhepViewerState>({ kind: 'closed' });
  readonly audioState = signal<WhepAudioState>('unavailable');
  readonly soundEnabled = signal(false);
  readonly playbackBlocked = signal(false);
  readonly frameAvailable = signal(false);
  readonly recordingElapsedSeconds = signal(0);
  readonly recordingState = signal<WhepRecordingState>(
    LocalCameraRecording.isSupported()
      ? { kind: 'idle' }
      : {
          kind: 'unsupported',
          message: 'Local recording is not supported by this browser.'
        }
  );
  readonly mediaMessage = signal<string | null>(null);

  readonly isBusy = computed(() => {
    const kind = this.state().kind;
    return kind === 'preparing' || kind === 'connecting' || kind === 'retrying';
  });
  readonly recordingPending = computed(() => {
    const kind = this.recordingState().kind;
    return kind === 'recording' || kind === 'finalizing';
  });
  readonly canTakeSnapshot = computed(
    () => this.state().kind === 'live' && this.frameAvailable()
  );
  readonly canStartRecording = computed(
    () =>
      this.state().kind === 'live' &&
      this.frameAvailable() &&
      this.recordingState().kind === 'idle' &&
      this.remoteStream?.getVideoTracks().some((track) => track.readyState === 'live') === true
  );
  readonly qualitySwitchDisabled = computed(
    () => this.isBusy() || this.recordingPending()
  );
  readonly statusMessage = computed(() => {
    const state = this.state();
    const quality = this.profileLabel(this.selectedProfile());
    switch (state.kind) {
      case 'preparing':
        return `Preparing secure ${quality} live access…`;
      case 'connecting':
        return `Connecting to the ${quality} camera stream…`;
      case 'live':
        return this.playbackBlocked()
          ? 'Connected. Select Sound On or retry playback from the player controls.'
          : 'Live';
      case 'failed':
        return state.message;
      case 'retrying':
        return `Requesting fresh ${quality} live access…`;
      case 'closed':
        return 'Live view closed.';
    }
  });
  readonly audioMessage = computed(() => {
    switch (this.audioState()) {
      case 'available':
        return this.soundEnabled() ? 'Sound is on.' : 'Sound is off.';
      case 'interrupted':
        return 'Audio is temporarily unavailable.';
      case 'unavailable':
        return 'Audio is unavailable for this stream.';
    }
  });
  readonly recordingElapsedLabel = computed(() => {
    const elapsed = this.recordingElapsedSeconds();
    const minutes = Math.floor(elapsed / 60).toString().padStart(2, '0');
    const seconds = (elapsed % 60).toString().padStart(2, '0');
    return `${minutes}:${seconds}`;
  });

  constructor() {
    effect((onCleanup) => {
      const siteId = this.siteId();
      const camera = this.camera();
      const profile = this.selectedProfile();
      const video = this.videoElement()?.nativeElement;
      if (!video) {
        return;
      }

      const generation = ++this.connectionGeneration;
      void this.startConnection(siteId, camera, profile, video, false, generation);
      onCleanup(() => {
        if (this.connectionGeneration === generation) {
          this.connectionGeneration += 1;
        }
        const recordingCleanup = this.stopRecording();
        void this.closeCurrentSession(true, recordingCleanup);
      });
    });

    effect(() => {
      this.recordingPendingChange.emit(this.recordingPending());
    });

    effect((onCleanup) => {
      if (!this.recordingPending() || typeof window === 'undefined') {
        return;
      }

      const warnBeforeUnload = (event: BeforeUnloadEvent): void => {
        event.preventDefault();
        event.returnValue = true;
      };
      window.addEventListener('beforeunload', warnBeforeUnload);
      onCleanup(() => window.removeEventListener('beforeunload', warnBeforeUnload));
    });

    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
      this.connectionGeneration += 1;
      const recordingCleanup = this.stopRecording();
      void this.closeCurrentSession(true, recordingCleanup);
    });
  }

  retry(): void {
    const video = this.videoElement()?.nativeElement;
    if (!video || this.destroyed || this.recordingPending()) {
      return;
    }

    const generation = ++this.connectionGeneration;
    void this.startConnection(
      this.siteId(),
      this.camera(),
      this.selectedProfile(),
      video,
      true,
      generation
    );
  }

  switchQuality(profile: CameraStreamProfile): void {
    if (
      profile === this.selectedProfile() ||
      this.qualitySwitchDisabled() ||
      this.destroyed
    ) {
      return;
    }

    this.mediaMessage.set(`Switching to ${this.profileLabel(profile)}…`);
    this.selectedProfile.set(profile);
  }

  async toggleSound(): Promise<void> {
    const video = this.videoElement()?.nativeElement;
    if (!video || this.audioState() === 'unavailable') {
      return;
    }

    if (this.soundEnabled()) {
      video.muted = true;
      this.soundEnabled.set(false);
      return;
    }

    video.muted = false;
    try {
      await video.play();
      this.soundEnabled.set(true);
      this.playbackBlocked.set(false);
    } catch {
      video.muted = true;
      this.soundEnabled.set(false);
      this.mediaMessage.set('Sound could not be started. Try again.');
    }
  }

  playbackStarted(): void {
    this.playbackBlocked.set(false);
    this.updateFrameAvailability();
  }

  updateFrameAvailability(): void {
    const video = this.videoElement()?.nativeElement;
    this.frameAvailable.set(
      this.state().kind === 'live' &&
        !!video &&
        video.readyState >= HTMLMediaElement.HAVE_CURRENT_DATA &&
        video.videoWidth > 0 &&
        video.videoHeight > 0
    );
  }

  async takeSnapshot(): Promise<void> {
    const video = this.videoElement()?.nativeElement;
    if (!video || !this.canTakeSnapshot()) {
      return;
    }

    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    try {
      const context = canvas.getContext('2d');
      if (!context) {
        throw new Error('snapshot');
      }
      context.drawImage(video, 0, 0, canvas.width, canvas.height);
      const blob = await new Promise<Blob | null>((resolve) =>
        canvas.toBlob(resolve, 'image/png')
      );
      if (!blob || blob.size === 0) {
        throw new Error('snapshot');
      }

      await this.downloadBlob(
        blob,
        `camera-${this.camera().numberId}-snapshot-${this.timestamp()}.png`
      );
      this.mediaMessage.set('Snapshot downloaded.');
    } catch {
      this.mediaMessage.set('The snapshot could not be saved. Try again.');
    } finally {
      canvas.width = 0;
      canvas.height = 0;
    }
  }

  startRecording(): void {
    const stream = this.remoteStream;
    if (!stream || !this.canStartRecording()) {
      return;
    }

    const recording = new LocalCameraRecording(
      stream,
      this.camera().numberId,
      {
        onElapsed: (seconds) => {
          if (this.recording === recording) {
            this.recordingElapsedSeconds.set(seconds);
          }
        },
        onLimitReached: (limit) => {
          if (this.recording === recording) {
            void this.stopRecording(limit);
          }
        },
        onRecorderTerminated: () => {
          if (this.recording === recording) {
            void this.stopRecording();
          }
        }
      }
    );
    this.recording = recording;

    try {
      const result = recording.start();
      this.recordingElapsedSeconds.set(0);
      this.recordingState.set({ kind: 'recording' });
      this.mediaMessage.set(
        this.audioState() !== 'unavailable' && !result.includesAudio
          ? 'Recording started without audio because audio recording is unavailable.'
          : 'Recording started.'
      );
    } catch (error) {
      this.recording = null;
      this.recordingState.set({
        kind: 'unsupported',
        message: error instanceof LocalCameraRecordingError
          ? 'This browser cannot create a supported local recording.'
          : 'Local recording is not supported by this browser.'
      });
      this.mediaMessage.set('Recording could not be started.');
    }
  }

  stopRecordingFromControl(): void {
    void this.stopRecording();
  }

  stop(): Promise<void> {
    if (this.viewerStopPromise) {
      return this.viewerStopPromise;
    }

    this.viewerStopPromise = this.stopInternal().finally(() => {
      this.viewerStopPromise = null;
    });
    return this.viewerStopPromise;
  }

  profileLabel(profile: CameraStreamProfile): 'SD' | 'HD' {
    return profile === 'sub' ? 'SD' : 'HD';
  }

  isQualityButtonDisabled(profile: CameraStreamProfile): boolean {
    return profile === this.selectedProfile() || this.qualitySwitchDisabled();
  }

  private async stopInternal(): Promise<void> {
    if (this.state().kind === 'closed') {
      return;
    }

    const generation = ++this.connectionGeneration;
    await this.stopRecording();
    this.playbackBlocked.set(false);
    this.frameAvailable.set(false);
    this.soundEnabled.set(false);
    const video = this.videoElement()?.nativeElement;
    if (video) {
      video.muted = true;
    }
    await this.closeCurrentSession(false);
    this.state.set({ kind: 'closed' });
    if (!this.destroyed && generation === this.connectionGeneration) {
      this.closed.emit();
    }
  }

  private async stopRecording(limit: RecordingLimit | null = null): Promise<void> {
    if (this.recordingStopPromise) {
      return this.recordingStopPromise;
    }

    const recording = this.recording;
    if (!recording) {
      return;
    }

    this.recordingState.set({ kind: 'finalizing' });
    this.mediaMessage.set('Finalizing local recording…');
    this.recordingStopPromise = recording.stopAndDownload()
      .then((result) => {
        if (this.recording === recording) {
          this.recording = null;
        }
        this.recordingElapsedSeconds.set(0);
        this.recordingState.set({ kind: 'idle' });
        if (!result.downloaded || result.failed) {
          this.mediaMessage.set('The recording could not be finalized.');
          return;
        }

        this.mediaMessage.set(
          limit === 'duration'
            ? 'Recording downloaded at the 5-minute limit.'
            : limit === 'bytes'
              ? 'Recording downloaded at the 256 MiB limit.'
              : 'Recording downloaded.'
        );
      })
      .catch(() => {
        if (this.recording === recording) {
          this.recording = null;
        }
        this.recordingElapsedSeconds.set(0);
        this.recordingState.set({ kind: 'idle' });
        this.mediaMessage.set('The recording could not be finalized.');
      })
      .finally(() => {
        this.recordingStopPromise = null;
      });
    return this.recordingStopPromise;
  }

  private async startConnection(
    siteId: string,
    camera: DashboardSiteCamera,
    profile: CameraStreamProfile,
    video: HTMLVideoElement,
    retrying: boolean,
    generation: number
  ): Promise<void> {
    this.state.set({ kind: retrying ? 'retrying' : 'preparing' });
    this.playbackBlocked.set(false);
    this.frameAvailable.set(false);
    this.soundEnabled.set(false);
    this.audioState.set('unavailable');
    this.liveProfile.set(null);
    this.remoteStream = null;
    video.muted = true;
    await this.closeCurrentSession(false);
    if (!this.isCurrent(generation)) {
      return;
    }

    let deadlineRefreshAvailable = true;
    while (this.isCurrent(generation)) {
      let access: WhepSessionAccess;
      try {
        const response = await this.dashboardCamerasService.requestStreamAccess(
          siteId,
          camera.id,
          profile
        );
        if (!this.isCurrent(generation)) {
          return;
        }
        access = this.validateAccess(response);
      } catch (error) {
        if (
          error instanceof WhepSessionError &&
          error.code === 'deadline' &&
          deadlineRefreshAvailable
        ) {
          deadlineRefreshAvailable = false;
          continue;
        }

        this.fail(generation, this.errorMessage(error));
        return;
      }

      const session = new WhepSession(video, {
        onLive: () => {
          if (this.isCurrent(generation) && this.session === session) {
            this.liveProfile.set(profile);
            this.state.set({ kind: 'live' });
            this.mediaMessage.set(`${this.profileLabel(profile)} stream is live.`);
            this.updateFrameAvailability();
          }
        },
        onFailure: (error) => void this.handleSessionFailure(generation, session, error),
        onPlaybackBlocked: () => {
          if (this.isCurrent(generation) && this.session === session) {
            this.playbackBlocked.set(true);
          }
        },
        onMediaStreamChange: (stream) => {
          if (this.isCurrent(generation) && this.session === session) {
            this.handleMediaStreamChange(stream, video);
          }
        }
      });
      this.session = session;
      this.state.set({ kind: 'connecting' });

      try {
        await session.connect(access);
        return;
      } catch (error) {
        if (!this.isCurrent(generation) || this.session !== session) {
          await session.close(true);
          return;
        }

        await this.closeCurrentSession(false);
        if (
          error instanceof WhepSessionError &&
          error.code === 'deadline' &&
          deadlineRefreshAvailable
        ) {
          deadlineRefreshAvailable = false;
          this.state.set({ kind: retrying ? 'retrying' : 'preparing' });
          continue;
        }

        this.fail(generation, this.errorMessage(error));
        return;
      }
    }
  }

  private handleMediaStreamChange(
    stream: MediaStream,
    video: HTMLVideoElement
  ): void {
    this.remoteStream = stream;
    const audioTrack = stream
      .getAudioTracks()
      .find((track) => track.readyState === 'live');
    if (!audioTrack) {
      video.muted = true;
      this.soundEnabled.set(false);
      this.audioState.set('unavailable');
      return;
    }

    this.audioState.set(audioTrack.muted ? 'interrupted' : 'available');
  }

  private validateAccess(response: CameraStreamAccessResponse): WhepSessionAccess {
    const connectionDeadline = Date.parse(response.connectionDeadline);
    if (
      !Number.isFinite(connectionDeadline) ||
      connectionDeadline - Date.now() <= CONNECTION_DEADLINE_MARGIN_MS
    ) {
      throw new WhepSessionError('deadline');
    }

    if (!response.bearerTicket.trim()) {
      throw new WhepSessionError('signaling');
    }

    let whepUrl: URL;
    try {
      whepUrl = new URL(response.whepUrl);
    } catch {
      throw new WhepSessionError('signaling');
    }

    if (
      (whepUrl.protocol !== 'https:' && whepUrl.protocol !== 'http:') ||
      whepUrl.username.length > 0 ||
      whepUrl.password.length > 0 ||
      whepUrl.search.length > 0 ||
      whepUrl.hash.length > 0 ||
      !whepUrl.pathname.endsWith('/whep') ||
      (typeof window !== 'undefined' &&
        window.location.protocol === 'https:' &&
        whepUrl.protocol !== 'https:')
    ) {
      throw new WhepSessionError('signaling');
    }

    return {
      whepUrl,
      bearerTicket: response.bearerTicket,
      connectionDeadline
    };
  }

  private async handleSessionFailure(
    generation: number,
    session: WhepSession,
    error: WhepSessionError
  ): Promise<void> {
    if (!this.isCurrent(generation) || this.session !== session) {
      return;
    }

    await this.stopRecording();
    await this.closeCurrentSession(false);
    this.fail(generation, this.errorMessage(error));
  }

  private closeCurrentSession(
    keepalive: boolean,
    prerequisite: Promise<void> = Promise.resolve()
  ): Promise<void> {
    const session = this.session;
    this.session = null;
    this.remoteStream = null;
    this.audioState.set('unavailable');
    this.soundEnabled.set(false);
    this.teardownChain = this.teardownChain
      .then(() => prerequisite)
      .then(() => session?.close(keepalive));
    return this.teardownChain;
  }

  private fail(generation: number, message: string): void {
    if (!this.isCurrent(generation)) {
      return;
    }

    this.frameAvailable.set(false);
    this.state.set({ kind: 'failed', message });
  }

  private errorMessage(error: unknown): string {
    if (!(error instanceof WhepSessionError)) {
      return 'Live view could not be prepared. Check your connection and try again.';
    }

    switch (error.code) {
      case 'unsupported':
        return 'Live view is not supported by this browser.';
      case 'deadline':
        return 'Live access expired before the connection could start. Try again.';
      case 'options':
        return 'The streaming service could not prepare this connection. Try again.';
      case 'signaling':
      case 'connection':
        return 'The camera live view could not connect. Try again.';
    }
  }

  private async downloadBlob(blob: Blob, filename: string): Promise<void> {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    try {
      anchor.href = url;
      anchor.download = filename;
      anchor.hidden = true;
      document.body.appendChild(anchor);
      anchor.click();
      await new Promise<void>((resolve) => setTimeout(resolve));
    } finally {
      anchor.remove();
      URL.revokeObjectURL(url);
    }
  }

  private timestamp(): string {
    return new Date().toISOString().replace(/[-:]/g, '').replace(/\.\d{3}Z$/, 'Z');
  }

  private isCurrent(generation: number): boolean {
    return !this.destroyed && generation === this.connectionGeneration;
  }
}
