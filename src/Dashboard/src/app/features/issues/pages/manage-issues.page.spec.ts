import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { DashboardIssuesService } from '../services/dashboard-issues.service';
import { ManageIssuesPage } from './manage-issues.page';

describe('ManageIssuesPage', () => {
  const issuesService = { dashboardIssuesQuery: { data: () => undefined }, setTableState: vi.fn(), getIssueById: vi.fn() };
  const dialog = { open: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageIssuesPage],
      providers: [
        { provide: DashboardIssuesService, useValue: issuesService },
        provideTanStackQuery(new QueryClient())
      ]
    })
      .overrideProvider(MatDialog, { useValue: dialog })
      .compileComponents();
    vi.clearAllMocks();
  });

  it('renders the Manage Issues heading and opens the add dialog', async () => {
    const fixture = TestBed.createComponent(ManageIssuesPage);
    fixture.componentInstance.openAddIssueDialog();
    await fixture.whenStable();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Manage Issues');
    expect(dialog.open).toHaveBeenCalledTimes(1);
  });

  it('opens retrieved issue details and leaves the table usable when retrieval fails', async () => {
    issuesService.getIssueById.mockResolvedValue({ id: 'issue-1' });
    const component = TestBed.createComponent(ManageIssuesPage).componentInstance;

    await component.onNumberIdClick({ id: 'issue-1' } as never);
    expect(dialog.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: { id: 'issue-1' } }));

    issuesService.getIssueById.mockRejectedValue(new Error('offline'));
    await expect(component.onNumberIdClick({ id: 'issue-1' } as never)).resolves.toBeUndefined();
  });
});
