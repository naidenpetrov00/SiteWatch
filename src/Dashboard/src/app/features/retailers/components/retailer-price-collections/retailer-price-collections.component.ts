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
import { injectQuery } from '@tanstack/angular-query-experimental';

import {
  RetailerPriceCollectionRunDetails,
  RetailerPriceCollectionRunSummary,
  isPriceCollectionRunUnfinished
} from '../../models/retailer-price-collection.models';
import {
  RetailerPriceCollectionsService,
  retailerPriceCollectionKeys
} from '../../services/retailer-price-collections.service';
import { getRetailerError } from '../../utils/retailer-error';

@Component({
  selector: 'app-retailer-price-collections',
  imports: [MatButtonModule],
  templateUrl: './retailer-price-collections.component.html',
  styleUrl: './retailer-price-collections.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerPriceCollectionsComponent {
  private readonly service = inject(RetailerPriceCollectionsService);
  private lastObservedRun: {
    id: string;
    succeededCount: number;
    unfinished: boolean;
  } | null = null;

  readonly companyPersonId = input.required<string>();
  readonly activeProfileAvailable = input.required<boolean>();
  readonly selectedRunId = signal<string | null>(null);
  readonly pageIndex = signal(0);
  readonly pageSize = 25;
  readonly startPending = signal(false);
  readonly feedback = signal<string | null>(null);
  readonly error = signal<string | null>(null);

  readonly recentQuery = injectQuery<readonly RetailerPriceCollectionRunSummary[]>(() => {
    const companyPersonId = this.companyPersonId();
    return {
      queryKey: retailerPriceCollectionKeys.recent(companyPersonId),
      queryFn: () => this.service.getRecent(companyPersonId),
      enabled: companyPersonId.length > 0,
      refetchInterval: (query) =>
        query.state.data?.some(isPriceCollectionRunUnfinished) ? 3000 : false
    };
  });

  readonly detailQuery = injectQuery<RetailerPriceCollectionRunDetails>(() => {
    const companyPersonId = this.companyPersonId();
    const runId = this.selectedRunId();
    const pageIndex = this.pageIndex();
    return {
      queryKey: retailerPriceCollectionKeys.detail(
        companyPersonId,
        runId ?? '',
        pageIndex,
        this.pageSize
      ),
      queryFn: () => this.service.getById(
        companyPersonId,
        runId!,
        pageIndex,
        this.pageSize
      ),
      enabled: companyPersonId.length > 0 && runId !== null,
      refetchInterval: (query) =>
        query.state.data && isPriceCollectionRunUnfinished(query.state.data.run)
        ? 3000
        : false
    };
  });

  readonly unfinishedRun = computed(() =>
    (this.recentQuery.data() ?? []).find(isPriceCollectionRunUnfinished) ?? null
  );
  readonly pageCount = computed(() => Math.max(
    1,
    Math.ceil((this.detailQuery.data()?.totalCount ?? 0) / this.pageSize)
  ));
  readonly disabledReason = computed(() => {
    if (this.unfinishedRun()) {
      return 'A company price-collection run is already queued or running.';
    }
    if (!this.activeProfileAvailable()) {
      return 'Publish and activate an extraction profile before collecting prices.';
    }
    if (this.recentQuery.isPending()) {
      return 'Checking recent collection runs…';
    }
    if (this.recentQuery.isError()) {
      return 'Recent collection runs could not be checked.';
    }
    return null;
  });

  constructor() {
    effect(() => {
      const runs = this.recentQuery.data();
      if (!runs?.length) return;
      const selectedId = this.selectedRunId();
      if (!selectedId || !runs.some((run) => run.id === selectedId)) {
        this.selectRun(runs.find(isPriceCollectionRunUnfinished) ?? runs[0]);
      }
    });

    effect(() => {
      const run = this.recentQuery.data()?.[0];
      if (!run) return;
      const current = {
        id: run.id,
        succeededCount: run.succeededCount,
        unfinished: isPriceCollectionRunUnfinished(run)
      };
      const previous = this.lastObservedRun;
      this.lastObservedRun = current;
      const pricesMayHaveChanged = previous === null
        ? current.succeededCount > 0 || !current.unfinished
        : previous.id !== current.id
          ? current.succeededCount > 0 || !current.unfinished
          : current.succeededCount > previous.succeededCount
            || (previous.unfinished && !current.unfinished);
      if (!pricesMayHaveChanged) return;

      void this.service.invalidatePriceDependentQueries(this.companyPersonId());
    });
  }

  async start(): Promise<void> {
    if (this.disabledReason() || this.startPending()) return;
    this.startPending.set(true);
    this.feedback.set(null);
    this.error.set(null);
    try {
      const run = await this.service.start(this.companyPersonId());
      this.selectRun(run);
      this.feedback.set('Price collection was queued. You can leave this page while it runs.');
    } catch (error) {
      this.error.set(getRetailerError(error, 'Price collection could not be started.'));
    } finally {
      this.startPending.set(false);
    }
  }

  selectRun(run: RetailerPriceCollectionRunSummary): void {
    this.pageIndex.set(0);
    this.selectedRunId.set(run.id);
    this.feedback.set(null);
    this.error.set(null);
  }

  previousPage(): void {
    this.pageIndex.update((value) => Math.max(0, value - 1));
  }

  nextPage(): void {
    this.pageIndex.update((value) => Math.min(this.pageCount() - 1, value + 1));
  }

  progress(run: RetailerPriceCollectionRunSummary): number {
    return run.totalCount === 0
      ? 100
      : Math.round((run.processedCount / run.totalCount) * 100);
  }

  statusLabel(status: string): string {
    return status.replace(/([A-Z])/g, ' $1').replace(/^./, (value) => value.toUpperCase());
  }

  formatDate(value: string | null): string {
    return value ? new Date(value).toLocaleString() : '—';
  }

  formatAmount(amount: number | null, currencyCode: string | null): string {
    if (amount === null || currencyCode !== 'EUR') return '—';
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(amount);
  }
}
