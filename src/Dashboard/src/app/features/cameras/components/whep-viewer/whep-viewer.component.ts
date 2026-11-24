import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import {
  CameraStreamAccessResponse,
  DashboardSiteCamera,
  WhepViewerState
} from '../../models/camera-stream.models';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';
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
  private teardownChain: Promise<void> = Promise.resolve();
  private connectionGeneration = 0;
  private destroyed = false;

  readonly siteId = input.required<string>();
  readonly camera = input.required<DashboardSiteCamera>();
  readonly closed = output<void>();

  readonly state = signal<WhepViewerState>({ kind: 'closed' });
  readonly playbackBlocked = signal(false);
  readonly isBusy = computed(() => {
    const kind = this.state().kind;
    return kind === 'preparing' || kind === 'connecting' || kind === 'retrying';
  });
  readonly statusMessage = computed(() => {
    const state = this.state();
    switch (state.kind) {
      case 'preparing':
        return 'Preparing secure live access…';
      case 'connecting':
        return 'Connecting to the camera…';
      case 'live':
        return this.playbackBlocked()
          ? 'Connected. Use the video controls to start playback.'
          : 'Live';
      case 'failed':
        return state.message;
      case 'retrying':
        return 'Requesting fresh live access…';
      case 'closed':
        return 'Live view closed.';
    }
  });

  constructor() {
    effect((onCleanup) => {
      const siteId = this.siteId();
      const camera = this.camera();
      const video = this.videoElement()?.nativeElement;
      if (!video) {
        return;
      }

      const generation = ++this.connectionGeneration;
      void this.startConnection(siteId, camera, video, false, generation);
      onCleanup(() => {
        if (this.connectionGeneration === generation) {
          this.connectionGeneration += 1;
        }
        void this.closeCurrentSession(true);
      });
    });

    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
      this.connectionGeneration += 1;
      void this.closeCurrentSession(true);
    });
  }

  retry(): void {
    const video = this.videoElement()?.nativeElement;
    if (!video || this.destroyed) {
      return;
    }

    const generation = ++this.connectionGeneration;
    void this.startConnection(
      this.siteId(),
      this.camera(),
      video,
      true,
      generation
    );
  }

  playbackStarted(): void {
    this.playbackBlocked.set(false);
  }

  async stop(): Promise<void> {
    const generation = ++this.connectionGeneration;
    this.state.set({ kind: 'closed' });
    this.playbackBlocked.set(false);
    await this.closeCurrentSession(false);
    if (!this.destroyed && generation === this.connectionGeneration) {
      this.closed.emit();
    }
  }

  private async startConnection(
    siteId: string,
    camera: DashboardSiteCamera,
    video: HTMLVideoElement,
    retrying: boolean,
    generation: number
  ): Promise<void> {
    this.state.set({ kind: retrying ? 'retrying' : 'preparing' });
    this.playbackBlocked.set(false);
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
          camera.id
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
            this.state.set({ kind: 'live' });
          }
        },
        onFailure: (error) => void this.handleSessionFailure(generation, session, error),
        onPlaybackBlocked: () => {
          if (this.isCurrent(generation) && this.session === session) {
            this.playbackBlocked.set(true);
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

    await this.closeCurrentSession(false);
    this.fail(generation, this.errorMessage(error));
  }

  private closeCurrentSession(keepalive: boolean): Promise<void> {
    const session = this.session;
    this.session = null;
    if (session) {
      this.teardownChain = this.teardownChain.then(() => session.close(keepalive));
    }
    return this.teardownChain;
  }

  private fail(generation: number, message: string): void {
    if (!this.isCurrent(generation)) {
      return;
    }

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

  private isCurrent(generation: number): boolean {
    return !this.destroyed && generation === this.connectionGeneration;
  }
}
