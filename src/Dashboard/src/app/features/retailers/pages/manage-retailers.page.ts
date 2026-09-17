import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal
} from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { RetailerDialogComponent } from '../components/retailer-dialog/retailer-dialog.component';
import {
  RetailerStatusConfirmDialogComponent,
  RetailerStatusConfirmDialogData
} from '../components/retailer-status-confirm-dialog/retailer-status-confirm-dialog.component';
import {
  DashboardRetailer,
  DashboardRetailerDetails
} from '../models/dashboard-retailer.models';
import { DashboardRetailersService } from '../services/dashboard-retailers.service';
import { getRetailerError } from '../utils/retailer-error';

@Component({
  selector: 'app-manage-retailers-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './manage-retailers.page.html',
  styleUrl: './manage-retailers.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageRetailersPage {
  private readonly retailersService = inject(DashboardRetailersService);
  private readonly dialog = inject(MatDialog);

  readonly retailers = signal<readonly DashboardRetailer[]>([]);
  readonly filteredCount = signal(0);
  readonly totalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardRetailer> | null>(null);
  readonly pageMessage = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;
  readonly columns: readonly DataTableColumn<DashboardRetailer>[] = [
    {
      key: 'displayName',
      label: 'Retailer Name',
      sortable: true,
      cellType: 'button',
      filter: { kind: 'text', placeholder: 'Filter Retailer Name' },
      ariaLabelAccessor: (retailer) => `View or edit ${retailer.displayName}`
    },
    {
      key: 'companyDisplayName',
      label: 'Legal Company',
      sortable: true,
      filter: { kind: 'text', placeholder: 'Filter Legal Company' }
    },
    {
      key: 'websiteHost',
      label: 'Website',
      sortable: true,
      cellType: 'external-link',
      filter: { kind: 'text', placeholder: 'Filter Website Host' },
      linkHrefAccessor: (retailer) => retailer.baseWebsiteUrl,
      ariaLabelAccessor: (retailer) =>
        `Open ${retailer.displayName} website in a new tab`
    },
    {
      key: 'isActive',
      label: 'Status',
      sortable: true,
      cellType: 'button',
      filter: {
        kind: 'select',
        placeholder: 'Filter Status',
        options: [
          { label: 'Active', value: 'true' },
          { label: 'Inactive', value: 'false' }
        ]
      },
      displayFormatter: (value) => (value ? 'Active' : 'Inactive'),
      ariaLabelAccessor: (retailer) =>
        retailer.isActive
          ? `Deactivate ${retailer.displayName}`
          : `Activate ${retailer.displayName}`,
      buttonDisabledPredicate: () =>
        this.retailersService.setRetailerStatusMutation.isPending()
    }
  ];

  constructor() {
    effect(() => {
      const state = this.tableState();
      if (state) {
        this.retailersService.setTableState(state);
      }
    });

    effect(() => {
      const response = this.retailersService.dashboardRetailersQuery.data();
      if (response) {
        this.retailers.set(response.items);
        this.filteredCount.set(response.filteredCount);
        this.totalCount.set(response.totalCount);
      }
    });
  }

  hasLoadError(): boolean {
    return this.retailersService.dashboardRetailersQuery.isError();
  }

  onTableStateChange(state: DataTableState<DashboardRetailer>): void {
    this.tableState.set(state);
  }

  async openAddRetailerDialog(): Promise<void> {
    this.clearFeedback();
    const dialogRef = this.dialog.open<
      RetailerDialogComponent,
      null,
      boolean
    >(RetailerDialogComponent, {
      autoFocus: false,
      ariaLabel: 'Add Retailer',
      width: '44rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: null
    });
    if (await firstValueFrom(dialogRef.afterClosed())) {
      this.pageMessage.set('Retailer created successfully.');
    }
  }

  async onCellButtonClick(event: {
    row: DashboardRetailer;
    column: DataTableColumn<DashboardRetailer>;
  }): Promise<void> {
    if (event.column.key === 'displayName') {
      await this.openEditRetailerDialog(event.row);
      return;
    }

    if (event.column.key === 'isActive') {
      await this.confirmStatusChange(event.row);
    }
  }

  private async openEditRetailerDialog(retailer: DashboardRetailer): Promise<void> {
    this.clearFeedback();
    try {
      const details = await this.retailersService.getRetailerById(retailer.id);
      const dialogRef = this.dialog.open<
        RetailerDialogComponent,
        DashboardRetailerDetails,
        boolean
      >(RetailerDialogComponent, {
        autoFocus: false,
        ariaLabel: `Edit ${details.displayName}`,
        width: '44rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: details
      });
      if (await firstValueFrom(dialogRef.afterClosed())) {
        this.pageMessage.set('Retailer updated successfully.');
      }
    } catch (error) {
      this.pageError.set(
        getRetailerError(error, 'The retailer details could not be loaded.')
      );
    }
  }

  private async confirmStatusChange(retailer: DashboardRetailer): Promise<void> {
    if (this.retailersService.setRetailerStatusMutation.isPending()) {
      return;
    }

    this.clearFeedback();
    const activate = !retailer.isActive;
    const dialogRef = this.dialog.open<
      RetailerStatusConfirmDialogComponent,
      RetailerStatusConfirmDialogData,
      boolean
    >(RetailerStatusConfirmDialogComponent, {
      autoFocus: false,
      ariaLabel: `${activate ? 'Activate' : 'Deactivate'} ${retailer.displayName}`,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { displayName: retailer.displayName, activate }
    });
    if (!(await firstValueFrom(dialogRef.afterClosed()))) {
      return;
    }

    try {
      await this.retailersService.setRetailerActive(retailer.id, activate);
      this.pageMessage.set(
        `${retailer.displayName} was ${activate ? 'activated' : 'deactivated'}.`
      );
    } catch (error) {
      this.pageError.set(getRetailerError(error));
    }
  }

  private clearFeedback(): void {
    this.pageMessage.set(null);
    this.pageError.set(null);
  }
}
