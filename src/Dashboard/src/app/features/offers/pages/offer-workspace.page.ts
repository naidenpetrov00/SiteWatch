import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import {
  OfferArchiveConfirmDialogComponent,
  OfferArchiveConfirmDialogData
} from '../components/offer-archive-confirm-dialog/offer-archive-confirm-dialog.component';
import { OffersService } from '../services/offers.service';
import { getOfferError } from '../utils/offer-error';

@Component({
  selector: 'app-offer-workspace-page',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './offer-workspace.page.html',
  styleUrl: './offer-workspace.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferWorkspacePage {
  private readonly offersService = inject(OffersService);
  private readonly dialog = inject(MatDialog);
  private readonly formBuilder = inject(FormBuilder);

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly offer = computed(() => this.offersService.offerDetailsQuery.data());
  readonly isDraft = computed(() => this.offer()?.status === 'Draft');
  readonly pageMessage = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly metadataForm = this.formBuilder.group({
    title: ['', [Validators.maxLength(200)]],
    notes: ['', [Validators.maxLength(2000)]]
  });

  constructor() {
    effect(() =>
      this.offersService.configureDetails(this.siteId(), this.offerId())
    );
    effect(() => {
      const offer = this.offer();
      if (!offer) {
        return;
      }

      this.metadataForm.reset(
        {
          title: offer.title ?? '',
          notes: offer.notes ?? ''
        },
        { emitEvent: false }
      );
      if (offer.status === 'Draft') {
        this.metadataForm.enable({ emitEvent: false });
      } else {
        this.metadataForm.disable({ emitEvent: false });
      }
    });
  }

  async saveMetadata(): Promise<void> {
    if (
      !this.isDraft() ||
      this.metadataForm.invalid ||
      this.offersService.updateOfferMutation.isPending()
    ) {
      this.metadataForm.markAllAsTouched();
      return;
    }

    this.clearFeedback();
    const value = this.metadataForm.getRawValue();
    try {
      await this.offersService.updateOffer({
        siteId: this.siteId(),
        offerId: this.offerId(),
        title: normalizeOptional(value.title),
        notes: normalizeOptional(value.notes)
      });
      this.pageMessage.set('Offer metadata saved.');
    } catch (error) {
      this.pageError.set(getOfferError(error));
    }
  }

  async confirmArchive(): Promise<void> {
    const offer = this.offer();
    if (!offer || offer.status === 'Archived') {
      return;
    }

    this.clearFeedback();
    const dialogRef = this.dialog.open<
      OfferArchiveConfirmDialogComponent,
      OfferArchiveConfirmDialogData,
      boolean
    >(OfferArchiveConfirmDialogComponent, {
      autoFocus: false,
      ariaLabel: `Archive Offer ${offer.numberId}`,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { offerNumber: offer.numberId }
    });
    if (!(await firstValueFrom(dialogRef.afterClosed()))) {
      return;
    }

    try {
      await this.offersService.archiveOffer(this.siteId(), this.offerId());
      this.pageMessage.set('Offer archived.');
    } catch (error) {
      this.pageError.set(getOfferError(error));
    }
  }

  isLoading(): boolean {
    return this.offersService.offerDetailsQuery.isPending();
  }

  hasLoadError(): boolean {
    return this.offersService.offerDetailsQuery.isError();
  }

  isSaving(): boolean {
    return this.offersService.updateOfferMutation.isPending();
  }

  isArchiving(): boolean {
    return this.offersService.archiveOfferMutation.isPending();
  }

  formatDateTime(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  private clearFeedback(): void {
    this.pageMessage.set(null);
    this.pageError.set(null);
  }
}

function normalizeOptional(value: string | null): string | null {
  const normalized = value?.trim() ?? '';
  return normalized.length > 0 ? normalized : null;
}
