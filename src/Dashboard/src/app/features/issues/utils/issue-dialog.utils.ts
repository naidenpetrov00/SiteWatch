import { HttpErrorResponse } from '@angular/common/http';

import { DashboardUserLookup } from '../../users/models/dashboard-user-lookup.model';
import { DashboardIssueWorker } from '../models/dashboard-issue.model';
import { IssueRequest } from '../models/issue-request.model';

interface IssueFormValue {
  siteId: string | null;
  title: string | null;
  description: string | null;
  status: string | null;
  startDate: Date | null;
  endDate: Date | null;
  assignedWorkerIds: string[] | null;
}

export function toDashboardUserLookup(worker: DashboardIssueWorker): DashboardUserLookup {
  return {
    id: worker.id,
    displayName: worker.userName ?? worker.email ?? worker.id,
    email: worker.email
  };
}

export function toIssueRequest(value: IssueFormValue): IssueRequest {
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

export function toLocalDate(value: string | null): Date | null {
  if (!value) return null;
  const [year, month, day] = value.split('-').map(Number);
  return new Date(year, month - 1, day);
}

export function getIssueSaveError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const errors = error.error?.errors as Record<string, string[]> | undefined;
    const message = errors && Object.values(errors).flat().find(Boolean);
    if (message) return message;
  }

  return 'Unable to save the issue. Please review the details and try again.';
}

function toDateOnly(value: Date | null): string | null {
  if (!value) return null;
  return `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`;
}
