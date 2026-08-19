import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { AddCameraDialogComponent } from '../components/add-camera-dialog/add-camera-dialog.component';
import { DeleteCameraDialogComponent } from '../components/delete-camera-dialog/delete-camera-dialog.component';
import { EditCameraDialogComponent } from '../components/edit-camera-dialog/edit-camera-dialog.component';
import { DashboardCamera } from '../models/dashboard-camera.model';
import { DashboardCamerasService } from '../services/dashboard-cameras.service';

const CAMERA_COLUMNS: readonly DataTableColumn<DashboardCamera>[] = [
  { key: 'numberId', label: 'NumberId', sortable: true, cellType: 'button', filter: { kind: 'number', placeholder: 'Filter NumberId' } },
  { key: 'id', label: 'Id', sortable: true, filter: { kind: 'text', placeholder: 'Filter Id' } },
  { key: 'name', label: 'Name', sortable: true, filter: { kind: 'text', placeholder: 'Filter Name' } },
  { key: 'brand', label: 'Brand', sortable: true, filter: { kind: 'text', placeholder: 'Filter Brand' } },
  { key: 'model', label: 'Model', sortable: true, filter: { kind: 'text', placeholder: 'Filter Model' } },
  { key: 'ipAddress', label: 'IP Address', sortable: true, filter: { kind: 'text', placeholder: 'Filter IP Address' } },
  { key: 'rtspPort', label: 'RTSP Port' },
  { key: 'ptzPort', label: 'PTZ Port' },
  { key: 'siteName', label: 'Site', sortable: true, filter: { kind: 'text', placeholder: 'Filter Site' } },
  { key: 'junk', label: 'Junk', cellType: 'button', sortable: false, valueAccessor: () => 'Junk' }
] as const;

@Component({
  selector: 'app-manage-cameras-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './manage-cameras.page.html',
  styleUrl: './manage-cameras.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageCamerasPage {
  private readonly dashboardCamerasService = inject(DashboardCamerasService);
  private readonly dialog = inject(MatDialog);

  readonly cameras = signal<readonly DashboardCamera[]>([]);
  readonly camerasFilteredCount = signal(0);
  readonly camerasTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardCamera> | null>(null);
  readonly columns = CAMERA_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const state = this.tableState();
      if (state) this.dashboardCamerasService.setTableState(state);
    });
    effect(() => {
      const response = this.dashboardCamerasService.dashboardCamerasQuery.data();
      if (!response) return;
      this.cameras.set(response.items);
      this.camerasFilteredCount.set(response.filteredCount);
      this.camerasTotalCount.set(response.totalCount);
    });
  }

  onTableStateChange(state: DataTableState<DashboardCamera>): void { this.tableState.set(state); }
  openAddCameraDialog(): void {
    this.dialog.open(AddCameraDialogComponent, { autoFocus: false, width: '48rem', maxWidth: 'calc(100vw - 2rem)' });
  }

  async onCellButtonClicked(event: { row: DashboardCamera; column: DataTableColumn<DashboardCamera> }): Promise<void> {
    if (event.column.key === 'numberId') {
      try {
        const camera = await this.dashboardCamerasService.getCameraById(event.row.id);
        this.dialog.open(EditCameraDialogComponent, { autoFocus: false, width: '48rem', maxWidth: 'calc(100vw - 2rem)', data: camera });
      } catch { /* Keep the table usable if loading details fails. */ }
      return;
    }
    if (event.column.key !== 'junk') return;

    const confirmed = await firstValueFrom(this.dialog.open(DeleteCameraDialogComponent, {
      autoFocus: false,
      width: '28rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { name: event.row.name, numberId: event.row.numberId }
    }).afterClosed());
    if (confirmed !== true) return;
    try { await this.dashboardCamerasService.deleteCamera(event.row.id); } catch { /* Keep the row visible on failure. */ }
  }
}
