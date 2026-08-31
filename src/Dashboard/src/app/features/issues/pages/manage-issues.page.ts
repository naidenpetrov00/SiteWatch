import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { IssueDialogComponent } from '../components/issue-dialog/issue-dialog.component';
import { DashboardIssue, DashboardIssueWorker } from '../models/dashboard-issue.model';
import { DashboardIssuesService } from '../services/dashboard-issues.service';

const ISSUE_STATUS_OPTIONS = [
  { label: 'Open', value: 'Open' },
  { label: 'In review', value: 'InReview' },
  { label: 'Approved waiting payment', value: 'ApprovedWaitingPayment' },
  { label: 'Working on', value: 'WorkingOn' },
  { label: 'Completed', value: 'Completed' }
] as const;

const ISSUE_COLUMNS: readonly DataTableColumn<DashboardIssue>[] = [
  { key: 'numberId', label: 'Number Id', sortable: true, cellType: 'button', filter: { kind: 'number', placeholder: 'Filter Number Id' } },
  { key: 'id', label: 'Id', sortable: true, filter: { kind: 'text', placeholder: 'Filter Id' } },
  { key: 'siteName', label: 'Site', sortable: true, filter: { kind: 'text', placeholder: 'Filter Site' } },
  { key: 'title', label: 'Title', sortable: true, filter: { kind: 'text', placeholder: 'Filter Title' } },
  { key: 'description', label: 'Description', sortable: true, filter: { kind: 'text', placeholder: 'Filter Description' } },
  { key: 'status', label: 'Status', sortable: true, filter: { kind: 'select', placeholder: 'Filter Status', options: ISSUE_STATUS_OPTIONS } },
  { key: 'startDate', label: 'Start Date', sortable: true, filter: { kind: 'text', placeholder: 'Filter Start Date' } },
  { key: 'endDate', label: 'End Date', sortable: true, filter: { kind: 'text', placeholder: 'Filter End Date' } },
  { key: 'created', label: 'Created', sortable: true, filter: { kind: 'text', placeholder: 'Filter Created' }, displayFormatter: (value) => formatDateTime(value) },
  { key: 'worker', label: 'Assigned Workers', filter: { kind: 'text', placeholder: 'Filter Worker' }, valueAccessor: (issue) => formatWorkers(issue.assignedWorkers) }
] as const;

@Component({
  selector: 'app-manage-issues-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './manage-issues.page.html',
  styleUrl: './manage-issues.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageIssuesPage {
  private readonly dashboardIssuesService = inject(DashboardIssuesService);
  private readonly dialog = inject(MatDialog);

  readonly issues = signal<readonly DashboardIssue[]>([]);
  readonly issuesFilteredCount = signal(0);
  readonly issuesTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardIssue> | null>(null);
  readonly columns = ISSUE_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const state = this.tableState();
      if (state) this.dashboardIssuesService.setTableState(state);
    });
    effect(() => {
      const response = this.dashboardIssuesService.dashboardIssuesQuery.data();
      if (!response) return;
      this.issues.set(response.items);
      this.issuesFilteredCount.set(response.filteredCount);
      this.issuesTotalCount.set(response.totalCount);
    });
  }

  onTableStateChange(state: DataTableState<DashboardIssue>): void {
    this.tableState.set(state);
  }

  openAddIssueDialog(): void {
    this.dialog.open(IssueDialogComponent, {
      autoFocus: false,
      width: '56rem',
      maxWidth: 'calc(100vw - 2rem)'
    });
  }

  async onNumberIdClick(issue: DashboardIssue): Promise<void> {
    try {
      const details = await this.dashboardIssuesService.getIssueById(issue.id);
      this.dialog.open(IssueDialogComponent, {
        autoFocus: false,
        width: '56rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: details
      });
    } catch {
      // Keep the table usable when detail retrieval fails.
    }
  }
}

function formatDateTime(value: unknown): string {
  if (typeof value !== 'string') return '';
  return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
}

function formatWorkers(workers: readonly DashboardIssueWorker[]): string {
  return workers.map((worker) => worker.userName ?? worker.email ?? worker.id).join(', ');
}
