import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

import { RetailerPriceHistoryDialogData } from '../../models/retailer-listing.models';
import { RetailerPriceHistoryComponent } from '../retailer-price-history/retailer-price-history.component';

@Component({
  selector: 'app-retailer-price-history-dialog',
  imports: [MatButtonModule, MatDialogModule, RetailerPriceHistoryComponent],
  templateUrl: './retailer-price-history-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerPriceHistoryDialogComponent {
  readonly data = inject<RetailerPriceHistoryDialogData>(MAT_DIALOG_DATA);
}
