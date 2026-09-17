import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import {
  OFFER_STATUS_OPTIONS,
  OfferSummary
} from '../models/offer.models';
import { OffersService } from '../services/offers.service';

const OFFER_COLUMNS: readonly DataTableColumn<OfferSummary>[] = [
  {
    key: 'numberId',
    label: 'Offer Number',
    sortable: true,
    cellType: 'button',
    filter: { kind: 'number', placeholder: 'Filter Offer Number' },
    ariaLabelAccessor: (offer) => `Open Offer ${offer.numberId}`
  },
  {
    key: 'title',
    label: 'Internal Title',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Internal Title' },
    displayFormatter: (value) =>
      typeof value === 'string' && value.length > 0 ? value : 'Untitled offer'
  },
  {
    key: 'status',
    label: 'Status',
    sortable: true,
    filter: {
      kind: 'select',
      placeholder: 'Filter Status',
      options: OFFER_STATUS_OPTIONS
    }
  },
  {
    key: 'created',
    label: 'Created',
    sortable: true,
    displayFormatter: formatDateTime
  },
  {
    key: 'lastModified',
    label: 'Modified',
    sortable: true,
    displayFormatter: formatDateTime
  }
] as const;

@Component({
  selector: 'app-site-offers-page',
  imports: [DataTableComponent, MatButtonModule, RouterLink],
  templateUrl: './site-offers.page.html',
  styleUrl: './site-offers.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SiteOffersPage {
  private readonly offersService = inject(OffersService);
  private readonly router = inject(Router);

  readonly siteId = input.required<string>();
  readonly tableState = signal<DataTableState<OfferSummary> | null>(null);
  readonly columns = OFFER_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;
  readonly site = computed(() => this.offersService.siteQuery.data());
  readonly offers = computed(
    () => this.offersService.siteOffersQuery.data()?.items ?? []
  );
  readonly filteredCount = computed(
    () => this.offersService.siteOffersQuery.data()?.filteredCount ?? 0
  );
  readonly totalCount = computed(
    () => this.offersService.siteOffersQuery.data()?.totalCount ?? 0
  );

  constructor() {
    effect(() => this.offersService.configureList(this.siteId()));
    effect(() => {
      const state = this.tableState();
      if (state) {
        this.offersService.setTableState(state);
      }
    });
  }

  onTableStateChange(state: DataTableState<OfferSummary>): void {
    this.tableState.set(state);
  }

  async openOffer(offer: OfferSummary): Promise<void> {
    await this.router.navigate(['/sites', this.siteId(), 'offers', offer.id]);
  }

  isLoading(): boolean {
    return (
      this.offersService.siteQuery.isPending() ||
      this.offersService.siteOffersQuery.isPending()
    );
  }

  hasLoadError(): boolean {
    return (
      this.offersService.siteQuery.isError() ||
      this.offersService.siteOffersQuery.isError()
    );
  }
}

function formatDateTime(value: unknown): string {
  if (typeof value !== 'string') {
    return '';
  }

  return new Intl.DateTimeFormat(undefined, {
    dateStyle: 'medium',
    timeStyle: 'short'
  }).format(new Date(value));
}
