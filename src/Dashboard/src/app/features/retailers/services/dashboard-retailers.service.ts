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
  CreateDashboardRetailerRequest,
  DashboardRetailer,
  DashboardRetailerDetails,
  DashboardRetailerLookup,
  DashboardRetailersResponse,
  UpdateDashboardRetailerRequest
} from '../models/dashboard-retailer.models';

interface CreateDashboardRetailerResponse {
  id: string;
}

interface DashboardRetailersQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

interface SetRetailerStatusRequest {
  id: string;
  isActive: boolean;
}

const DEFAULT_QUERY_STATE: DashboardRetailersQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({ providedIn: 'root' })
export class DashboardRetailersService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardRetailersQueryState>(
    DEFAULT_QUERY_STATE
  );

  readonly dashboardRetailersQuery = injectQuery<DashboardRetailersResponse>(() => {
    const state = this.queryState();
    return {
      queryKey: ['retailers', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () =>
        firstValueFrom(
          this.http.get<DashboardRetailersResponse>(
            buildApiUrl('/dashboard/retailers'),
            { params: this.buildQueryParams(state) }
          )
        )
    };
  });

  readonly createRetailerMutation = injectMutation<
    CreateDashboardRetailerResponse,
    Error,
    CreateDashboardRetailerRequest
  >(() => ({
    mutationKey: ['retailers', 'create'],
    mutationFn: async (request) =>
      firstValueFrom(
        this.http.post<CreateDashboardRetailerResponse>(
          buildApiUrl('/retailers'),
          request
        )
      ),
    onSuccess: async () => this.invalidateRetailerLists()
  }));

  readonly updateRetailerMutation = injectMutation<
    void,
    Error,
    UpdateDashboardRetailerRequest
  >(() => ({
    mutationKey: ['retailers', 'update'],
    mutationFn: async ({ id, ...request }) =>
      firstValueFrom(this.http.put<void>(buildApiUrl(`/retailers/${id}`), request)),
    onSuccess: async () => this.invalidateRetailerLists()
  }));

  readonly setRetailerStatusMutation = injectMutation<
    void,
    Error,
    SetRetailerStatusRequest
  >(() => ({
    mutationKey: ['retailers', 'set-status'],
    mutationFn: async ({ id, isActive }) =>
      firstValueFrom(
        this.http.patch<void>(
          buildApiUrl(`/retailers/${id}/${isActive ? 'activate' : 'deactivate'}`),
          {}
        )
      ),
    onSuccess: async () => this.invalidateRetailerLists()
  }));

  setTableState(state: DataTableState<DashboardRetailer>): void {
    const nextState = this.toQueryState(state);
    if (this.queryKeyFromState(this.queryState()) !== this.queryKeyFromState(nextState)) {
      this.queryState.set(nextState);
    }
  }

  createRetailer(
    request: CreateDashboardRetailerRequest
  ): Promise<CreateDashboardRetailerResponse> {
    return this.createRetailerMutation.mutateAsync(request);
  }

  updateRetailer(request: UpdateDashboardRetailerRequest): Promise<void> {
    return this.updateRetailerMutation.mutateAsync(request);
  }

  setRetailerActive(id: string, isActive: boolean): Promise<void> {
    return this.setRetailerStatusMutation.mutateAsync({ id, isActive });
  }

  getRetailerById(retailerId: string): Promise<DashboardRetailerDetails> {
    return firstValueFrom(
      this.http.get<DashboardRetailerDetails>(
        buildApiUrl(`/retailers/${retailerId}`)
      )
    );
  }

  searchRetailers(searchTerm: string): Promise<readonly DashboardRetailerLookup[]> {
    const normalizedSearchTerm = searchTerm.trim();
    if (!normalizedSearchTerm) {
      return Promise.resolve([]);
    }

    return firstValueFrom(
      this.http.get<readonly DashboardRetailerLookup[]>(
        buildApiUrl('/dashboard/retailers/search'),
        { params: new HttpParams().set('searchTerm', normalizedSearchTerm) }
      )
    );
  }

  private async invalidateRetailerLists(): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: ['retailers', 'dashboard']
    });
  }

  private toQueryState(
    state: DataTableState<DashboardRetailer>
  ): DashboardRetailersQueryState {
    return {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
  }

  private buildQueryParams(state: DashboardRetailersQueryState): HttpParams {
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

  private queryKeyFromState(state: DashboardRetailersQueryState): string {
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
