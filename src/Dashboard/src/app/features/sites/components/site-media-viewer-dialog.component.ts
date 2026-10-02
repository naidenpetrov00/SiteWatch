import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

export interface SiteMediaViewerDialogData {
  kind: 'image' | 'video';
  url: string;
  label: string;
}

@Component({
  selector: 'app-site-media-viewer-dialog',
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>{{ data.label }}</h2>
    <mat-dialog-content>
      @if (data.kind === 'image') {
        <img [src]="data.url" [alt]="data.label" class="viewer-media" />
      } @else {
        <video [src]="data.url" controls autoplay class="viewer-media">Your browser cannot play this video.</video>
      }
    </mat-dialog-content>
    <mat-dialog-actions align="end"><button mat-button mat-dialog-close type="button">Close</button></mat-dialog-actions>
  `,
  styles: `.viewer-media { display: block; max-width: min(78rem, calc(100vw - 5rem)); max-height: min(75vh, 52rem); }`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SiteMediaViewerDialogComponent {
  readonly data = inject<SiteMediaViewerDialogData>(MAT_DIALOG_DATA);
}
