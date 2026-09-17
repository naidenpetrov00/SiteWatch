import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableRowAction,
  DataTableRowActionEvent,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { OffersService } from '../../offers/services/offers.service';
import { getOfferError } from '../../offers/utils/offer-error';
import { DashboardSite } from '../models/dashboard-site.model';
import { DashboardSitesService } from '../services/dashboard-sites.service';
import { EditSiteDialogComponent } from '../components/edit-site-dialog/edit-site-dialog.component';
import { AddSiteDialogComponent } from '../components/add-site-dialog/add-site-dialog.component';
import { formatMediaPolicyPreset } from '../models/site-media-policy-presets';

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
    key: 'managerDisplayName',
    label: 'Site Manager'
  },
  {
    key: 'startDate',
    label: 'Start Date'
  },
  {
    key: 'endDate',
    label: 'End Date'
  },
  {
    key: 'status',
    label: 'Status'
  },
  {
    key: 'mediaPolicy',
    label: 'Media Policy',
    valueAccessor: (site) => site.mediaPolicy.preset,
    displayFormatter: (_, site) => formatMediaPolicyPreset(site.mediaPolicy.preset)
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
  private readonly router = inject(Router);
  private readonly offersService = inject(OffersService);

  readonly sites = signal<readonly DashboardSite[]>([]);
  readonly sitesFilteredCount = signal(0);
  readonly sitesTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardSite> | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly columns = SITE_COLUMNS;
  readonly rowActions: readonly DataTableRowAction<DashboardSite>[] = [
    {
      id: 'create-offer',
      label: 'Create Offer',
      ariaLabelAccessor: (site) => `Create an offer for Site ${site.numberId}`,
      disabledPredicate: () => this.offersService.createOfferMutation.isPending()
    },
    {
      id: 'view-offers',
      label: 'View Offers',
      ariaLabelAccessor: (site) => `View offers for Site ${site.numberId}`
    }
  ];
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

  async onRowActionClicked(
    event: DataTableRowActionEvent<DashboardSite>
  ): Promise<void> {
    this.pageError.set(null);
    if (event.action.id === 'view-offers') {
      await this.router.navigate(['/sites', event.row.id, 'offers']);
      return;
    }

    if (
      event.action.id !== 'create-offer' ||
      this.offersService.createOfferMutation.isPending()
    ) {
      return;
    }

    try {
      const created = await this.offersService.createOffer(event.row.id);
      await this.router.navigate([
        '/sites',
        event.row.id,
        'offers',
        created.id
      ]);
    } catch (error) {
      this.pageError.set(
        getOfferError(error, 'The draft offer could not be created.')
      );
    }
  }
}
