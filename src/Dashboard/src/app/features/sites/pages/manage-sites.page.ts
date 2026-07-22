import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { DashboardSite } from '../models/dashboard-site.model';
import { DashboardSitesService } from '../services/dashboard-sites.service';

const SITE_COLUMNS: readonly DataTableColumn<DashboardSite>[] = [
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
  imports: [DataTableComponent],
  templateUrl: './manage-sites.page.html',
  styleUrl: './manage-sites.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageSitesPage {
  private readonly dashboardSitesService = inject(DashboardSitesService);

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
}
