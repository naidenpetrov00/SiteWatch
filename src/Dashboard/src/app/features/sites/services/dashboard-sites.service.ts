import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  injectMutation,
  injectQuery,
  QueryClient
} from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { CreateDashboardSiteRequest } from '../models/create-dashboard-site-request.model';
import { DashboardSite } from '../models/dashboard-site.model';
import { DashboardSiteLookup } from '../models/dashboard-site-lookup.model';
import { DashboardSitesResponse } from '../models/dashboard-sites-response.model';
import { UpdateDashboardSiteRequest } from '../models/update-dashboard-site-request.model';

interface DashboardSitesQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

interface CreateDashboardSiteResponse {
  id: string;
}

const DEFAULT_QUERY_STATE: DashboardSitesQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({
  providedIn: 'root'
})
export class DashboardSitesService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardSitesQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardSitesQuery = injectQuery<DashboardSitesResponse>(() => {
    const state = this.queryState();

    return {
      queryKey: ['sites', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () =>
        firstValueFrom(
          this.http.get<DashboardSitesResponse>(buildApiUrl('/dashboard/sites'), {
            params: this.buildQueryParams(state)
          })
        )
    };
  });

  readonly createSiteMutation = injectMutation<
    CreateDashboardSiteResponse,
    Error,
    CreateDashboardSiteRequest
  >(() => ({
    mutationKey: ['sites', 'create'],
    mutationFn: async (request: CreateDashboardSiteRequest) =>
      firstValueFrom(this.http.post<CreateDashboardSiteResponse>(buildApiUrl('/sites'), request)),
    onSuccess: async () => {
      await this.queryClient.invalidateQueries({
        queryKey: ['sites', 'dashboard']
      });
    }
  }));

  readonly updateSiteMutation = injectMutation<void, Error, UpdateDashboardSiteRequest>(() => ({
    mutationKey: ['sites', 'update'],
    mutationFn: async (request: UpdateDashboardSiteRequest) =>
      firstValueFrom(this.http.put<void>(buildApiUrl(`/dashboard/sites/${request.id}`), request)),
    onSuccess: async () => {
      await this.queryClient.invalidateQueries({
        queryKey: ['sites', 'dashboard']
      });
    }
  }));

  setTableState(state: DataTableState<DashboardSite>): void {
    const nextState = this.toQueryState(state);

    if (this.areStatesEqual(this.queryState(), nextState)) {
      return;
    }

    this.queryState.set(nextState);
  }

  getSiteById(siteId: string): Promise<DashboardSite> {
    return firstValueFrom(this.http.get<DashboardSite>(buildApiUrl(`/dashboard/sites/${siteId}`)));
  }

  searchSites(searchTerm: string): Promise<readonly DashboardSiteLookup[]> {
    const normalizedSearchTerm = searchTerm.trim();

    if (normalizedSearchTerm.length === 0) {
      return Promise.resolve([]);
    }

    return firstValueFrom(
      this.http.get<readonly DashboardSiteLookup[]>(buildApiUrl('/dashboard/sites/search'), {
        params: new HttpParams().set('searchTerm', normalizedSearchTerm)
      })
    );
  }

  createSite(request: CreateDashboardSiteRequest): Promise<CreateDashboardSiteResponse> {
    return this.createSiteMutation.mutateAsync(request);
  }

  updateSite(request: UpdateDashboardSiteRequest): Promise<void> {
    return this.updateSiteMutation.mutateAsync(request);
  }

  private toQueryState(state: DataTableState<DashboardSite>): DashboardSitesQueryState {
    return {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
  }

  private buildQueryParams(state: DashboardSitesQueryState): HttpParams {
    let params = new HttpParams()
      .set('pageIndex', state.pageIndex)
      .set('pageSize', state.pageSize);

    if (state.sortActive.length > 0) {
      params = params.set('sortActive', state.sortActive);
    }

    if (state.sortDirection.length > 0) {
      params = params.set('sortDirection', state.sortDirection);
    }

    for (const [key, value] of Object.entries(this.normalizeFilters(state.appliedFilters))) {
      params = params.set(key, value);
    }

    return params;
  }

  private queryKeyFromState(state: DashboardSitesQueryState): string {
    const normalizedFilters = Object.entries(this.normalizeFilters(state.appliedFilters))
      .sort(([leftKey], [rightKey]) => leftKey.localeCompare(rightKey));

    return JSON.stringify({
      pageIndex: state.pageIndex,
      pageSize: state.pageSize,
      sortActive: state.sortActive,
      sortDirection: state.sortDirection,
      appliedFilters: normalizedFilters
    });
  }

  private areStatesEqual(
    leftState: DashboardSitesQueryState,
    rightState: DashboardSitesQueryState
  ): boolean {
    if (
      leftState.pageIndex !== rightState.pageIndex ||
      leftState.pageSize !== rightState.pageSize ||
      leftState.sortActive !== rightState.sortActive ||
      leftState.sortDirection !== rightState.sortDirection
    ) {
      return false;
    }

    const leftFilters = this.normalizeFilters(leftState.appliedFilters);
    const rightFilters = this.normalizeFilters(rightState.appliedFilters);
    const leftKeys = Object.keys(leftFilters);
    const rightKeys = Object.keys(rightFilters);

    if (leftKeys.length !== rightKeys.length) {
      return false;
    }

    return leftKeys.every((key) => leftFilters[key] === rightFilters[key]);
  }

  private normalizeFilters(
    filters: Readonly<Record<string, string>>
  ): Record<string, string> {
    return Object.entries(filters).reduce<Record<string, string>>(
      (normalizedFilters, [key, value]) => {
        const normalizedValue = value.trim().toLowerCase();

        if (normalizedValue.length > 0) {
          normalizedFilters[key] = normalizedValue;
        }

        return normalizedFilters;
      },
      {}
    );
  }
}
