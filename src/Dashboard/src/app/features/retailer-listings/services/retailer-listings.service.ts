import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  RetailerListing,
  RetailerListingQueryState,
  RetailerListingsResponse,
  RetailerPriceHistory,
  SaveRetailerListingRequest,
  UpdateRetailerListingRequest
} from '../models/retailer-listing.models';

export const retailerListingKeys = {
  companies: () => ['retailer-listings', 'company'] as const,
  product: (productId: string, state?: RetailerListingQueryState) =>
    state
      ? (['retailer-listings', 'product', productId, state] as const)
      : (['retailer-listings', 'product', productId] as const),
  retailer: (retailerId: string, state?: RetailerListingQueryState) =>
    state
      ? (['retailer-listings', 'retailer', retailerId, state] as const)
      : (['retailer-listings', 'retailer', retailerId] as const),
  company: (companyPersonId: string, state?: RetailerListingQueryState) =>
    state
      ? (['retailer-listings', 'company', companyPersonId, state] as const)
      : (['retailer-listings', 'company', companyPersonId] as const),
  detail: (listingId: string) =>
    ['retailer-listings', 'detail', listingId] as const,
  history: (
    productId: string,
    retailerId: string,
    pageIndex?: number,
    pageSize?: number
  ) =>
    pageIndex === undefined
      ? (['retailer-pricing', productId, retailerId, 'history'] as const)
      : ([
          'retailer-pricing',
          productId,
          retailerId,
          'history',
          pageIndex,
          pageSize
        ] as const)
};

