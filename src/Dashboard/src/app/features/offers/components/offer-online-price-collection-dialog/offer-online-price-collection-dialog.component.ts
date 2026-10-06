import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal
} from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatCheckboxModule } from '@angular/material/checkbox';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  OfferOnlinePriceCollectionCompanyOption,
  OfferOnlinePriceCollectionStartResponse
} from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';

export interface OfferOnlinePriceCollectionDialogData {
  siteId: string;
  offerId: string;
}

@Component({
  selector: 'app-offer-online-price-collection-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, MatCheckboxModule],
  templateUrl: './offer-online-price-collection-dialog.component.html',
  styleUrl: './offer-online-price-collection-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferOnlinePriceCollectionDialogComponent {
  private readonly dialogRef = inject(
    MatDialogRef<
      OfferOnlinePriceCollectionDialogComponent,
      OfferOnlinePriceCollectionStartResponse | null
    >
  );
  readonly data = inject<OfferOnlinePriceCollectionDialogData>(MAT_DIALOG_DATA);
  readonly offersService = inject(OffersService);
  readonly selectedCompanyIds = signal<ReadonlySet<string>>(new Set());
  readonly error = signal<string | null>(null);
  readonly submitting = signal(false);

  readonly options = computed(() =>
    this.offersService.onlinePriceCollectionOptionsQuery.data()
  );
  readonly canSubmit = computed(() =>
    this.selectedCompanyIds().size > 0 && !this.submitting()
  );

  constructor() {
    this.dialogRef.disableClose = true;
    void this.loadOptions();
  }

  private async loadOptions(): Promise<void> {
    await this.offersService.refreshOnlinePriceCollectionOptions();
    const options = this.options();
    if (!options) return;
    this.selectedCompanyIds.set(new Set(
      options.companies
        .filter(company => company.canStart)
        .map(company => company.companyPersonId)
    ));
  }

  isSelected(companyPersonId: string): boolean {
    return this.selectedCompanyIds().has(companyPersonId);
  }

  toggle(company: OfferOnlinePriceCollectionCompanyOption, checked: boolean): void {
    if (!company.canStart) return;
    const selected = new Set(this.selectedCompanyIds());
    if (checked) selected.add(company.companyPersonId);
    else selected.delete(company.companyPersonId);
    this.selectedCompanyIds.set(selected);
  }

  blockingLabel(company: OfferOnlinePriceCollectionCompanyOption): string {
    if (company.blockingCodes.includes('unfinishedCompanyRun')) {
      return 'Another company run is queued or running.';
    }
    if (company.blockingCodes.includes('missingActivePublishedProfile')) {
      return 'No active published extraction profile.';
    }
    if (company.blockingCodes.includes('noCollectableListings')) {
      return 'No active offer listings with saved product URLs.';
    }
    return 'This company is not currently available.';
  }

  cancel(): void {
    if (!this.submitting()) this.dialogRef.close(null);
  }

  async submit(): Promise<void> {
    if (!this.canSubmit()) return;
    this.error.set(null);
    this.submitting.set(true);
    try {
      const response = await this.offersService.startOnlinePriceCollection({
        siteId: this.data.siteId,
        offerId: this.data.offerId,
        companyPersonIds: [...this.selectedCompanyIds()]
      });
      this.dialogRef.close(response);
    } catch (error) {
      this.error.set(getOfferError(error));
      await this.offersService.refreshOnlinePriceCollectionOptions();
    } finally {
      this.submitting.set(false);
    }
  }
}
