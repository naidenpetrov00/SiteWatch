import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { RetailerPriceHistory } from '../../models/retailer-listing.models';
import {
  RetailerListingsService,
  getRetailerListingError
} from '../../services/retailer-listings.service';

@Component({
  selector: 'app-retailer-price-history',
  imports: [MatPaginatorModule, MatProgressSpinnerModule],
  templateUrl: './retailer-price-history.component.html',
  styleUrl: './retailer-price-history.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerPriceHistoryComponent {
  private readonly listingsService = inject(RetailerListingsService);
  private loadRevision = 0;

  readonly productId = input.required<string>();
  readonly productLabel = input.required<string>();
  readonly retailerId = input.required<string>();
  readonly retailerName = input.required<string>();
  readonly refreshToken = input<unknown>(null);
  readonly history = signal<RetailerPriceHistory | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly pageSize = 25;

  constructor() {
    effect(() => {
      const productId = this.productId();
      const retailerId = this.retailerId();
      this.refreshToken();
      if (productId && retailerId) void this.loadPage(0);
    });
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
    const revision = ++this.loadRevision;
    this.loading.set(true);
    this.error.set(null);
    try {
      const history = await this.listingsService.getHistory(
        this.productId(),
        this.retailerId(),
        pageIndex,
        this.pageSize
      );
      if (revision === this.loadRevision) this.history.set(history);
    } catch (error) {
      if (revision === this.loadRevision) {
        this.error.set(
          getRetailerListingError(error, 'Price history could not be loaded.')
        );
      }
    } finally {
      if (revision === this.loadRevision) this.loading.set(false);
    }
  }
}
