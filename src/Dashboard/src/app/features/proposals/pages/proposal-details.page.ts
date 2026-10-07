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
  ProposalIssueConfirmDialogComponent,
  ProposalIssueConfirmDialogData
} from '../components/proposal-issue-confirm-dialog.component';
import { ProposalsService } from '../services/proposals.service';
import { getProposalError } from '../utils/proposal-error';

@Component({
  selector: 'app-proposal-details-page',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './proposal-details.page.html',
  styleUrl: './proposal-details.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalDetailsPage {
  private readonly proposalsService = inject(ProposalsService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);
  private configuredProposalId: string | null = null;

  readonly siteId = input.required<string>();
  readonly proposalId = input.required<string>();
  readonly proposal = computed(() => this.proposalsService.detailQuery.data());
  readonly isDraft = computed(() => this.proposal()?.status === 'Draft');
  readonly pageMessage = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly metadataForm = this.formBuilder.group({
    validUntil: [''],
    publicNotes: ['', Validators.maxLength(2000)],
    paymentTerms: ['', Validators.maxLength(2000)]
  });

  constructor() {
    effect(() =>
      this.proposalsService.configureDetail(this.siteId(), this.proposalId())
    );
    effect(() => {
      const proposal = this.proposal();
      if (!proposal) {
        return;
      }

      if (
        this.configuredProposalId !== proposal.id ||
        !this.metadataForm.dirty ||
        proposal.status !== 'Draft'
      ) {
        this.metadataForm.reset(
          {
            validUntil: proposal.validUntil ?? '',
            publicNotes: proposal.publicNotes ?? '',
            paymentTerms: proposal.paymentTerms ?? ''
          },
          { emitEvent: false }
        );
      }
      this.configuredProposalId = proposal.id;
      if (proposal.status === 'Draft') {
        this.metadataForm.enable({ emitEvent: false });
      } else {
        this.metadataForm.disable({ emitEvent: false });
      }
    });
  }

  async saveMetadata(): Promise<void> {
    if (!this.isDraft() || this.metadataForm.invalid || this.isSaving()) {
      this.metadataForm.markAllAsTouched();
      return;
    }

    this.clearFeedback();
    const value = this.metadataForm.getRawValue();
    try {
      await this.proposalsService.updateMetadata({
        siteId: this.siteId(),
        proposalId: this.proposalId(),
        validUntil: value.validUntil || null,
        publicNotes: normalizeOptional(value.publicNotes),
        paymentTerms: normalizeOptional(value.paymentTerms)
      });
      this.metadataForm.markAsPristine();
      this.pageMessage.set('Proposal metadata saved.');
    } catch (error) {
      this.pageError.set(getProposalError(error));
    }
  }

  async confirmIssue(): Promise<void> {
    const proposal = this.proposal();
    if (!proposal || proposal.status !== 'Draft' || this.isIssuing()) {
      return;
    }
    if (this.metadataForm.dirty) {
      this.pageError.set('Save the Proposal metadata before issuing.');
      return;
    }
    if (!proposal.validUntil) {
      this.pageError.set('Set and save a validity date before issuing.');
      return;
    }

    this.clearFeedback();
    const dialogRef = this.dialog.open<
      ProposalIssueConfirmDialogComponent,
      ProposalIssueConfirmDialogData,
      boolean
    >(ProposalIssueConfirmDialogComponent, {
      autoFocus: false,
      width: '34rem',
      maxWidth: 'calc(100vw - 2rem)',
      ariaLabel: `Issue Proposal ${proposal.numberId} revision ${proposal.revisionNumber}`,
      data: {
        proposalNumber: proposal.numberId,
        revisionNumber: proposal.revisionNumber
      }
    });
    if (!(await firstValueFrom(dialogRef.afterClosed()))) {
      return;
    }

    try {
      await this.proposalsService.issue(this.siteId(), this.proposalId());
      this.pageMessage.set('Proposal issued and PDF stored.');
    } catch (error) {
      this.pageError.set(
        getProposalError(error, 'The Proposal could not be issued or its PDF could not be stored.')
      );
    }
  }

  async downloadPdf(): Promise<void> {
    const proposal = this.proposal();
    if (!proposal?.hasPdf || proposal.status === 'Draft') {
      return;
    }

    this.clearFeedback();
    try {
      await this.proposalsService.downloadPdf(this.siteId(), this.proposalId());
    } catch (error) {
      this.pageError.set(
        getProposalError(error, 'The Proposal PDF could not be downloaded.')
      );
    }
  }

  isLoading(): boolean {
    return this.proposalsService.detailQuery.isPending();
  }

  hasLoadError(): boolean {
    return this.proposalsService.detailQuery.isError();
  }

  isSaving(): boolean {
    return this.proposalsService.updateMetadataMutation.isPending();
  }

  isIssuing(): boolean {
    return this.proposalsService.issueMutation.isPending();
  }

  formatDateTime(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  formatDate(value: string): string {
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium' }).format(
      new Date(`${value}T00:00:00`)
    );
  }

  formatMoney(value: number): string {
    return new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: 'EUR'
    }).format(value);
  }

  formatQuantity(value: number): string {
    return new Intl.NumberFormat(undefined, { maximumFractionDigits: 8 }).format(value);
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
