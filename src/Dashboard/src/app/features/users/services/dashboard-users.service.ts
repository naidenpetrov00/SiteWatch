import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { injectQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { DashboardUser } from '../models/dashboard-user.model';
import { DashboardUsersResponse } from '../models/dashboard-users-response.model';

interface DashboardUsersQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

const DEFAULT_QUERY_STATE: DashboardUsersQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({
  providedIn: 'root'
})
export class DashboardUsersService {
  private readonly http = inject(HttpClient);
  private readonly queryState = signal<DashboardUsersQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardUsersQuery = injectQuery<DashboardUsersResponse>(() => {
    const state = this.queryState();

    return {
      queryKey: ['users', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () =>
        firstValueFrom(
          this.http.get<DashboardUsersResponse>(buildApiUrl('/dashboard/users'), {
            params: this.buildQueryParams(state)
          })
        )
    };
  });

  setTableState(state: DataTableState<DashboardUser>): void {
    const nextState = this.toQueryState(state);

    if (this.areStatesEqual(this.queryState(), nextState)) {
      return;
    }

    this.queryState.set(nextState);
  }

  private toQueryState(state: DataTableState<DashboardUser>): DashboardUsersQueryState {
    return {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
  }

  private buildQueryParams(state: DashboardUsersQueryState): HttpParams {
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

  private queryKeyFromState(state: DashboardUsersQueryState): string {
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
    leftState: DashboardUsersQueryState,
    rightState: DashboardUsersQueryState
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
