import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { CreateDashboardInvoiceRequest } from '../models/create-dashboard-invoice-request.model';
import { DashboardInvoice } from '../models/dashboard-invoice.model';
import { DashboardInvoicesResponse } from '../models/dashboard-invoices-response.model';
import { UpdateInvoiceSiteAllocationsRequest } from '../models/invoice-site-allocation.model';

interface CreateDashboardInvoiceResponse {
  id: string;
}

interface DashboardInvoicesQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

const DEFAULT_QUERY_STATE: DashboardInvoicesQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({
  providedIn: 'root'
})
export class DashboardInvoicesService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardInvoicesQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardInvoicesQuery = injectQuery<DashboardInvoicesResponse>(() => {
    const state = this.queryState();

    return {
      queryKey: ['invoices', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () =>
        firstValueFrom(
          this.http.get<DashboardInvoicesResponse>(buildApiUrl('/dashboard/invoices'), {
            params: this.buildQueryParams(state)
          })
        )
    };
  });

  readonly createInvoiceMutation = injectMutation<
    CreateDashboardInvoiceResponse,
    Error,
    CreateDashboardInvoiceRequest
  >(() => ({
    mutationKey: ['invoices', 'create'],
    mutationFn: async (request: CreateDashboardInvoiceRequest) =>
      firstValueFrom(this.http.post<CreateDashboardInvoiceResponse>(buildApiUrl('/invoices'), request)),
    onSuccess: async () => {
      await this.queryClient.invalidateQueries({
        queryKey: ['invoices', 'dashboard']
      });
    }
  }));

  readonly updateSiteAllocationsMutation = injectMutation<
    void,
    Error,
    UpdateInvoiceSiteAllocationsRequest
  >(() => ({
    mutationKey: ['invoices', 'site-allocations', 'update'],
    mutationFn: async (request: UpdateInvoiceSiteAllocationsRequest) =>
      firstValueFrom(
        this.http.put<void>(buildApiUrl(`/invoices/${request.invoiceId}/site-allocations`), {
          siteAllocations: request.siteAllocations
        })
      ),
    onSuccess: async () => {
      await this.queryClient.invalidateQueries({
        queryKey: ['invoices', 'dashboard']
      });
    }
  }));

  setTableState(state: DataTableState<DashboardInvoice>): void {
    const nextState = this.toQueryState(state);

    if (this.areStatesEqual(this.queryState(), nextState)) {
      return;
    }

    this.queryState.set(nextState);
  }

  createInvoice(request: CreateDashboardInvoiceRequest): Promise<CreateDashboardInvoiceResponse> {
    return this.createInvoiceMutation.mutateAsync(request);
  }

  updateSiteAllocations(request: UpdateInvoiceSiteAllocationsRequest): Promise<void> {
    return this.updateSiteAllocationsMutation.mutateAsync(request);
  }

  private toQueryState(state: DataTableState<DashboardInvoice>): DashboardInvoicesQueryState {
    return {
      pageIndex: state.page.pageIndex,
      pageSize: state.page.pageSize,
      sortActive: state.sort.active,
      sortDirection: state.sort.direction,
      appliedFilters: { ...state.appliedFilters }
    };
  }

  private buildQueryParams(state: DashboardInvoicesQueryState): HttpParams {
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

  private queryKeyFromState(state: DashboardInvoicesQueryState): string {
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
    leftState: DashboardInvoicesQueryState,
    rightState: DashboardInvoicesQueryState
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
