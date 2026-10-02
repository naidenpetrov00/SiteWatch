import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';

export interface OfferFinalizeConfirmDialogData {
  siteId: string;
  offerId: string;
  offerNumber: number;
}

@Component({
  selector: 'app-offer-finalize-confirm-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './offer-finalize-confirm-dialog.component.html',
  styleUrl: './offer-finalize-confirm-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferFinalizeConfirmDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<OfferFinalizeConfirmDialogComponent>
  );
  readonly data = inject<OfferFinalizeConfirmDialogData>(MAT_DIALOG_DATA);
  readonly offersService = inject(OffersService);
  readonly readiness = computed(() =>
    this.offersService.finalizationReadinessQuery.data()
  );
  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly loading = computed(() =>
    this.offersService.finalizationReadinessQuery.isPending() ||
    this.offersService.finalizationReadinessQuery.isFetching()
  );
  readonly canSubmit = computed(() =>
    this.readiness()?.canFinalize === true &&
    !this.loading() &&
    !this.offersService.finalizationReadinessQuery.isError() &&
    !this.submitting()
  );

  constructor() {
    this.dialogRef.disableClose = true;
    void this.offersService.finalizationReadinessQuery.refetch();
  }

  cancel(): void {
    if (!this.submitting()) {
      this.dialogRef.close(false);
    }
  }

  async confirm(): Promise<void> {
    if (!this.canSubmit()) return;

    this.error.set(null);
    this.submitting.set(true);
    try {
      await this.offersService.finalizeOffer(this.data.siteId, this.data.offerId);
      this.dialogRef.close(true);
    } catch (error) {
      this.error.set(getOfferError(error));
      await this.offersService.refreshOfferWorkspace(
        this.data.siteId,
        this.data.offerId
      );
    } finally {
      this.submitting.set(false);
    }
  }

  formatCurrency(value: number | null): string {
    return value === null
      ? '—'
      : new Intl.NumberFormat(undefined, {
          style: 'currency',
          currency: 'EUR'
        }).format(value);
  }
}
