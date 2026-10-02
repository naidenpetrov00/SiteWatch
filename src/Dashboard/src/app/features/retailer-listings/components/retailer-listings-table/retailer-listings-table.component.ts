import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input,
  signal
} from '@angular/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialog } from '@angular/material/dialog';
import { injectQuery } from '@tanstack/angular-query-experimental';

import { DataTableComponent } from '../../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableRowAction,
  DataTableRowActionEvent,
  DataTableState
} from '../../../../shared/data-table/data-table.types';
import {
  RetailerListing,
  RetailerListingDialogData,
  RetailerListingFixedProduct,
  RetailerListingFixedRetailer,
  RetailerListingQueryState,
  RetailerPriceHistoryDialogData
} from '../../models/retailer-listing.models';
import {
  RetailerListingsService,
  retailerListingKeys
} from '../../services/retailer-listings.service';
import { RetailerListingDialogComponent } from '../retailer-listing-dialog/retailer-listing-dialog.component';
import { RetailerPriceHistoryDialogComponent } from '../retailer-price-history-dialog/retailer-price-history-dialog.component';

@Component({
  selector: 'app-retailer-listings-table',
  imports: [DataTableComponent, MatCheckboxModule],
  templateUrl: './retailer-listings-table.component.html',
  styleUrl: './retailer-listings-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerListingsTableComponent {
  private readonly listingsService = inject(RetailerListingsService);
  private readonly dialog = inject(MatDialog);

  readonly perspective = input.required<'product' | 'retailer'>();
  readonly ownerId = input.required<string>();
  readonly fixedProduct = input<RetailerListingFixedProduct | undefined>();
  readonly fixedRetailer = input<RetailerListingFixedRetailer | undefined>();
  readonly includeInactive = signal(true);
  readonly tableState = signal<DataTableState<RetailerListing> | null>(null);
  readonly pageSize = 25;
  readonly pageSizeOptions = [25, 50, 100] as const;

  readonly queryState = computed<RetailerListingQueryState>(() => {
    const state = this.tableState();
    const counterpartKey =
      this.perspective() === 'product' ? 'retailerDisplayName' : 'productTitle';
    return {
      pageIndex: state?.page.pageIndex ?? 0,
      pageSize: state?.page.pageSize ?? this.pageSize,
      sortActive: state?.sort.active ?? '',
      sortDirection: state?.sort.direction ?? '',
      searchTerm: state?.appliedFilters[counterpartKey] ?? '',
      includeInactive: this.includeInactive(),
      isActive:
        state?.appliedFilters['isActive'] === 'true'
          ? true
          : state?.appliedFilters['isActive'] === 'false'
            ? false
            : null
    };
  });

  readonly listingsQuery = injectQuery(() => {
    const ownerId = this.ownerId();
    const state = this.queryState();
    const perspective = this.perspective();
    return {
      queryKey:
        perspective === 'product'
          ? retailerListingKeys.product(ownerId, state)
          : retailerListingKeys.retailer(ownerId, state),
      queryFn: () =>
        perspective === 'product'
          ? this.listingsService.getForProduct(ownerId, state)
          : this.listingsService.getForRetailer(ownerId, state),
      enabled: ownerId.length > 0
    };
  });

  readonly columns = computed<readonly DataTableColumn<RetailerListing>[]>(() => {
    const counterpart: DataTableColumn<RetailerListing> =
      this.perspective() === 'product'
        ? {
            key: 'retailerDisplayName',
            label: 'Retailer',
            sortable: true,
            sortKey: 'retailer',
            cellType: 'internal-link',
            filter: { kind: 'text', placeholder: 'Search Retailers' },
            linkRouteAccessor: (listing) => [
              '/manage-retailers',
              listing.retailerId
            ],
            ariaLabelAccessor: (listing) =>
              'Open ' + listing.retailerDisplayName + ' listing page'
          }
        : {
            key: 'productTitle',
            label: 'Product',
            sortable: true,
            sortKey: 'product',
            cellType: 'internal-link',
            filter: { kind: 'text', placeholder: 'Search Products' },
            displayFormatter: (_, listing) =>
              '#' + listing.productNumberId + ' · ' + listing.productTitle,
            linkRouteAccessor: (listing) => [
              '/manage-products',
              listing.productId
            ],
            ariaLabelAccessor: (listing) =>
              'Open Product ' + listing.productNumberId + ', ' + listing.productTitle
          };

    const catalogStatus: DataTableColumn<RetailerListing> =
      this.perspective() === 'product'
        ? {
            key: 'retailerIsActive',
            label: 'Retailer status',
            sortable: false,
            displayFormatter: (value) => (value ? 'Active' : 'Inactive')
          }
        : {
            key: 'productStatus',
            label: 'Product status',
            sortable: false
          };

    return [
      counterpart,
      catalogStatus,
      {
        key: 'latestObservation',
        label: 'Latest EUR price',
        sortable: true,
        sortKey: 'latestPrice',
        valueAccessor: (listing) => listing.latestObservation?.amount ?? null,
        displayFormatter: (_, listing) =>
          listing.latestObservation
            ? this.formatCurrency(listing.latestObservation.amount)
            : 'No price',
        align: 'end'
      },
      {
        key: 'productPackageUnit',
        label: 'Basis',
        sortable: false,
        valueAccessor: (listing) => listing.latestObservation?.basis ?? null,
        displayFormatter: (_, listing) =>
          listing.latestObservation
            ? 'per ' + listing.latestObservation.basis
            : '—'
      },
      {
        key: 'lastModified',
        label: 'Observed',
        sortable: true,
        sortKey: 'observedAt',
        valueAccessor: (listing) => listing.latestObservation?.observedAt ?? null,
        displayFormatter: (_, listing) =>
          listing.latestObservation
            ? this.relativeAge(listing.latestObservation.observedAt)
            : '—',
        tooltipAccessor: (listing) =>
          listing.latestObservation
            ? this.exactDate(listing.latestObservation.observedAt)
            : null
      },
      {
        key: 'retailerProductCode',
        label: 'Retailer code',
        sortable: true
      },
      {
        key: 'productUrl',
        label: 'Product URL',
        sortable: false,
        cellType: 'external-link',
        displayFormatter: (value) => (value ? 'Open product' : '—'),
        linkHrefAccessor: (listing) => listing.productUrl,
        ariaLabelAccessor: (listing) =>
          'Open ' + listing.retailerDisplayName + ' product page'
      },
      {
        key: 'isActive',
        label: 'Listing status',
        sortable: true,
        filter: {
          kind: 'select',
          placeholder: 'Filter Listing Status',
          options: [
            { label: 'Active', value: 'true' },
            { label: 'Inactive', value: 'false' }
          ]
        },
        displayFormatter: (value) => (value ? 'Active' : 'Inactive')
      }
    ];
  });

  readonly rowActions: readonly DataTableRowAction<RetailerListing>[] = [
    {
      id: 'manage',
      label: 'Manage',
      ariaLabelAccessor: (listing) =>
        'Manage ' + listing.productTitle + ' at ' + listing.retailerDisplayName
    },
    {
      id: 'history',
      label: 'History',
      ariaLabelAccessor: (listing) =>
        'View price history for ' +
        listing.productTitle +
        ' at ' +
        listing.retailerDisplayName
    }
  ];

  onTableStateChange(state: DataTableState<RetailerListing>): void {
    this.tableState.set(state);
  }

  async onRowAction(event: DataTableRowActionEvent<RetailerListing>): Promise<void> {
    if (event.action.id === 'history') {
      this.openHistory(event.row);
      return;
    }

    let listing = event.row;
    try {
      listing = await this.listingsService.getById(event.row.id);
    } catch {
      // The row still provides a complete dialog fallback if refresh fails.
    }
    this.dialog.open<
      RetailerListingDialogComponent,
      RetailerListingDialogData,
      RetailerListing | null
    >(RetailerListingDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '52rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        listing,
        fixedProduct: this.fixedProduct(),
        fixedRetailer: this.fixedRetailer()
      }
    });
  }

  private openHistory(listing: RetailerListing): void {
    this.dialog.open<
      RetailerPriceHistoryDialogComponent,
      RetailerPriceHistoryDialogData
    >(RetailerPriceHistoryDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '48rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        productId: listing.productId,
        productLabel:
          '#' + listing.productNumberId + ' · ' + listing.productTitle,
        retailerId: listing.retailerId,
        retailerName: listing.retailerDisplayName
      }
    });
  }

  private formatCurrency(amount: number): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }

  private relativeAge(value: string): string {
    const observed = new Date(value);
    const today = new Date();
    const observedDay = new Date(
      observed.getFullYear(),
      observed.getMonth(),
      observed.getDate()
    );
    const todayDay = new Date(
      today.getFullYear(),
      today.getMonth(),
      today.getDate()
    );
    const days = Math.max(
      0,
      Math.round((todayDay.getTime() - observedDay.getTime()) / 86_400_000)
    );
    return new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' }).format(
      -days,
      'day'
    );
  }

  private exactDate(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }
}
