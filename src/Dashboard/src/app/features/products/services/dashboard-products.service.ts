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
  CreateDashboardProductRequest,
  DashboardProduct,
  DashboardProductDetails,
  DashboardProductLookup,
  DashboardProductsResponse,
  UpdateDashboardProductRequest
} from '../models/dashboard-product.models';

interface CreateDashboardProductResponse {
  id: string;
}

interface DashboardProductsQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

const DEFAULT_QUERY_STATE: DashboardProductsQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({ providedIn: 'root' })
export class DashboardProductsService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardProductsQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardProductsQuery = injectQuery<DashboardProductsResponse>(() => {
    const state = this.queryState();

    return {
      queryKey: ['products', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () =>
        firstValueFrom(
          this.http.get<DashboardProductsResponse>(
            buildApiUrl('/dashboard/products'),
            { params: this.buildQueryParams(state) }
          )
        )
    };
  });

  readonly createProductMutation = injectMutation<
    CreateDashboardProductResponse,
    Error,
    CreateDashboardProductRequest
  >(() => ({
    mutationKey: ['products', 'create'],
    mutationFn: async (request) =>
      firstValueFrom(
        this.http.post<CreateDashboardProductResponse>(
          buildApiUrl('/products'),
          request
        )
      ),
    onSuccess: async () => this.invalidateProductLists()
  }));

  readonly updateProductMutation = injectMutation<
    void,
    Error,
    UpdateDashboardProductRequest
  >(() => ({
    mutationKey: ['products', 'update'],
    mutationFn: async (request) =>
      firstValueFrom(
        this.http.put<void>(buildApiUrl(`/products/${request.id}`), request)
      ),
    onSuccess: async () => this.invalidateProductLists()
  }));

  setTableState(state: DataTableState<DashboardProduct>): void {
    const nextState = this.toQueryState(state);
    if (!this.areStatesEqual(this.queryState(), nextState)) {
      this.queryState.set(nextState);
    }
  }

  createProduct(
    request: CreateDashboardProductRequest
  ): Promise<CreateDashboardProductResponse> {
    return this.createProductMutation.mutateAsync(request);
  }

  updateProduct(request: UpdateDashboardProductRequest): Promise<void> {
    return this.updateProductMutation.mutateAsync(request);
  }

  getProductById(productId: string): Promise<DashboardProductDetails> {
    return firstValueFrom(
      this.http.get<DashboardProductDetails>(buildApiUrl(`/products/${productId}`))
    );
  }

  searchProducts(searchTerm: string): Promise<readonly DashboardProductLookup[]> {
    const normalizedSearchTerm = searchTerm.trim();
    if (!normalizedSearchTerm) {
      return Promise.resolve([]);
    }

    return firstValueFrom(
      this.http.get<readonly DashboardProductLookup[]>(
        buildApiUrl('/dashboard/products/search'),
        { params: new HttpParams().set('searchTerm', normalizedSearchTerm) }
      )
    );
  }

  private async invalidateProductLists(): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: ['products', 'dashboard']
    });
  }

  private toQueryState(
    state: DataTableState<DashboardProduct>
  ): DashboardProductsQueryState {
    return {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
  }

  private buildQueryParams(state: DashboardProductsQueryState): HttpParams {
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

  private queryKeyFromState(state: DashboardProductsQueryState): string {
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

  private areStatesEqual(
    left: DashboardProductsQueryState,
    right: DashboardProductsQueryState
  ): boolean {
    return this.queryKeyFromState(left) === this.queryKeyFromState(right);
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
