import {
  ChangeDetectionStrategy,
  Component,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSiteCamera } from '../../models/camera-stream.models';
import { PtzSafetyState } from '../camera-ptz-controls/camera-ptz.types';
import { WhepViewerComponent } from '../whep-viewer/whep-viewer.component';

export interface CameraLiveViewDialogData {
  siteId: string;
  camera: DashboardSiteCamera;
}

@Component({
  selector: 'app-camera-live-view-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, WhepViewerComponent],
  templateUrl: './camera-live-view-dialog.component.html',
  styleUrl: './camera-live-view-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CameraLiveViewDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<CameraLiveViewDialogComponent>);
  private readonly viewer = viewChild(WhepViewerComponent);

  readonly data = inject<CameraLiveViewDialogData>(MAT_DIALOG_DATA);
  readonly recordingPending = signal(false);
  readonly ptzSafetyState = signal<PtzSafetyState>('clear');
  readonly forceClosePending = signal(false);
  private forceCloseAuthorized = false;

  closeDialog(): void {
    this.dialogRef.close();
  }

  onViewerClosed(): void {
    this.recordingPending.set(false);
    this.ptzSafetyState.set('clear');
    this.dialogRef.close();
  }

  canCloseDialog(): boolean {
    if (this.forceCloseAuthorized) {
      this.forceCloseAuthorized = false;
      return true;
    }

    const viewer = this.viewer();
    if (!viewer) {
      return true;
    }

    if (this.forceClosePending() || this.ptzSafetyState() === 'stopFailed') {
      return false;
    }

    if (viewer.isClosed()) {
      return true;
    }

    void viewer.stop();
    return false;
  }

  onRecordingPendingChange(pending: boolean): void {
    this.recordingPending.set(pending);
  }

  onPtzSafetyChange(safety: PtzSafetyState): void {
    this.ptzSafetyState.set(safety);
  }

  closeAnyway(): void {
    const viewer = this.viewer();
    if (
      !viewer ||
      this.ptzSafetyState() !== 'stopFailed' ||
      this.forceClosePending()
    ) {
      return;
    }

    this.forceClosePending.set(true);
    void viewer.closeAnyway();
  }

  onForcedCloseReady(): void {
    this.forceCloseAuthorized = true;
    this.dialogRef.close();
  }
}
