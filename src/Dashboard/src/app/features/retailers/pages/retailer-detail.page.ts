import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { RetailerListingDialogComponent } from '../../retailer-listings/components/retailer-listing-dialog/retailer-listing-dialog.component';
import { RetailerListingsTableComponent } from '../../retailer-listings/components/retailer-listings-table/retailer-listings-table.component';
import {
  RetailerListing,
  RetailerListingDialogData
} from '../../retailer-listings/models/retailer-listing.models';
import { RetailerDialogComponent } from '../components/retailer-dialog/retailer-dialog.component';
import {
  RetailerStatusConfirmDialogComponent,
  RetailerStatusConfirmDialogData
} from '../components/retailer-status-confirm-dialog/retailer-status-confirm-dialog.component';
import { DashboardRetailersService } from '../services/dashboard-retailers.service';
import { getRetailerError } from '../utils/retailer-error';

@Component({
  selector: 'app-retailer-detail-page',
  imports: [
    RouterLink,
    MatButtonModule,
    ActionButtonComponent,
    RetailerListingsTableComponent
  ],
  templateUrl: './retailer-detail.page.html',
  styleUrl: '../../products/pages/catalog-detail.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerDetailPage {
  private readonly retailersService = inject(DashboardRetailersService);
  private readonly dialog = inject(MatDialog);

  readonly retailerId = input.required<string>();
  readonly pageError = signal<string | null>(null);
  readonly retailerQuery = injectQuery(() => {
    const retailerId = this.retailerId();
    return {
      queryKey: ['retailers', 'detail', retailerId] as const,
      queryFn: () => this.retailersService.getRetailerById(retailerId),
      enabled: retailerId.length > 0
    };
  });

  async editRetailer(): Promise<void> {
    const retailer = this.retailerQuery.data();
    if (!retailer) return;
    const dialogRef = this.dialog.open<
      RetailerDialogComponent,
      typeof retailer,
      boolean
    >(RetailerDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '44rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: retailer
    });
    if (await firstValueFrom(dialogRef.afterClosed())) {
      await this.retailerQuery.refetch();
    }
  }

  openAddListing(): void {
    const retailer = this.retailerQuery.data();
    if (!retailer?.isActive) return;
    this.dialog.open<
      RetailerListingDialogComponent,
      RetailerListingDialogData,
      RetailerListing | null
    >(RetailerListingDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '52rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { listing: null, fixedRetailer: retailer }
    });
  }

  statusPending(): boolean {
    return this.retailersService.setRetailerStatusMutation.isPending();
  }

  async changeRetailerStatus(): Promise<void> {
    const retailer = this.retailerQuery.data();
    if (!retailer || this.retailersService.setRetailerStatusMutation.isPending()) {
      return;
    }
    const activate = !retailer.isActive;
    const confirmRef = this.dialog.open<
      RetailerStatusConfirmDialogComponent,
      RetailerStatusConfirmDialogData,
      boolean
    >(RetailerStatusConfirmDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { displayName: retailer.displayName, activate }
    });
    if (!(await firstValueFrom(confirmRef.afterClosed()))) return;

    this.pageError.set(null);
    try {
      await this.retailersService.setRetailerActive(retailer.id, activate);
      await this.retailerQuery.refetch();
    } catch (error) {
      this.pageError.set(getRetailerError(error));
    }
  }
}
