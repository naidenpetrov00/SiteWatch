import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { DashboardIssue } from '../models/dashboard-issue.model';
import { DashboardIssuesResponse } from '../models/dashboard-issues-response.model';
import { IssueRequest, UpdateIssueRequest } from '../models/issue-request.model';

interface DashboardIssuesQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

interface CreateIssueResponse { id: string; }

const DEFAULT_QUERY_STATE: DashboardIssuesQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({ providedIn: 'root' })
export class DashboardIssuesService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardIssuesQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardIssuesQuery = injectQuery<DashboardIssuesResponse>(() => {
    const state = this.queryState();
    return {
      queryKey: ['issues', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () => firstValueFrom(this.http.get<DashboardIssuesResponse>(
        buildApiUrl('/dashboard/issues'), { params: this.buildQueryParams(state) }))
    };
  });

  readonly createIssueMutation = injectMutation<CreateIssueResponse, Error, IssueRequest>(() => ({
    mutationKey: ['issues', 'create'],
    mutationFn: async (request) => firstValueFrom(
      this.http.post<CreateIssueResponse>(buildApiUrl('/issues'), request)),
    onSuccess: async () => this.invalidateDashboardIssues()
  }));

  readonly updateIssueMutation = injectMutation<void, Error, UpdateIssueRequest>(() => ({
    mutationKey: ['issues', 'update'],
    mutationFn: async ({ id, ...request }) => firstValueFrom(
      this.http.put<void>(buildApiUrl(`/issues/${id}`), request)),
    onSuccess: async () => this.invalidateDashboardIssues()
  }));

  setTableState(state: DataTableState<DashboardIssue>): void {
    const nextState: DashboardIssuesQueryState = {
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

  getIssueById(id: string): Promise<DashboardIssue> {
    return firstValueFrom(this.http.get<DashboardIssue>(buildApiUrl(`/issues/${id}`)));
  }

  createIssue(request: IssueRequest): Promise<CreateIssueResponse> {
    return this.createIssueMutation.mutateAsync(request);
  }

  updateIssue(request: UpdateIssueRequest): Promise<void> {
    return this.updateIssueMutation.mutateAsync(request);
  }

  private invalidateDashboardIssues(): Promise<void> {
    return this.queryClient.invalidateQueries({ queryKey: ['issues', 'dashboard'] });
  }

  private buildQueryParams(state: DashboardIssuesQueryState): HttpParams {
    let params = new HttpParams().set('pageIndex', state.pageIndex).set('pageSize', state.pageSize);
    if (state.sortActive) params = params.set('sortActive', state.sortActive);
    if (state.sortDirection) params = params.set('sortDirection', state.sortDirection);
    for (const [key, value] of Object.entries(this.normalizedFilters(state.appliedFilters))) {
      params = params.set(key, value);
    }
    return params;
  }

  private queryKeyFromState(state: DashboardIssuesQueryState): string {
    return JSON.stringify({
      pageIndex: state.pageIndex,
      pageSize: state.pageSize,
      sortActive: state.sortActive,
      sortDirection: state.sortDirection,
      appliedFilters: Object.entries(this.normalizedFilters(state.appliedFilters))
        .sort(([left], [right]) => left.localeCompare(right))
    });
  }

  private normalizedFilters(filters: Readonly<Record<string, string>>): Record<string, string> {
    return Object.entries(filters).reduce<Record<string, string>>((result, [key, value]) => {
      const normalized = value.trim().toLowerCase();
      if (normalized) result[key] = normalized;
      return result;
    }, {});
  }
}
