import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface RetailerListingStatusConfirmDialogData {
  label: string;
  activate: boolean;
}

@Component({
  selector: 'app-retailer-listing-status-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule],
  templateUrl: './retailer-listing-status-confirm-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerListingStatusConfirmDialogComponent {
  readonly data = inject<RetailerListingStatusConfirmDialogData>(MAT_DIALOG_DATA);
}
