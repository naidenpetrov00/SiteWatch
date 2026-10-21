import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { DashboardSitesService } from '../../sites/services/dashboard-sites.service';
import { DashboardUsersService } from '../../users/services/dashboard-users.service';
import { AssetActionRequest, AssetDetails, AssetDetailsRequest, AssetEvent, AssetListItem, PagedResponse, RegisterAssetsRequest, RegisteredAsset, SiteLookup, UserLookup } from '../models/asset-tracking.models';

@Injectable({ providedIn: 'root' })
export class AssetTrackingService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly sites = inject(DashboardSitesService);
  private readonly users = inject(DashboardUsersService);

  getAssets(state: DataTableState<AssetListItem>, reviewStatus?: string): Promise<PagedResponse<AssetListItem>> { return firstValueFrom(this.http.get<PagedResponse<AssetListItem>>(buildApiUrl('/assets'), { params: this.tableParams(state, reviewStatus ? { reviewStatus } : {}, 'assetCode') })); }
  getAsset(id: string): Promise<AssetDetails> { return firstValueFrom(this.http.get<AssetDetails>(buildApiUrl(`/assets/${id}`))); }
  getEvents(id: string): Promise<PagedResponse<AssetEvent>> { return firstValueFrom(this.http.get<PagedResponse<AssetEvent>>(buildApiUrl(`/assets/${id}/events`), { params: new HttpParams().set('pageSize', 100) })); }
  register(request: RegisterAssetsRequest): Promise<readonly RegisteredAsset[]> { return firstValueFrom(this.http.post<readonly RegisteredAsset[]>(buildApiUrl('/assets'), request)); }
  updateDetails(id: string, request: AssetDetailsRequest): Promise<AssetDetails> { return firstValueFrom(this.http.put<AssetDetails>(buildApiUrl(`/assets/${id}/details`), request)); }
  mutate(id: string, action: string, request: AssetActionRequest): Promise<AssetDetails> { return firstValueFrom(this.http.patch<AssetDetails>(buildApiUrl(`/assets/${id}/${action}`), request)); }

  searchSites(term: string): Promise<readonly SiteLookup[]> { return this.sites.searchSites(term); }
  searchEligibleResponsibleUsers(term: string): Promise<readonly UserLookup[]> {
    return this.users.searchUsersByRoles(term, ['Administrator', 'Worker']);
  }
  async invalidate(id?: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({ queryKey: ['asset-tracking'] }),
      id ? this.queryClient.invalidateQueries({ queryKey: ['asset-tracking', 'asset', id] }) : Promise.resolve()
    ]);
  }
  isConcurrencyConflict(error: unknown): boolean { return error instanceof HttpErrorResponse && error.status === 409 && (error.error as { code?: string } | null)?.code === 'asset_concurrency_conflict'; }
  errorMessage(error: unknown, resource: string): string {
    if (!(error instanceof HttpErrorResponse)) return `${resource} could not be loaded because of an unexpected error.`;
    if (error.status === 403) return `You do not have administrator access to ${resource}.`;
    if (error.status === 404) return `${resource} was not found.`;
    if (error.status === 0) return `${resource} could not be loaded because the API is unavailable.`;
    return `${resource} could not be loaded. Try again.`;
  }

  private tableParams<T extends object>(state: DataTableState<T>, extra: Readonly<Record<string, string>>, searchFilterKey: string): HttpParams {
    let params = new HttpParams().set('pageIndex', state.page.pageIndex).set('pageSize', state.page.pageSize);
    if (state.sort.active) params = params.set('sortActive', state.sort.active);
    if (state.sort.direction) params = params.set('sortDirection', state.sort.direction);
    for (const [key, value] of Object.entries({ ...state.appliedFilters, ...extra })) if (value.trim()) params = params.set(key === searchFilterKey ? 'search' : key, value.trim());
    return params;
  }
}
