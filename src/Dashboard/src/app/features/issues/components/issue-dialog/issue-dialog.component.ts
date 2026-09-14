import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
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
import { DialogWizardTabsComponent } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import { DialogWizardTabDefinition } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.types';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardUserLookup } from '../../../users/models/dashboard-user-lookup.model';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardIssue } from '../../models/dashboard-issue.model';
import { DashboardIssuesService } from '../../services/dashboard-issues.service';
import {
  getIssueSaveError,
  toDashboardUserLookup,
  toIssueRequest,
  toLocalDate
} from '../../utils/issue-dialog.utils';
import { IssueAttachmentManager } from './issue-attachment-manager.service';
import { IssueAttachmentObjectUrlRegistry } from './issue-attachment-object-url-registry.service';
import { IssueAttachmentUploader } from './issue-attachment-uploader.service';
import { IssueAttachmentsSectionComponent } from './issue-attachments-section.component';

const ISSUE_STATUS_OPTIONS = [
  'Open',
  'InReview',
  'ApprovedWaitingPayment',
  'WorkingOn',
  'Completed'
] as const;

type IssueDialogTabId = 'details' | 'attachments';

const ISSUE_DIALOG_TABS: readonly DialogWizardTabDefinition[] = [
  {
    id: 'details',
    label: 'Details',
    description: 'Issue information and assignments'
  },
  {
    id: 'attachments',
    label: 'Attachments',
    description: 'Photos, videos, and files'
  }
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
    DialogShellComponent,
    DialogWizardTabsComponent,
    IssueAttachmentsSectionComponent
  ],
  templateUrl: './issue-dialog.component.html',
  styleUrl: './issue-dialog.component.css',
  providers: [
    IssueAttachmentManager,
    IssueAttachmentObjectUrlRegistry,
    IssueAttachmentUploader
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class IssueDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<IssueDialogComponent>);
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dashboardUsersService = inject(DashboardUsersService);
  readonly attachmentManager = inject(IssueAttachmentManager);
  readonly issue = inject(MAT_DIALOG_DATA, { optional: true }) as DashboardIssue | null;

  private siteSearchRevision = 0;
  private workerSearchRevision = 0;

  readonly statuses = ISSUE_STATUS_OPTIONS;
  readonly tabs = ISSUE_DIALOG_TABS;
  readonly selectedTabId = signal<IssueDialogTabId>('details');
  readonly persistedIssueId = signal<string | null>(this.issue?.id ?? null);
  private readonly saveInProgress = signal(false);
  readonly formId = this.issue ? 'edit-issue-dialog-form' : 'add-issue-dialog-form';
  readonly siteResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly workerResults = signal<readonly DashboardUserLookup[]>([]);
  readonly selectedWorkers = signal<readonly DashboardUserLookup[]>(
    this.issue?.assignedWorkers.map(toDashboardUserLookup) ?? []
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
    startDate: [toLocalDate(this.issue?.startDate ?? null)],
    endDate: [toLocalDate(this.issue?.endDate ?? null)],
    assignedWorkerIds: [this.issue?.assignedWorkers.map((worker) => worker.id) ?? []]
  }, { validators: dateRangeValidator });
  readonly isSaving = computed(() =>
    this.saveInProgress()
    || this.attachmentManager.isPersisting()
    || this.dashboardIssuesService.createIssueMutation.isPending()
    || this.dashboardIssuesService.updateIssueMutation.isPending()
  );
  readonly dialogTitle = computed(() => {
    if (this.issue) return `Manage Issue #${this.issue.numberId}`;
    return this.persistedIssueId() ? 'Manage Created Issue' : 'Add Issue';
  });
  readonly dialogSubtitle = computed(() =>
    this.persistedIssueId()
      ? 'Update the issue details and manage its attachments.'
      : 'Enter the issue details, site assignment, and optional attachments.'
  );
  readonly submitLabel = computed(() =>
    this.persistedIssueId() ? 'Save' : 'Add Issue'
  );
  readonly displaySite = (value: string | DashboardSiteLookup | null): string =>
    typeof value === 'string' ? value : value?.name ?? '';
  readonly displayWorker = (value: string | DashboardUserLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    if (this.issue) {
      void this.attachmentManager.load(this.issue.id);
    }

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
    if (this.isSaving()) return;
    this.dialogRef.close();
  }

  setSelectedTab(tabId: string): void {
    if (tabId === 'details' || tabId === 'attachments') {
      this.selectedTabId.set(tabId);
    }
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
      this.selectedTabId.set('details');
      return;
    }

    if (this.isSaving()) return;

    const request = toIssueRequest(this.issueForm.getRawValue());
    this.saveInProgress.set(true);
    this.dialogRef.disableClose = true;
    this.saveError.set(null);
    try {
      let issueId = this.persistedIssueId();
      if (issueId) {
        await this.dashboardIssuesService.updateIssue({ id: issueId, ...request });
      } else {
        const created = await this.dashboardIssuesService.createIssue(request);
        issueId = created.id;
        this.persistedIssueId.set(issueId);
      }

      const attachmentsSaved = await this.attachmentManager.saveChanges(issueId);
      if (attachmentsSaved) {
        this.dialogRef.close(true);
      } else {
        this.selectedTabId.set('attachments');
      }
    } catch (error) {
      this.saveError.set(getIssueSaveError(error));
      this.selectedTabId.set('details');
    } finally {
      this.dialogRef.disableClose = false;
      this.saveInProgress.set(false);
    }
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
