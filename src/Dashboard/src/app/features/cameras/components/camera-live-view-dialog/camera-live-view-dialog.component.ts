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

  closeDialog(): void {
    this.dialogRef.close();
  }

  onViewerClosed(): void {
    this.recordingPending.set(false);
    this.dialogRef.close();
  }

  canCloseDialog(): boolean {
    if (!this.recordingPending()) {
      return true;
    }

    const viewer = this.viewer();
    if (!viewer) {
      return true;
    }

    void viewer.stop();
    return false;
  }

  onRecordingPendingChange(pending: boolean): void {
    this.recordingPending.set(pending);
  }
}
