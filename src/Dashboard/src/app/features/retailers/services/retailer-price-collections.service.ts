import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  QueryClient,
  injectMutation
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  RetailerPriceCollectionRunDetails,
  RetailerPriceCollectionRunSummary
} from '../models/retailer-price-collection.models';

export const retailerPriceCollectionKeys = {
  root: (companyPersonId: string) =>
    ['retailer-price-collection-runs', companyPersonId] as const,
  recent: (companyPersonId: string, limit = 10) =>
    ['retailer-price-collection-runs', companyPersonId, 'recent', limit] as const,
  details: (companyPersonId: string, runId: string) =>
    ['retailer-price-collection-runs', companyPersonId, 'detail', runId] as const,
  detail: (
    companyPersonId: string,
    runId: string,
    pageIndex: number,
    pageSize: number
  ) => [
    'retailer-price-collection-runs',
    companyPersonId,
    'detail',
    runId,
    pageIndex,
    pageSize
  ] as const
};

@Injectable({ providedIn: 'root' })
export class RetailerPriceCollectionsService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly startMutation = injectMutation<
    RetailerPriceCollectionRunSummary,
    Error,
    string
  >(() => ({
    mutationKey: ['retailer-price-collection-runs', 'start'],
    mutationFn: (companyPersonId) => firstValueFrom(
      this.http.post<RetailerPriceCollectionRunSummary>(
        this.baseUrl(companyPersonId),
        {}
      )
    )
  }));

  getRecent(
    companyPersonId: string,
    limit = 10
  ): Promise<readonly RetailerPriceCollectionRunSummary[]> {
    return firstValueFrom(
      this.http.get<readonly RetailerPriceCollectionRunSummary[]>(
        this.baseUrl(companyPersonId),
        { params: new HttpParams().set('limit', limit) }
      )
    );
  }

  getById(
    companyPersonId: string,
    runId: string,
    pageIndex: number,
    pageSize: number
  ): Promise<RetailerPriceCollectionRunDetails> {
    return firstValueFrom(
      this.http.get<RetailerPriceCollectionRunDetails>(
        `${this.baseUrl(companyPersonId)}/${runId}`,
        {
          params: new HttpParams()
            .set('pageIndex', pageIndex)
            .set('pageSize', pageSize)
        }
      )
    );
  }

  async start(companyPersonId: string): Promise<RetailerPriceCollectionRunSummary> {
    const run = await this.startMutation.mutateAsync(companyPersonId);
    await this.invalidateRunQueries(companyPersonId, run.id);
    return run;
  }

  async invalidatePriceDependentQueries(companyPersonId: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: ['retailer-listings', 'company', companyPersonId]
      }),
      this.queryClient.invalidateQueries({
        queryKey: ['retailer-listings', 'detail']
      }),
      this.queryClient.invalidateQueries({
        queryKey: ['retailer-pricing']
      }),
      this.queryClient.invalidateQueries({
        predicate: (query) =>
          query.queryKey[0] === 'offers' && query.queryKey.includes('pricing')
      })
    ]);
  }

  private async invalidateRunQueries(
    companyPersonId: string,
    runId: string
  ): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerPriceCollectionKeys.recent(companyPersonId)
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerPriceCollectionKeys.details(companyPersonId, runId)
      })
    ]);
  }

  private baseUrl(companyPersonId: string): string {
    return buildApiUrl(`/persons/${companyPersonId}/price-collection-runs`);
  }
}
