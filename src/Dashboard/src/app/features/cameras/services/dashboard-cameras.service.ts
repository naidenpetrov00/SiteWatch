import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { injectMutation, injectQuery, QueryClient } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DataTableState } from '../../../shared/data-table/data-table.types';
import { CameraRequest, UpdateCameraRequest } from '../models/camera-request.model';
import { DashboardCamera } from '../models/dashboard-camera.model';
import { DashboardCameraDetails } from '../models/dashboard-camera-details.model';
import { DashboardCamerasResponse } from '../models/dashboard-cameras-response.model';

interface DashboardCamerasQueryState {
  pageIndex: number;
  pageSize: number;
  sortActive: string;
  sortDirection: string;
  appliedFilters: Readonly<Record<string, string>>;
}

interface CreateCameraResponse { id: string; }

const DEFAULT_QUERY_STATE: DashboardCamerasQueryState = {
  pageIndex: 0,
  pageSize: 50,
  sortActive: '',
  sortDirection: '',
  appliedFilters: {}
};

@Injectable({ providedIn: 'root' })
export class DashboardCamerasService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly queryState = signal<DashboardCamerasQueryState>(DEFAULT_QUERY_STATE);

  readonly dashboardCamerasQuery = injectQuery<DashboardCamerasResponse>(() => {
    const state = this.queryState();
    return {
      queryKey: ['cameras', 'dashboard', this.queryKeyFromState(state)] as const,
      queryFn: async () => firstValueFrom(this.http.get<DashboardCamerasResponse>(
        buildApiUrl('/dashboard/cameras'), { params: this.buildQueryParams(state) }))
    };
  });

  readonly createCameraMutation = injectMutation<CreateCameraResponse, Error, CameraRequest>(() => ({
    mutationKey: ['cameras', 'create'],
    mutationFn: async (request) => firstValueFrom(this.http.post<CreateCameraResponse>(buildApiUrl('/cameras'), request)),
    onSuccess: async () => this.invalidateDashboardCameras()
  }));

  readonly updateCameraMutation = injectMutation<void, Error, UpdateCameraRequest>(() => ({
    mutationKey: ['cameras', 'update'],
    mutationFn: async ({ id, ...request }) => firstValueFrom(this.http.put<void>(buildApiUrl(`/cameras/${id}`), request)),
    onSuccess: async () => this.invalidateDashboardCameras()
  }));

  readonly deleteCameraMutation = injectMutation<void, Error, string>(() => ({
    mutationKey: ['cameras', 'delete'],
    mutationFn: async (id) => firstValueFrom(this.http.delete<void>(buildApiUrl(`/cameras/${id}`))),
    onSuccess: async () => this.invalidateDashboardCameras()
  }));

  setTableState(state: DataTableState<DashboardCamera>): void {
    const nextState: DashboardCamerasQueryState = {
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

  createCamera(request: CameraRequest): Promise<CreateCameraResponse> { return this.createCameraMutation.mutateAsync(request); }
  updateCamera(request: UpdateCameraRequest): Promise<void> { return this.updateCameraMutation.mutateAsync(request); }
  deleteCamera(id: string): Promise<void> { return this.deleteCameraMutation.mutateAsync(id); }
  getCameraById(id: string): Promise<DashboardCameraDetails> {
    return firstValueFrom(this.http.get<DashboardCameraDetails>(buildApiUrl(`/dashboard/cameras/${id}`)));
  }

  private invalidateDashboardCameras(): Promise<void> {
    return this.queryClient.invalidateQueries({ queryKey: ['cameras', 'dashboard'] });
  }

  private buildQueryParams(state: DashboardCamerasQueryState): HttpParams {
    let params = new HttpParams().set('pageIndex', state.pageIndex).set('pageSize', state.pageSize);
    if (state.sortActive) params = params.set('sortActive', state.sortActive);
    if (state.sortDirection) params = params.set('sortDirection', state.sortDirection);
    for (const [key, value] of Object.entries(this.normalizedFilters(state.appliedFilters))) params = params.set(key, value);
    return params;
  }

  private queryKeyFromState(state: DashboardCamerasQueryState): string {
    return JSON.stringify({
      pageIndex: state.pageIndex,
      pageSize: state.pageSize,
      sortActive: state.sortActive,
      sortDirection: state.sortDirection,
      appliedFilters: Object.entries(this.normalizedFilters(state.appliedFilters)).sort(([a], [b]) => a.localeCompare(b))
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
