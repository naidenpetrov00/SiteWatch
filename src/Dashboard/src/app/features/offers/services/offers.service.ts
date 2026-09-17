import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import {
  QueryClient,
  injectMutation,
  injectQuery
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import {
  CreateOfferResponse,
  OfferDetails,
  OfferSiteIdentity,
  OfferSummary,
  SiteOffersResponse,
  UpdateOfferMetadataRequest
} from '../models/offer.models';

interface SiteOffersQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

interface OfferRouteIdentity {
  siteId: string;
  offerId: string;
}

const DEFAULT_QUERY_STATE: SiteOffersQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({ providedIn: 'root' })
export class OffersService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly listSiteId = signal('');
  private readonly detailRoute = signal<OfferRouteIdentity>({
    siteId: '',
    offerId: ''
  });
  private readonly queryState = signal<SiteOffersQueryState>(DEFAULT_QUERY_STATE);

  readonly siteQuery = injectQuery<OfferSiteIdentity>(() => {
    const siteId = this.listSiteId();
    return {
      queryKey: ['sites', 'dashboard', 'detail', siteId] as const,
      queryFn: () =>
        firstValueFrom(
          this.http.get<OfferSiteIdentity>(buildApiUrl(`/dashboard/sites/${siteId}`))
        ),
      enabled: siteId.length > 0
    };
  });

  readonly siteOffersQuery = injectQuery<SiteOffersResponse>(() => {
    const siteId = this.listSiteId();
    const state = this.queryState();
    return {
      queryKey: [
        'offers',
        'site',
        siteId,
        'list',
        this.queryKeyFromState(state)
      ] as const,
      queryFn: () =>
        firstValueFrom(
          this.http.get<SiteOffersResponse>(
            buildApiUrl(`/sites/${siteId}/offers`),
            { params: this.buildQueryParams(state) }
          )
        ),
      enabled: siteId.length > 0
    };
  });

  readonly offerDetailsQuery = injectQuery<OfferDetails>(() => {
    const route = this.detailRoute();
    return {
      queryKey: [
        'offers',
        'site',
        route.siteId,
        'detail',
        route.offerId
      ] as const,
      queryFn: () =>
        firstValueFrom(
          this.http.get<OfferDetails>(
            buildApiUrl(`/sites/${route.siteId}/offers/${route.offerId}`)
          )
        ),
      enabled: route.siteId.length > 0 && route.offerId.length > 0
    };
  });

  readonly createOfferMutation = injectMutation<
    CreateOfferResponse,
    Error,
    string
  >(() => ({
    mutationKey: ['offers', 'create'],
    mutationFn: (siteId) =>
      firstValueFrom(
        this.http.post<CreateOfferResponse>(
          buildApiUrl(`/sites/${siteId}/offers`),
          {}
        )
      ),
    onSuccess: async (_, siteId) => this.invalidateSiteOffers(siteId)
  }));

  readonly updateOfferMutation = injectMutation<
    void,
    Error,
    UpdateOfferMetadataRequest
  >(() => ({
    mutationKey: ['offers', 'update'],
    mutationFn: ({ siteId, offerId, title, notes }) =>
      firstValueFrom(
        this.http.put<void>(
          buildApiUrl(`/sites/${siteId}/offers/${offerId}`),
          { title, notes }
        )
      ),
    onSuccess: async (_, request) => this.invalidateSiteOffers(request.siteId)
  }));

  readonly archiveOfferMutation = injectMutation<void, Error, OfferRouteIdentity>(
    () => ({
      mutationKey: ['offers', 'archive'],
      mutationFn: ({ siteId, offerId }) =>
        firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(`/sites/${siteId}/offers/${offerId}/archive`),
            {}
          )
        ),
      onSuccess: async (_, request) => this.invalidateSiteOffers(request.siteId)
    })
  );

  configureList(siteId: string): void {
    if (this.listSiteId() !== siteId) {
      this.listSiteId.set(siteId);
      this.queryState.set(DEFAULT_QUERY_STATE);
    }
  }

  configureDetails(siteId: string, offerId: string): void {
    const current = this.detailRoute();
    if (current.siteId !== siteId || current.offerId !== offerId) {
      this.detailRoute.set({ siteId, offerId });
    }
  }

  setTableState(state: DataTableState<OfferSummary>): void {
    const nextState: SiteOffersQueryState = {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
    if (this.queryKeyFromState(this.queryState()) !== this.queryKeyFromState(nextState)) {
      this.queryState.set(nextState);
    }
  }

  createOffer(siteId: string): Promise<CreateOfferResponse> {
    return this.createOfferMutation.mutateAsync(siteId);
  }

  updateOffer(request: UpdateOfferMetadataRequest): Promise<void> {
    return this.updateOfferMutation.mutateAsync(request);
  }

  archiveOffer(siteId: string, offerId: string): Promise<void> {
    return this.archiveOfferMutation.mutateAsync({ siteId, offerId });
  }

  private async invalidateSiteOffers(siteId: string): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: ['offers', 'site', siteId]
    });
  }

  private buildQueryParams(state: SiteOffersQueryState): HttpParams {
    let params = new HttpParams()
      .set('pageIndex', state.pageIndex)
      .set('pageSize', state.pageSize);
    if (state.sortActive) {
      params = params.set('sortActive', state.sortActive);
    }
    if (state.sortDirection) {
      params = params.set('sortDirection', state.sortDirection);
    }
    for (const [key, value] of Object.entries(
      this.normalizeFilters(state.appliedFilters)
    )) {
      params = params.set(key, value);
    }
    return params;
  }

  private queryKeyFromState(state: SiteOffersQueryState): string {
    return JSON.stringify({
      pageIndex: state.pageIndex,
      pageSize: state.pageSize,
      sortActive: state.sortActive,
      sortDirection: state.sortDirection,
      appliedFilters: Object.entries(
        this.normalizeFilters(state.appliedFilters)
      ).sort(([left], [right]) => left.localeCompare(right))
    });
  }

  private normalizeFilters(
    filters: Readonly<Record<string, string>>
  ): Record<string, string> {
    return Object.entries(filters).reduce<Record<string, string>>(
      (normalized, [key, value]) => {
        const normalizedValue = value.trim().toLowerCase();
        if (normalizedValue) {
          normalized[key] = normalizedValue;
        }
        return normalized;
      },
      {}
    );
  }
}
