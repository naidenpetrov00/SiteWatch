import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardUserLookup } from '../../../users/models/dashboard-user-lookup.model';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardIssue, DashboardIssueWorker } from '../../models/dashboard-issue.model';
import { IssueRequest } from '../../models/issue-request.model';
import { DashboardIssuesService } from '../../services/dashboard-issues.service';

const ISSUE_STATUS_OPTIONS = [
  'Open',
  'InReview',
  'ApprovedWaitingPayment',
  'WorkingOn',
  'Completed'
] as const;

const dateRangeValidator: ValidatorFn = (control): ValidationErrors | null => {
  const { startDate, endDate } = control.value as { startDate: Date | null; endDate: Date | null };
  return startDate && endDate && endDate < startDate ? { dateRange: true } : null;
};

@Component({
  selector: 'app-issue-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    DatepickerComponent,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './issue-dialog.component.html',
  styleUrl: './issue-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IssueDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<IssueDialogComponent>);
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dashboardUsersService = inject(DashboardUsersService);
  readonly issue = inject(MAT_DIALOG_DATA, { optional: true }) as DashboardIssue | null;

  private siteSearchRevision = 0;
  private workerSearchRevision = 0;

  readonly statuses = ISSUE_STATUS_OPTIONS;
  readonly formId = this.issue ? 'edit-issue-dialog-form' : 'add-issue-dialog-form';
  readonly siteResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly workerResults = signal<readonly DashboardUserLookup[]>([]);
  readonly selectedWorkers = signal<readonly DashboardUserLookup[]>(
    this.issue?.assignedWorkers.map(toUserLookup) ?? []
  );
  readonly saveError = signal<string | null>(null);
  readonly siteSearchControl = this.formBuilder.control<string | DashboardSiteLookup | null>(
    this.issue?.siteName ?? ''
  );
  readonly workerSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>('');
  readonly issueForm = this.formBuilder.group({
    siteId: [this.issue?.siteId ?? '', [Validators.required]],
    title: [this.issue?.title ?? '', [Validators.required, Validators.maxLength(200)]],
    description: [this.issue?.description ?? '', [Validators.required, Validators.maxLength(4000)]],
    status: [this.issue?.status ?? 'Open', [Validators.required]],
    startDate: [toDate(this.issue?.startDate ?? null)],
    endDate: [toDate(this.issue?.endDate ?? null)],
    assignedWorkerIds: [this.issue?.assignedWorkers.map((worker) => worker.id) ?? []]
  }, { validators: dateRangeValidator });
  readonly isSaving = () =>
    this.issue
      ? this.dashboardIssuesService.updateIssueMutation.isPending()
      : this.dashboardIssuesService.createIssueMutation.isPending();
  readonly displaySite = (value: string | DashboardSiteLookup | null): string =>
    typeof value === 'string' ? value : value?.name ?? '';
  readonly displayWorker = (value: string | DashboardUserLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    this.siteSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        this.issueForm.controls.siteId.setValue('', { emitEvent: false });
        void this.searchSites(value);
      });
    this.workerSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') void this.searchWorkers(value);
      });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  onSiteSelected(event: MatAutocompleteSelectedEvent): void {
    const site = event.option.value as DashboardSiteLookup;
    this.siteSearchRevision += 1;
    this.siteResults.set([]);
    this.issueForm.controls.siteId.setValue(site.id, { emitEvent: false });
  }

  onWorkerSelected(event: MatAutocompleteSelectedEvent): void {
    const worker = event.option.value as DashboardUserLookup;
    this.workerSearchRevision += 1;
    this.workerResults.set([]);
    this.workerSearchControl.setValue('', { emitEvent: false });
    if (this.selectedWorkers().some((selected) => selected.id === worker.id)) return;

    const workers = [...this.selectedWorkers(), worker];
    this.selectedWorkers.set(workers);
    this.issueForm.controls.assignedWorkerIds.setValue(workers.map((selected) => selected.id));
  }

  removeWorker(workerId: string): void {
    const workers = this.selectedWorkers().filter((worker) => worker.id !== workerId);
    this.selectedWorkers.set(workers);
    this.issueForm.controls.assignedWorkerIds.setValue(workers.map((worker) => worker.id));
  }

  async submitIssue(): Promise<void> {
    if (this.issueForm.invalid) {
      this.issueForm.markAllAsTouched();
      this.siteSearchControl.markAsTouched();
      return;
    }

    this.saveError.set(null);
    const request = this.toIssueRequest();
    try {
      if (this.issue) {
        await this.dashboardIssuesService.updateIssue({ id: this.issue.id, ...request });
      } else {
        await this.dashboardIssuesService.createIssue(request);
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.saveError.set(getErrorMessage(error));
    }
  }

  private toIssueRequest(): IssueRequest {
    const value = this.issueForm.getRawValue();
    return {
      siteId: value.siteId ?? '',
      title: value.title?.trim() ?? '',
      description: value.description?.trim() ?? '',
      status: value.status ?? 'Open',
      startDate: toDateOnly(value.startDate),
      endDate: toDateOnly(value.endDate),
      assignedWorkerIds: value.assignedWorkerIds ?? []
    };
  }

  private async searchSites(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.siteSearchRevision;
    this.siteResults.set([]);
    if (!searchTerm) return;
    try {
      const sites = await this.dashboardSitesService.searchSites(searchTerm);
      if (revision === this.siteSearchRevision) this.siteResults.set(sites);
    } catch {
      if (revision === this.siteSearchRevision) this.siteResults.set([]);
    }
  }

  private async searchWorkers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.workerSearchRevision;
    this.workerResults.set([]);
    if (!searchTerm) return;
    try {
      const workers = await this.dashboardUsersService.searchWorkers(searchTerm);
      if (revision === this.workerSearchRevision) this.workerResults.set(workers);
    } catch {
      if (revision === this.workerSearchRevision) this.workerResults.set([]);
    }
  }
}

function toUserLookup(worker: DashboardIssueWorker): DashboardUserLookup {
  return { id: worker.id, displayName: worker.userName ?? worker.email ?? worker.id, email: worker.email };
}

function toDate(value: string | null): Date | null {
  if (!value) return null;
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

function toDateOnly(value: Date | null | undefined): string | null {
  if (!value) return null;
  return `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`;
}

function getErrorMessage(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const errors = error.error?.errors as Record<string, string[]> | undefined;
    const message = errors && Object.values(errors).flat().find(Boolean);
    if (message) return message;
  }
  return 'Unable to save the issue. Please review the details and try again.';
}
