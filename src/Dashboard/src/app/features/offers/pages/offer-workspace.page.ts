import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  viewChild
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import {
  OfferArchiveConfirmDialogComponent,
  OfferArchiveConfirmDialogData
} from '../components/offer-archive-confirm-dialog/offer-archive-confirm-dialog.component';
import { OfferActivityBrowserComponent } from '../components/offer-activity-browser/offer-activity-browser.component';
import {
  OfferFinalizeConfirmDialogComponent,
  OfferFinalizeConfirmDialogData
} from '../components/offer-finalize-confirm-dialog/offer-finalize-confirm-dialog.component';
import { OfferPricingMatrixComponent } from '../components/offer-pricing-matrix/offer-pricing-matrix.component';
import { OfferCommercialSummaryComponent } from '../components/offer-commercial-summary/offer-commercial-summary.component';
import { OfferSelectedActivitiesComponent } from '../components/offer-selected-activities/offer-selected-activities.component';
import { OffersService } from '../services/offers.service';
import { getOfferError } from '../utils/offer-error';
import { ProposalsService } from '../../proposals/services/proposals.service';
import { getProposalError } from '../../proposals/utils/proposal-error';

@Component({
  selector: 'app-offer-workspace-page',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    OfferActivityBrowserComponent,
    OfferCommercialSummaryComponent,
    OfferPricingMatrixComponent,
    OfferSelectedActivitiesComponent,
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
  private readonly proposalsService = inject(ProposalsService);
  private readonly router = inject(Router);
  private configuredOfferId: string | null = null;

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly offer = computed(() => this.offersService.offerDetailsQuery.data());
  readonly pricing = computed(() => this.offersService.pricingMatrixQuery.data());
  readonly isDraft = computed(() => this.offer()?.status === 'Draft');
  readonly pricingMatrix = viewChild(OfferPricingMatrixComponent);
  readonly commercialSummary = viewChild(OfferCommercialSummaryComponent);
  readonly selectedActivities = viewChild(OfferSelectedActivitiesComponent);
  readonly pageMessage = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly proposals = computed(
    () => this.proposalsService.historyQuery.data() ?? []
  );
  readonly hasDraftProposal = computed(() =>
    this.proposals().some((proposal) => proposal.status === 'Draft')
  );
  readonly metadataForm = this.formBuilder.group({
    title: ['', [Validators.maxLength(200)]],
    notes: ['', [Validators.maxLength(2000)]]
  });

  constructor() {
    effect(() =>
      this.offersService.configureDetails(this.siteId(), this.offerId())
    );
    effect(() =>
      this.proposalsService.configureHistory(this.siteId(), this.offerId())
    );
    effect(() => {
      const offer = this.offer();
      if (!offer) {
        return;
      }

      if (
        this.configuredOfferId !== offer.id ||
        !this.metadataForm.dirty ||
        offer.status !== 'Draft'
      ) {
        this.metadataForm.reset(
          {
            title: offer.title ?? '',
            notes: offer.notes ?? ''
          },
          { emitEvent: false }
        );
      }
      this.configuredOfferId = offer.id;
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
      this.metadataForm.markAsPristine();
      this.pageMessage.set('Offer metadata saved.');
    } catch (error) {
      this.pageError.set(getOfferError(error));
    }
  }

  async confirmFinalize(): Promise<void> {
    const offer = this.offer();
    if (!offer || offer.status !== 'Draft' || this.isFinalizing()) {
      return;
    }

    if (this.metadataForm.dirty || this.pricingMatrix()?.hasUnsavedEdits() ||
        this.commercialSummary()?.hasUnsavedEdits() ||
        this.selectedActivities()?.hasUnsavedEdits()) {
      this.pageError.set('Save or cancel unsaved Offer edits before finalizing.');
      return;
    }
    if (this.isSaving() || this.isArchiving() ||
        this.pricingMatrix()?.isMutating() ||
        this.commercialSummary()?.isMutating() ||
        this.selectedActivities()?.isMutating() ||
        this.offersService.addActivityMutation.isPending()) {
      this.pageError.set('Wait for the current Offer change to finish before finalizing.');
      return;
    }

    this.clearFeedback();
    const dialogRef = this.dialog.open<
      OfferFinalizeConfirmDialogComponent,
      OfferFinalizeConfirmDialogData,
      boolean
    >(OfferFinalizeConfirmDialogComponent, {
      autoFocus: false,
      ariaLabel: `Finalize Offer ${offer.numberId}`,
      width: '38rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerNumber: offer.numberId
      }
    });
    if (await firstValueFrom(dialogRef.afterClosed())) {
      this.pageMessage.set('Offer finalized.');
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

  onActivityAdded(): void {
    this.pageError.set(null);
    this.pageMessage.set('Activity added and product requirements recalculated.');
  }

  async createProposal(): Promise<void> {
    const offer = this.offer();
    if (
      !offer ||
      offer.status !== 'Finalized' ||
      this.hasDraftProposal() ||
      this.proposalsService.createMutation.isPending()
    ) {
      return;
    }

    this.clearFeedback();
    try {
      const created = await this.proposalsService.create(
        this.siteId(),
        this.offerId()
      );
      await this.router.navigate([
        '/sites',
        this.siteId(),
        'proposals',
        created.id
      ]);
    } catch (error) {
      this.pageError.set(
        getProposalError(error, 'The draft Proposal could not be created.')
      );
    }
  }

  async openProposal(proposalId: string): Promise<void> {
    await this.router.navigate([
      '/sites',
      this.siteId(),
      'proposals',
      proposalId
    ]);
  }

  isLoadingProposals(): boolean {
    return this.proposalsService.historyQuery.isPending();
  }

  hasProposalLoadError(): boolean {
    return this.proposalsService.historyQuery.isError();
  }

  isCreatingProposal(): boolean {
    return this.proposalsService.createMutation.isPending();
  }

  formatMoney(value: number): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
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

  isFinalizing(): boolean {
    return this.offersService.finalizeOfferMutation.isPending();
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
