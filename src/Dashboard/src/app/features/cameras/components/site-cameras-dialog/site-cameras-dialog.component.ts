import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSiteCamera } from '../../models/camera-stream.models';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';
import { WhepViewerComponent } from '../whep-viewer/whep-viewer.component';

export interface SiteCamerasDialogData {
  siteId: string;
  siteName: string;
  siteNumberId: number;
}

@Component({
  selector: 'app-site-cameras-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    WhepViewerComponent
  ],
  templateUrl: './site-cameras-dialog.component.html',
  styleUrl: './site-cameras-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SiteCamerasDialogComponent {
  private readonly camerasService = inject(DashboardCamerasService);
  private readonly dialogRef = inject(MatDialogRef<SiteCamerasDialogComponent>);
  private readonly destroyRef = inject(DestroyRef);
  private readonly viewer = viewChild(WhepViewerComponent);
  private destroyed = false;
  private closeAfterViewerStop = false;

  readonly data = inject<SiteCamerasDialogData>(MAT_DIALOG_DATA);
  readonly cameras = signal<readonly DashboardSiteCamera[]>([]);
  readonly selectedCamera = signal<DashboardSiteCamera | null>(null);
  readonly isLoading = signal(true);
  readonly loadFailed = signal(false);
  readonly recordingPending = signal(false);

  constructor() {
    this.destroyRef.onDestroy(() => {
      this.destroyed = true;
    });
    void this.loadCameras();
  }

  selectCamera(camera: DashboardSiteCamera): void {
    if (this.recordingPending() || this.selectedCamera()?.id === camera.id) {
      return;
    }
    this.selectedCamera.set(camera);
  }

  clearSelection(): void {
    this.selectedCamera.set(null);
  }

  closeDialog(): void {
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

    this.closeAfterViewerStop = true;
    void viewer.stop();
    return false;
  }

  onRecordingPendingChange(pending: boolean): void {
    this.recordingPending.set(pending);
  }

  onViewerClosed(): void {
    this.recordingPending.set(false);
    if (this.closeAfterViewerStop) {
      this.closeAfterViewerStop = false;
      this.dialogRef.close();
      return;
    }

    this.clearSelection();
  }

  private async loadCameras(): Promise<void> {
    this.isLoading.set(true);
    this.loadFailed.set(false);
    try {
      const cameras = await this.camerasService.getCamerasBySite(this.data.siteId);
      if (!this.destroyed) {
        this.cameras.set(cameras);
      }
    } catch {
      if (!this.destroyed) {
        this.loadFailed.set(true);
      }
    } finally {
      if (!this.destroyed) {
        this.isLoading.set(false);
      }
    }
  }
}
