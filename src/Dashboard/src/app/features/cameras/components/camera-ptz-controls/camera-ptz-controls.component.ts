import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

import {
  PtzDirection,
  PtzRelativeMovement
} from '../../models/camera-stream.models';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';
import { PtzControlState, PtzSafetyState } from './camera-ptz.types';

const HOLD_DELAY_MS = 250;
const RELATIVE_STEP = 0.1;
const SAFETY_STOP_MS = 5_000;
const STOP_RETRY_DELAY_MS = 300;

interface PtzMotion {
  generation: number;
  cameraId: string;
  direction: PtzDirection;
  startPromise: Promise<void> | null;
  stopPromise: Promise<boolean> | null;
  stopRequested: boolean;
}

interface ActiveKeyboardInteraction {
  key: string;
  direction: PtzDirection;
}

@Component({
  selector: 'app-camera-ptz-controls',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  templateUrl: './camera-ptz-controls.component.html',
  styleUrl: './camera-ptz-controls.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CameraPtzControlsComponent {
  private readonly camerasService = inject(DashboardCamerasService);
  private readonly destroyRef = inject(DestroyRef);
  private motion: PtzMotion | null = null;
  private holdTimer: ReturnType<typeof setTimeout> | null = null;
  private safetyTimer: ReturnType<typeof setTimeout> | null = null;
  private activePointerId: number | null = null;
  private activeKeyboardInteraction: ActiveKeyboardInteraction | null = null;
  private generation = 0;
  private lastManagedActivationAt = 0;
  private destroyed = false;

  readonly cameraId = input.required<string>();
  readonly cameraName = input.required<string>();
  readonly enabled = input(false);
  readonly safetyChange = output<PtzSafetyState>();

  readonly state = signal<PtzControlState>({ kind: 'idle' });
  readonly safetyState = signal<PtzSafetyState>('clear');
  readonly statusMessage = signal('PTZ controls are ready.');

  readonly busy = computed(() => {
    const kind = this.state().kind;
    return kind === 'relative' || kind === 'starting' || kind === 'stopping';
  });
  readonly movementControlsDisabled = computed(
    () =>
      !this.enabled() ||
      this.state().kind === 'relative' ||
      this.safetyState() === 'stopping' ||
      this.safetyState() === 'stopFailed'
  );

  constructor() {
    if (typeof window !== 'undefined' && typeof document !== 'undefined') {
      const stopForBlur = (): void => {
        void this.ensureStopped('Movement stopped because the window lost focus.');
      };
      const stopForVisibility = (): void => {
        if (document.visibilityState === 'hidden') {
          void this.ensureStopped('Movement stopped because the page was hidden.');
        }
      };
      const stopForPageHide = (): void => {
        void this.ensureStopped();
      };

      window.addEventListener('blur', stopForBlur);
      window.addEventListener('pagehide', stopForPageHide);
      document.addEventListener('visibilitychange', stopForVisibility);
      this.destroyRef.onDestroy(() => {
        window.removeEventListener('blur', stopForBlur);
        window.removeEventListener('pagehide', stopForPageHide);
        document.removeEventListener('visibilitychange', stopForVisibility);
      });
    }

    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
      this.clearHoldTimer();
      this.clearSafetyTimer();
      void this.ensureStopped();
    });
  }

  directionButtonDisabled(direction: PtzDirection): boolean {
    const state = this.state();
    if (this.movementControlsDisabled()) {
      return true;
    }

    if (
      state.kind === 'arming' ||
      state.kind === 'starting' ||
      state.kind === 'moving'
    ) {
      return state.direction !== direction;
    }

    return false;
  }

  isDirectionActive(direction: PtzDirection): boolean {
    const state = this.state();
    return (
      (state.kind === 'arming' ||
        state.kind === 'starting' ||
        state.kind === 'moving' ||
        state.kind === 'stopping' ||
        state.kind === 'stopFailed') &&
      state.direction === direction
    );
  }

  onPointerDown(event: PointerEvent, direction: PtzDirection): void {
    if (
      event.button !== 0 ||
      this.activePointerId !== null ||
      this.activeKeyboardInteraction !== null ||
      this.directionButtonDisabled(direction)
    ) {
      return;
    }

    event.preventDefault();
    this.lastManagedActivationAt = performance.now();
    this.activePointerId = event.pointerId;
    (event.currentTarget as HTMLElement).setPointerCapture(event.pointerId);
    this.beginDirection(direction);
  }

  onPointerUp(event: PointerEvent, direction: PtzDirection): void {
    if (event.pointerId !== this.activePointerId) {
      return;
    }

    event.preventDefault();
    this.lastManagedActivationAt = performance.now();
    this.activePointerId = null;
    this.finishDirection(direction, false);
  }

  onPointerCancelled(event: PointerEvent, direction: PtzDirection): void {
    if (event.pointerId !== this.activePointerId) {
      return;
    }

    this.activePointerId = null;
    this.finishDirection(direction, true);
  }

  onLostPointerCapture(event: PointerEvent, direction: PtzDirection): void {
    if (event.pointerId !== this.activePointerId) {
      return;
    }

    this.activePointerId = null;
    this.finishDirection(direction, true);
  }

  onDirectionKeyDown(event: KeyboardEvent, direction: PtzDirection): void {
    if (
      (event.key !== ' ' && event.key !== 'Enter') ||
      event.repeat ||
      this.activeKeyboardInteraction !== null ||
      this.activePointerId !== null ||
      this.directionButtonDisabled(direction)
    ) {
      return;
    }

    event.preventDefault();
    this.lastManagedActivationAt = performance.now();
    this.activeKeyboardInteraction = { key: event.key, direction };
    this.beginDirection(direction);
  }

  onDirectionKeyUp(event: KeyboardEvent, direction: PtzDirection): void {
    const interaction = this.activeKeyboardInteraction;
    if (
      !interaction ||
      event.key !== interaction.key ||
      direction !== interaction.direction
    ) {
      return;
    }

    event.preventDefault();
    this.lastManagedActivationAt = performance.now();
    this.activeKeyboardInteraction = null;
    this.finishDirection(interaction.direction, false);
  }

  onDirectionBlur(direction: PtzDirection): void {
    const interaction = this.activeKeyboardInteraction;
    if (!interaction || direction !== interaction.direction) {
      return;
    }

    this.lastManagedActivationAt = performance.now();
    this.activeKeyboardInteraction = null;
    this.finishDirection(interaction.direction, true);
  }

  onDirectionClick(event: MouseEvent, direction: PtzDirection): void {
    if (
      event.detail !== 0 ||
      performance.now() - this.lastManagedActivationAt < HOLD_DELAY_MS * 2 ||
      this.directionButtonDisabled(direction)
    ) {
      return;
    }

    void this.moveRelatively(direction);
  }

  zoom(amount: -1 | 1): void {
    if (this.movementControlsDisabled() || this.safetyState() !== 'clear') {
      return;
    }

    void this.sendRelativeMovement(
      { horizontal: 0, vertical: 0, zoom: amount * RELATIVE_STEP },
      amount > 0 ? 'Zooming in…' : 'Zooming out…',
      amount > 0 ? 'Camera zoomed in.' : 'Camera zoomed out.'
    );
  }

  stopMovement(): void {
    if (
      this.safetyState() === 'clear' ||
      this.safetyState() === 'stopping'
    ) {
      return;
    }

    void this.ensureStopped('Movement stopped.');
  }

  ensureStopped(successMessage = 'Movement stopped.'): Promise<boolean> {
    this.activePointerId = null;
    this.activeKeyboardInteraction = null;
    this.clearHoldTimer();
    this.clearSafetyTimer();

    const motion = this.motion;
    if (!motion) {
      if (this.state().kind === 'arming') {
        this.setState({ kind: 'idle' });
      }
      return Promise.resolve(true);
    }

    motion.stopRequested = true;
    if (!motion.startPromise) {
      this.motion = null;
      this.setState({ kind: 'idle' });
      this.statusMessage.set('Movement cancelled.');
      return Promise.resolve(true);
    }

    if (motion.stopPromise) {
      return motion.stopPromise;
    }

    this.setState({ kind: 'stopping', direction: motion.direction });
    this.statusMessage.set('Stopping camera movement…');
    motion.stopPromise = this.stopMotion(motion, successMessage).finally(() => {
      if (this.isCurrentMotion(motion)) {
        motion.stopPromise = null;
      }
    });
    return motion.stopPromise;
  }

  private beginDirection(direction: PtzDirection): void {
    if (
      !this.enabled() ||
      this.state().kind === 'relative' ||
      this.motion ||
      this.safetyState() !== 'clear'
    ) {
      return;
    }

    const motion: PtzMotion = {
      generation: ++this.generation,
      cameraId: this.cameraId(),
      direction,
      startPromise: null,
      stopPromise: null,
      stopRequested: false
    };
    this.motion = motion;
    this.setState({ kind: 'arming', direction });
    this.statusMessage.set(`Hold to move ${direction.toLowerCase()}.`);
    this.holdTimer = setTimeout(() => this.startContinuousMotion(motion), HOLD_DELAY_MS);
  }

  private finishDirection(direction: PtzDirection, cancelled: boolean): void {
    const motion = this.motion;
    if (!motion || motion.direction !== direction) {
      return;
    }

    this.clearHoldTimer();
    if (motion.startPromise) {
      void this.ensureStopped();
      return;
    }

    this.motion = null;
    this.setState({ kind: 'idle' });
    if (cancelled) {
      this.statusMessage.set('Movement cancelled.');
      return;
    }

    void this.moveRelatively(direction);
  }

  private startContinuousMotion(motion: PtzMotion): void {
    this.holdTimer = null;
    if (!this.isCurrentMotion(motion) || motion.stopRequested || this.destroyed) {
      return;
    }

    this.setState({ kind: 'starting', direction: motion.direction });
    this.statusMessage.set(`Starting ${motion.direction.toLowerCase()} movement…`);
    motion.startPromise = this.camerasService.startPtzMovement(
      motion.cameraId,
      motion.direction
    );
    this.startSafetyTimer(motion);
    void motion.startPromise
      .then(() => {
        if (!this.isCurrentMotion(motion) || motion.stopRequested || this.destroyed) {
          return;
        }

        this.setState({ kind: 'moving', direction: motion.direction });
        this.statusMessage.set(`Moving ${motion.direction.toLowerCase()}. Release to stop.`);
      })
      .catch(() => {
        if (this.isCurrentMotion(motion)) {
          this.statusMessage.set('Movement could not be started. Confirming that the camera is stopped…');
        }
      })
      .finally(() => {
        if (this.isCurrentMotion(motion) && (motion.stopRequested || this.state().kind === 'starting')) {
          void this.ensureStopped(
            this.state().kind === 'starting'
              ? 'The movement command failed and the camera was stopped.'
              : 'Movement stopped.'
          );
        }
      });
  }

  private async stopMotion(
    motion: PtzMotion,
    successMessage: string
  ): Promise<boolean> {
    void this.camerasService
      .stopPtzMovement(motion.cameraId, motion.direction)
      .catch(() => undefined);

    try {
      await motion.startPromise;
    } catch {
      // A failed response does not prove that the camera rejected Start.
    }

    for (let attempt = 0; attempt < 2; attempt += 1) {
      if (attempt > 0) {
        await new Promise<void>((resolve) => setTimeout(resolve, STOP_RETRY_DELAY_MS));
      }

      try {
        await this.camerasService.stopPtzMovement(
          motion.cameraId,
          motion.direction
        );
        if (this.isCurrentMotion(motion)) {
          this.motion = null;
          this.setState({ kind: 'idle' });
          this.statusMessage.set(successMessage);
        }
        return true;
      } catch {
        // Retry once before exposing the recovery state.
      }
    }

    if (this.isCurrentMotion(motion)) {
      this.setState({ kind: 'stopFailed', direction: motion.direction });
      this.statusMessage.set(
        'Camera movement may still be active. Select Retry Stop before continuing.'
      );
    }
    return false;
  }

  private moveRelatively(direction: PtzDirection): Promise<void> {
    const movement: PtzRelativeMovement =
      direction === 'Left'
        ? { horizontal: -RELATIVE_STEP, vertical: 0, zoom: 0 }
        : direction === 'Right'
          ? { horizontal: RELATIVE_STEP, vertical: 0, zoom: 0 }
          : direction === 'Up'
            ? { horizontal: 0, vertical: RELATIVE_STEP, zoom: 0 }
            : { horizontal: 0, vertical: -RELATIVE_STEP, zoom: 0 };

    return this.sendRelativeMovement(
      movement,
      `Moving ${direction.toLowerCase()}…`,
      `Camera moved ${direction.toLowerCase()}.`
    );
  }

  private async sendRelativeMovement(
    movement: PtzRelativeMovement,
    pendingMessage: string,
    successMessage: string
  ): Promise<void> {
    if (
      !this.enabled() ||
      this.state().kind === 'relative' ||
      this.motion ||
      this.safetyState() !== 'clear'
    ) {
      return;
    }

    const generation = ++this.generation;
    const cameraId = this.cameraId();
    this.setState({ kind: 'relative', label: pendingMessage });
    this.statusMessage.set(pendingMessage);
    try {
      await this.camerasService.movePtzRelatively(cameraId, movement);
      if (this.isCurrentRelative(generation, cameraId)) {
        this.statusMessage.set(successMessage);
      }
    } catch {
      if (this.isCurrentRelative(generation, cameraId)) {
        this.statusMessage.set('The PTZ command failed. Try again.');
      }
    } finally {
      if (!this.destroyed && generation === this.generation) {
        this.setState({ kind: 'idle' });
      }
    }
  }

  private isCurrentRelative(generation: number, cameraId: string): boolean {
    return (
      !this.destroyed &&
      generation === this.generation &&
      cameraId === this.cameraId()
    );
  }

  private isCurrentMotion(motion: PtzMotion): boolean {
    return (
      this.motion === motion &&
      motion.generation === this.generation
    );
  }

  private setState(state: PtzControlState): void {
    this.state.set(state);
    const safety = this.safetyForState(state);
    if (safety !== this.safetyState()) {
      this.safetyState.set(safety);
      this.safetyChange.emit(safety);
    }
  }

  private safetyForState(state: PtzControlState): PtzSafetyState {
    switch (state.kind) {
      case 'starting':
      case 'moving':
      case 'arming':
        return 'active';
      case 'stopping':
        return 'stopping';
      case 'stopFailed':
        return 'stopFailed';
      case 'idle':
      case 'relative':
        return 'clear';
    }
  }

  private clearHoldTimer(): void {
    if (this.holdTimer !== null) {
      clearTimeout(this.holdTimer);
      this.holdTimer = null;
    }
  }

  private startSafetyTimer(motion: PtzMotion): void {
    this.clearSafetyTimer();
    this.safetyTimer = setTimeout(() => {
      this.safetyTimer = null;
      if (!this.isCurrentMotion(motion)) {
        return;
      }

      this.statusMessage.set('Movement reached the 5-second safety limit. Stopping…');
      void this.ensureStopped('Movement stopped at the 5-second safety limit.');
    }, SAFETY_STOP_MS);
  }

  private clearSafetyTimer(): void {
    if (this.safetyTimer !== null) {
      clearTimeout(this.safetyTimer);
      this.safetyTimer = null;
    }
  }
}
