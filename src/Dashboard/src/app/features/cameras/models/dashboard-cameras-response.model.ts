import { DashboardCamera } from './dashboard-camera.model';

export interface DashboardCamerasResponse {
  items: readonly DashboardCamera[];
  filteredCount: number;
  totalCount: number;
}
