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
  AddOfferActivityRequest,
  AddOfferPricingRetailerRequest,
  CreateOfferResponse,
  OfferActivityCandidate,
  OfferActivityCatalogNode,
  OfferDetails,
  OfferPricingMatrix,
  OfferPricingProductRow,
  OfferRetailerPriceCell,
  OfferSiteIdentity,
  OfferSummary,
  RecordManualOfferPriceRequest,
  RemoveOfferActivityRequest,
  RetailerPriceHistory,
  SelectOfferProductPriceRequest,
  SiteOffersResponse,
  UpdateOfferActivityMeasurementsRequest,
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

interface OfferCatalogRouteIdentity extends OfferRouteIdentity {
  searchTerm: string;
}

interface OfferActivityCreatedResponse {
  id: string;
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
  private readonly catalogRoute = signal<OfferCatalogRouteIdentity>({
    siteId: '',
    offerId: '',
    searchTerm: ''
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

  readonly pricingMatrixQuery = injectQuery<OfferPricingMatrix>(() => {
    const route = this.detailRoute();
    return {
      queryKey: this.pricingQueryKey(route.siteId, route.offerId),
      queryFn: () =>
        firstValueFrom(
          this.http.get<OfferPricingMatrix>(
            buildApiUrl(`/sites/${route.siteId}/offers/${route.offerId}/pricing`)
          )
        ),
      enabled: route.siteId.length > 0 && route.offerId.length > 0
    };
  });

  readonly activityCatalogQuery = injectQuery<
    readonly OfferActivityCatalogNode[]
  >(() => {
    const route = this.catalogRoute();
    return {
      queryKey: [
        'offers',
        'site',
        route.siteId,
        'detail',
        route.offerId,
        'activity-catalog',
        route.searchTerm
      ] as const,
      queryFn: () => {
        let params = new HttpParams();
        if (route.searchTerm) {
          params = params.set('searchTerm', route.searchTerm);
        }
        return firstValueFrom(
          this.http.get<readonly OfferActivityCatalogNode[]>(
            buildApiUrl(
              `/sites/${route.siteId}/offers/${route.offerId}/activity-catalog`
            ),
            { params }
          )
        );
      },
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

  readonly addActivityMutation = injectMutation<
    OfferActivityCreatedResponse,
    Error,
    AddOfferActivityRequest
  >(() => ({
    mutationKey: ['offers', 'activity', 'add'],
    mutationFn: ({ siteId, offerId, activityId, sectionMeasurements }) =>
      firstValueFrom(
        this.http.post<OfferActivityCreatedResponse>(
          buildApiUrl(`/sites/${siteId}/offers/${offerId}/activities`),
          { activityId, sectionMeasurements }
        )
      ),
    onSuccess: async (_, request) =>
      this.invalidateOfferWorkspace(request.siteId, request.offerId)
  }));

  readonly removeActivityMutation = injectMutation<
    void,
    Error,
    RemoveOfferActivityRequest
  >(() => ({
    mutationKey: ['offers', 'activity', 'remove'],
    mutationFn: ({ siteId, offerId, offerActivityId }) =>
      firstValueFrom(
        this.http.delete<void>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/activities/${offerActivityId}`
          )
        )
      ),
    onSuccess: async (_, request) =>
      this.invalidateOfferWorkspace(request.siteId, request.offerId)
  }));

  readonly updateMeasurementsMutation = injectMutation<
    void,
    Error,
    UpdateOfferActivityMeasurementsRequest
  >(() => ({
    mutationKey: ['offers', 'activity', 'measurements'],
    mutationFn: ({ siteId, offerId, offerActivityId, sectionMeasurements }) =>
      firstValueFrom(
        this.http.put<void>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/activities/${offerActivityId}/section-measurements`
          ),
          { sectionMeasurements }
        )
      ),
    onSuccess: async (_, request) =>
      this.invalidateOfferWorkspace(request.siteId, request.offerId)
  }));

  readonly addPricingRetailerMutation = injectMutation<
    OfferPricingMatrix,
    Error,
    AddOfferPricingRetailerRequest
  >(() => ({
    mutationKey: ['offers', 'pricing', 'retailer', 'add'],
    mutationFn: ({ siteId, offerId, retailerId }) =>
      firstValueFrom(
        this.http.post<OfferPricingMatrix>(
          buildApiUrl(`/sites/${siteId}/offers/${offerId}/pricing/retailers`),
          { retailerId }
        )
      ),
    onSuccess: async (matrix, request) => {
      this.queryClient.setQueryData(
        this.pricingQueryKey(request.siteId, request.offerId),
        matrix
      );
      await this.invalidateOfferPricingMutation(request.siteId, request.offerId);
    }
  }));

  readonly removePricingRetailerMutation = injectMutation<
    void,
    Error,
    AddOfferPricingRetailerRequest
  >(() => ({
    mutationKey: ['offers', 'pricing', 'retailer', 'remove'],
    mutationFn: ({ siteId, offerId, retailerId }) =>
      firstValueFrom(
        this.http.delete<void>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/pricing/retailers/${retailerId}`
          )
        )
      ),
    onSuccess: async (_, request) =>
      this.invalidateOfferPricingMutation(request.siteId, request.offerId)
  }));

  readonly recordManualPriceMutation = injectMutation<
    OfferRetailerPriceCell,
    Error,
    RecordManualOfferPriceRequest
  >(() => ({
    mutationKey: ['offers', 'pricing', 'price', 'manual'],
    mutationFn: ({
      siteId,
      offerId,
      offerProductLineId,
      retailerId,
      amount,
      basis,
      productUrl,
      retailerProductCode
    }) =>
      firstValueFrom(
        this.http.put<OfferRetailerPriceCell>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/pricing/products/${offerProductLineId}/retailers/${retailerId}/current-price`
          ),
          { amount, basis, productUrl, retailerProductCode }
        )
      ),
    onSuccess: async (cell, request) => {
      this.patchPricingCell(request.siteId, request.offerId, cell);
      await Promise.all([
        this.queryClient.invalidateQueries({
          queryKey: this.pricingQueryKey(request.siteId, request.offerId),
          exact: true
        }),
        this.queryClient.invalidateQueries({
          queryKey: this.priceHistoryQueryKey(request.productId, request.retailerId)
        })
      ]);
    }
  }));

  readonly selectProductPriceMutation = injectMutation<
    OfferPricingProductRow,
    Error,
    SelectOfferProductPriceRequest
  >(() => ({
    mutationKey: ['offers', 'pricing', 'selection', 'set'],
    mutationFn: ({
      siteId,
      offerId,
      offerProductLineId,
      retailerId,
      observationId
    }) =>
      firstValueFrom(
        this.http.put<OfferPricingProductRow>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/pricing/products/${offerProductLineId}/selection`
          ),
          { retailerId, observationId }
        )
      ),
    onSuccess: async (row, request) => {
      this.patchPricingRow(request.siteId, request.offerId, row);
      await this.invalidateOfferPricingMutation(request.siteId, request.offerId);
    }
  }));

  readonly clearProductPriceMutation = injectMutation<
    void,
    Error,
    Omit<SelectOfferProductPriceRequest, 'retailerId' | 'observationId'>
  >(() => ({
    mutationKey: ['offers', 'pricing', 'selection', 'clear'],
    mutationFn: ({ siteId, offerId, offerProductLineId }) =>
      firstValueFrom(
        this.http.delete<void>(
          buildApiUrl(
            `/sites/${siteId}/offers/${offerId}/pricing/products/${offerProductLineId}/selection`
          )
        )
      ),
    onSuccess: async (_, request) =>
      this.invalidateOfferPricingMutation(request.siteId, request.offerId)
  }));

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

  configureActivityCatalog(
    siteId: string,
    offerId: string,
    searchTerm: string
  ): void {
    const normalizedSearchTerm = searchTerm.trim();
    const current = this.catalogRoute();
    if (
      current.siteId !== siteId ||
      current.offerId !== offerId ||
      current.searchTerm !== normalizedSearchTerm
    ) {
      this.catalogRoute.set({
        siteId,
        offerId,
        searchTerm: normalizedSearchTerm
      });
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

  getActivityCandidate(
    siteId: string,
    offerId: string,
    activityId: string
  ): Promise<OfferActivityCandidate> {
    return firstValueFrom(
      this.http.get<OfferActivityCandidate>(
        buildApiUrl(
          `/sites/${siteId}/offers/${offerId}/activity-catalog/${activityId}`
        )
      )
    );
  }

  addActivity(
    request: AddOfferActivityRequest
  ): Promise<OfferActivityCreatedResponse> {
    return this.addActivityMutation.mutateAsync(request);
  }

  removeActivity(request: RemoveOfferActivityRequest): Promise<void> {
    return this.removeActivityMutation.mutateAsync(request);
  }

  updateActivityMeasurements(
    request: UpdateOfferActivityMeasurementsRequest
  ): Promise<void> {
    return this.updateMeasurementsMutation.mutateAsync(request);
  }

  addPricingRetailer(
    request: AddOfferPricingRetailerRequest
  ): Promise<OfferPricingMatrix> {
    return this.addPricingRetailerMutation.mutateAsync(request);
  }

  removePricingRetailer(request: AddOfferPricingRetailerRequest): Promise<void> {
    return this.removePricingRetailerMutation.mutateAsync(request);
  }

  recordManualPrice(
    request: RecordManualOfferPriceRequest
  ): Promise<OfferRetailerPriceCell> {
    return this.recordManualPriceMutation.mutateAsync(request);
  }

  selectProductPrice(
    request: SelectOfferProductPriceRequest
  ): Promise<OfferPricingProductRow> {
    return this.selectProductPriceMutation.mutateAsync(request);
  }

  clearProductPrice(
    request: Omit<SelectOfferProductPriceRequest, 'retailerId' | 'observationId'>
  ): Promise<void> {
    return this.clearProductPriceMutation.mutateAsync(request);
  }

  getRetailerPriceHistory(
    productId: string,
    retailerId: string,
    pageIndex: number,
    pageSize = 25
  ): Promise<RetailerPriceHistory> {
    return this.queryClient.fetchQuery({
      queryKey: [
        ...this.priceHistoryQueryKey(productId, retailerId),
        pageIndex,
        pageSize
      ] as const,
      queryFn: () =>
        firstValueFrom(
          this.http.get<RetailerPriceHistory>(
            buildApiUrl(
              `/products/${productId}/retailers/${retailerId}/price-history`
            ),
            {
              params: new HttpParams()
                .set('pageIndex', pageIndex)
                .set('pageSize', pageSize)
            }
          )
        )
    });
  }

  private async invalidateSiteOffers(siteId: string): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: ['offers', 'site', siteId]
    });
  }

  private async invalidateOfferWorkspace(
    siteId: string,
    offerId: string
  ): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: ['offers', 'site', siteId, 'detail', offerId]
      }),
      this.queryClient.invalidateQueries({
        queryKey: ['offers', 'site', siteId, 'list']
      }),
      this.queryClient.invalidateQueries({
        queryKey: this.pricingQueryKey(siteId, offerId),
        exact: true
      })
    ]);
  }

  private async invalidateOfferPricingMutation(
    siteId: string,
    offerId: string
  ): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: this.pricingQueryKey(siteId, offerId),
        exact: true
      }),
      this.queryClient.invalidateQueries({
        queryKey: ['offers', 'site', siteId, 'detail', offerId],
        exact: true
      }),
      this.queryClient.invalidateQueries({
        queryKey: ['offers', 'site', siteId, 'list']
      })
    ]);
  }

  private patchPricingCell(
    siteId: string,
    offerId: string,
    cell: OfferRetailerPriceCell
  ): void {
    this.queryClient.setQueryData<OfferPricingMatrix>(
      this.pricingQueryKey(siteId, offerId),
      (matrix) =>
        matrix
          ? {
              ...matrix,
              products: matrix.products.map((row) =>
                row.offerProductLineId === cell.offerProductLineId
                  ? {
                      ...row,
                      retailerPrices: row.retailerPrices.map((current) =>
                        current.retailerId === cell.retailerId ? cell : current
                      )
                    }
                  : row
              )
            }
          : matrix
    );
  }

  private patchPricingRow(
    siteId: string,
    offerId: string,
    row: OfferPricingProductRow
  ): void {
    this.queryClient.setQueryData<OfferPricingMatrix>(
      this.pricingQueryKey(siteId, offerId),
      (matrix) =>
        matrix
          ? {
              ...matrix,
              products: matrix.products.map((current) =>
                current.offerProductLineId === row.offerProductLineId
                  ? row
                  : current
              )
            }
          : matrix
    );
  }

  private pricingQueryKey(siteId: string, offerId: string) {
    return ['offers', 'site', siteId, 'pricing', offerId] as const;
  }

  private priceHistoryQueryKey(productId: string, retailerId: string) {
    return ['retailer-pricing', productId, retailerId, 'history'] as const;
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
