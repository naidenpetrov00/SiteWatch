import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardIssuesService } from '../../services/dashboard-issues.service';
import { IssueDialogComponent } from './issue-dialog.component';

describe('IssueDialogComponent', () => {
  const issuesService = {
    createIssueMutation: { isPending: () => false }, updateIssueMutation: { isPending: () => false },
    createIssue: vi.fn(), updateIssue: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IssueDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: null },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: DashboardIssuesService, useValue: issuesService },
        { provide: DashboardSitesService, useValue: { searchSites: vi.fn() } },
        { provide: DashboardUsersService, useValue: { searchWorkers: vi.fn() } }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('does not add the same selected worker twice', () => {
    const component = TestBed.createComponent(IssueDialogComponent).componentInstance;
    const worker = { id: 'worker-1', displayName: 'Ada', email: 'ada@example.test' };

    component.onWorkerSelected({ option: { value: worker } } as never);
    component.onWorkerSelected({ option: { value: worker } } as never);

    expect(component.selectedWorkers()).toEqual([worker]);
    expect(component.issueForm.controls.assignedWorkerIds.value).toEqual(['worker-1']);
  });

  it('submits a valid new issue and closes the dialog on success', async () => {
    issuesService.createIssue.mockResolvedValue({ id: 'issue-1' });
    const component = TestBed.createComponent(IssueDialogComponent).componentInstance;
    component.issueForm.patchValue({ siteId: 'site-1', title: '  Gate  ', description: '  Broken  ', status: 'Open' });

    await component.submitIssue();

    expect(issuesService.createIssue).toHaveBeenCalledWith(expect.objectContaining({ title: 'Gate', description: 'Broken' }));
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('keeps the dialog open and exposes a save failure', async () => {
    issuesService.createIssue.mockRejectedValue(new Error('offline'));
    const component = TestBed.createComponent(IssueDialogComponent).componentInstance;
    component.issueForm.patchValue({ siteId: 'site-1', title: 'Gate', description: 'Broken', status: 'Open' });

    await component.submitIssue();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.saveError()).toBe('Unable to save the issue. Please review the details and try again.');
  });
});