@Injectable({ providedIn: 'root' })
export class RetailerListingsService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);

  getForProduct(
    productId: string,
    state: RetailerListingQueryState
  ): Promise<RetailerListingsResponse> {
    return firstValueFrom(
      this.http.get<RetailerListingsResponse>(
        buildApiUrl('/products/' + productId + '/retailer-listings'),
        { params: this.buildParams(state) }
      )
    );
  }

  getForRetailer(
    retailerId: string,
    state: RetailerListingQueryState
  ): Promise<RetailerListingsResponse> {
    return firstValueFrom(
      this.http.get<RetailerListingsResponse>(
        buildApiUrl('/retailers/' + retailerId + '/product-listings'),
        { params: this.buildParams(state) }
      )
    );
  }

  getForCompany(
    companyPersonId: string,
    state: RetailerListingQueryState
  ): Promise<RetailerListingsResponse> {
    return firstValueFrom(
      this.http.get<RetailerListingsResponse>(
        buildApiUrl('/persons/' + companyPersonId + '/retailer-listings'),
        { params: this.buildParams(state) }
      )
    );
  }

  getById(listingId: string): Promise<RetailerListing> {
    return firstValueFrom(
      this.http.get<RetailerListing>(
        buildApiUrl('/retailer-listings/' + listingId)
      )
    );
  }

  getHistory(
    productId: string,
    retailerId: string,
    pageIndex: number,
    pageSize: number
  ): Promise<RetailerPriceHistory> {
    return this.queryClient.fetchQuery({
      queryKey: retailerListingKeys.history(
        productId,
        retailerId,
        pageIndex,
        pageSize
      ),
      queryFn: () =>
        firstValueFrom(
          this.http.get<RetailerPriceHistory>(
            buildApiUrl(
              '/products/' + productId + '/retailers/' + retailerId + '/price-history'
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

  async create(request: SaveRetailerListingRequest): Promise<RetailerListing> {
    const listing = await firstValueFrom(
      this.http.post<RetailerListing>(buildApiUrl('/retailer-listings'), request)
    );
    await this.afterMutation(listing);
    return listing;
  }

  async update(request: UpdateRetailerListingRequest): Promise<RetailerListing> {
    const { listingId, ...body } = request;
    const listing = await firstValueFrom(
      this.http.put<RetailerListing>(
        buildApiUrl('/retailer-listings/' + listingId),
        body
      )
    );
    await this.afterMutation(listing);
    return listing;
  }

  async setActive(
    listing: RetailerListing,
    isActive: boolean
  ): Promise<RetailerListing> {
    const updated = await firstValueFrom(
      this.http.patch<RetailerListing>(
        buildApiUrl(
          '/retailer-listings/' +
            listing.id +
            '/' +
            (isActive ? 'activate' : 'deactivate')
        ),
        {}
      )
    );
    await this.afterMutation(updated);
    return updated;
  }

  async invalidatePair(productId: string, retailerId: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.product(productId)
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.retailer(retailerId)
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.history(productId, retailerId)
      }),
      this.queryClient.invalidateQueries({
        predicate: (query) =>
          this.isAffectedOfferPricingQuery(
            query.queryKey,
            query.state.data,
            productId,
            retailerId
          )
      })
    ]);
  }

  async invalidateRetailerLifecycle(
    retailerId: string,
    companyPersonIds?: readonly string[]
  ): Promise<void> {
    const companyInvalidations = companyPersonIds?.length
      ? companyPersonIds.map((companyPersonId) =>
          this.invalidateCompany(companyPersonId)
        )
      : [
          this.queryClient.invalidateQueries({
            queryKey: retailerListingKeys.companies()
          })
        ];
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.retailer(retailerId)
      }),
      this.queryClient.invalidateQueries({
        predicate: (query) => {
          if (
            query.queryKey[0] !== 'retailer-listings' ||
            query.queryKey[1] !== 'product'
          ) return false;
          const response = query.state.data as RetailerListingsResponse | undefined;
          return response?.items.some(
            (listing) => listing.retailerId === retailerId
          ) ?? false;
        }
      }),
      this.queryClient.invalidateQueries({
        predicate: (query) => {
          if (query.queryKey[0] !== 'offers' || !query.queryKey.includes('pricing')) {
            return false;
          }
          const matrix = query.state.data as {
            retailers?: readonly { retailerId?: string }[];
          } | undefined;
          return matrix?.retailers?.some(
            (retailer) => retailer.retailerId === retailerId
          ) ?? false;
        }
      }),
      ...companyInvalidations
    ]);
  }

  async invalidateProductLifecycle(productId: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.product(productId)
      }),
      this.queryClient.invalidateQueries({
        predicate: (query) => {
          if (
            query.queryKey[0] !== 'retailer-listings' ||
            query.queryKey[1] !== 'retailer'
          ) return false;
          const response = query.state.data as RetailerListingsResponse | undefined;
          return response?.items.some(
            (listing) => listing.productId === productId
          ) ?? false;
        }
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.companies()
      })
    ]);
  }

  async invalidateCompany(companyPersonId: string): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: retailerListingKeys.company(companyPersonId)
    });
  }

  private async afterMutation(listing: RetailerListing): Promise<void> {
    this.queryClient.setQueryData(retailerListingKeys.detail(listing.id), listing);
    this.patchListingPages(retailerListingKeys.product(listing.productId), listing);
    this.patchListingPages(retailerListingKeys.retailer(listing.retailerId), listing);
    await Promise.all([
      this.invalidatePair(listing.productId, listing.retailerId),
      this.queryClient.invalidateQueries({
        queryKey: retailerListingKeys.companies()
      })
    ]);
  }

  private patchListingPages(
    queryKey: readonly unknown[],
    listing: RetailerListing
  ): void {
    this.queryClient.setQueriesData<RetailerListingsResponse>(
      { queryKey },
      (current) =>
        current
          ? {
              ...current,
              items: current.items.map((item) =>
                item.id === listing.id ? listing : item
              )
            }
          : current
    );
  }

  private buildParams(state: RetailerListingQueryState): HttpParams {
    let params = new HttpParams()
      .set('pageIndex', state.pageIndex)
      .set('pageSize', state.pageSize)
      .set('includeInactive', state.includeInactive);
    if (state.isActive !== null) {
      params = params.set('isActive', state.isActive);
    }
    if (state.sortActive) params = params.set('sortActive', state.sortActive);
    if (state.sortDirection) {
      params = params.set('sortDirection', state.sortDirection);
    }
    if (state.searchTerm.trim()) {
      params = params.set('searchTerm', state.searchTerm.trim());
    }
    return params;
  }

  private isAffectedOfferPricingQuery(
    queryKey: readonly unknown[],
    data: unknown,
    productId: string,
    retailerId: string
  ): boolean {
    if (queryKey[0] !== 'offers' || !queryKey.includes('pricing')) return false;
    const matrix = data as {
      retailers?: readonly { retailerId?: string }[];
      products?: readonly { productId?: string }[];
    } | undefined;
    if (!matrix?.retailers || !matrix.products) return true;
    return (
      matrix.retailers.some((item) => item.retailerId === retailerId) &&
      matrix.products.some((item) => item.productId === productId)
    );
  }
}

export function getRetailerListingError(
  error: unknown,
  fallback = 'The retailer listing could not be saved.'
): string {
  if (!(error instanceof HttpErrorResponse)) return fallback;
  const detail = error.error?.detail;
  if (typeof detail === 'string' && detail.trim()) return detail;
  const validationErrors = error.error?.errors as
    | Record<string, string[]>
    | undefined;
  const first = validationErrors
    ? Object.values(validationErrors).flat().find(Boolean)
    : null;
  if (first) return first;
  const validationDetails = error.error?.details as
    | readonly { message?: string }[]
    | undefined;
  const detailMessage = validationDetails?.find(
    (item) => typeof item.message === 'string' && item.message.trim()
  )?.message;
  if (detailMessage) return detailMessage;
  const title = error.error?.title;
  return typeof title === 'string' && title.trim() ? title : fallback;
}
