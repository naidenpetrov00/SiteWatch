import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { RetailerPriceHistory } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';

export interface OfferPriceHistoryDialogData {
  productId: string;
  productLabel: string;
  retailerId: string;
  retailerName: string;
}

@Component({
  selector: 'app-offer-price-history-dialog',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatPaginatorModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './offer-price-history-dialog.component.html',
  styleUrl: './offer-price-history-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPriceHistoryDialogComponent {
  readonly data = inject<OfferPriceHistoryDialogData>(MAT_DIALOG_DATA);
  private readonly offersService = inject(OffersService);

  readonly history = signal<RetailerPriceHistory | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly pageSize = 25;

  constructor() {
    void this.loadPage(0);
  }

  onPage(event: PageEvent): void {
    void this.loadPage(event.pageIndex);
  }

  formatCurrency(amount: number): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }

  formatDateTime(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  private async loadPage(pageIndex: number): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      this.history.set(
        await this.offersService.getRetailerPriceHistory(
          this.data.productId,
          this.data.retailerId,
          pageIndex,
          this.pageSize
        )
      );
    } catch (error) {
      this.error.set(getOfferError(error, 'Price history could not be loaded.'));
    } finally {
      this.loading.set(false);
    }
  }
}
