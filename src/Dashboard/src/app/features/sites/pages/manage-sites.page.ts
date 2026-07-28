import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { DashboardSite } from '../models/dashboard-site.model';
import { DashboardSitesService } from '../services/dashboard-sites.service';
import { EditSiteDialogComponent } from '../components/edit-site-dialog/edit-site-dialog.component';
import { AddSiteDialogComponent } from '../components/add-site-dialog/add-site-dialog.component';

const SITE_COLUMNS: readonly DataTableColumn<DashboardSite>[] = [
  {
    key: 'numberId',
    label: 'NumberId',
    sortable: true,
    cellType: 'button',
    filter: { kind: 'number', placeholder: 'Filter NumberId' }
  },
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'name',
    label: 'Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Name' }
  },
  {
    key: 'address',
    label: 'Address',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Address' }
  },
  {
    key: 'mediaPolicy',
    label: 'Media Policy'
  }
] as const;

@Component({
  selector: 'app-manage-sites-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './manage-sites.page.html',
  styleUrl: './manage-sites.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageSitesPage {
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dialog = inject(MatDialog);

  readonly sites = signal<readonly DashboardSite[]>([]);
  readonly sitesFilteredCount = signal(0);
  readonly sitesTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardSite> | null>(null);
  readonly columns = SITE_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const tableState = this.tableState();

      if (!tableState) {
        return;
      }

      this.dashboardSitesService.setTableState(tableState);
    });

    effect(() => {
      const dashboardSites = this.dashboardSitesService.dashboardSitesQuery.data();

      if (!dashboardSites) {
        return;
      }

      this.sites.set(dashboardSites.items);
      this.sitesFilteredCount.set(dashboardSites.filteredCount);
      this.sitesTotalCount.set(dashboardSites.totalCount);
    });
  }

  onTableStateChange(state: DataTableState<DashboardSite>): void {
    this.tableState.set(state);
  }

  openAddSiteDialog(): void {
    this.dialog.open(AddSiteDialogComponent, {
      autoFocus: false,
      width: '42rem',
      maxWidth: 'calc(100vw - 2rem)'
    });
  }

  async onCellButtonClicked(event: { row: DashboardSite; column: DataTableColumn<DashboardSite> }): Promise<void> {
    if (event.column.key !== 'numberId') {
      return;
    }

    try {
      const site = await this.dashboardSitesService.getSiteById(event.row.id);

      this.dialog.open(EditSiteDialogComponent, {
        autoFocus: false,
        width: '42rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: site
      });
    } catch {
      // Keep the table usable if the detail fetch fails.
    }
  }
}
