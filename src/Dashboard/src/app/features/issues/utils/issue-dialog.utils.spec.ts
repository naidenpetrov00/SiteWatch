import { HttpErrorResponse } from '@angular/common/http';

import { getIssueSaveError, toDashboardUserLookup, toIssueRequest, toLocalDate } from './issue-dialog.utils';

describe('issue dialog utilities', () => {
  it('maps trimmed form values and local calendar dates to the issue request contract', () => {
    expect(toIssueRequest({
      siteId: 'site-1', title: '  Gate  ', description: '  Broken  ', status: null,
      startDate: new Date(2026, 3, 2), endDate: null, assignedWorkerIds: ['worker-1']
    })).toEqual({
      siteId: 'site-1', title: 'Gate', description: 'Broken', status: 'Open',
      startDate: '2026-04-02', endDate: null, assignedWorkerIds: ['worker-1']
    });
    expect(toLocalDate('2026-04-02')).toEqual(new Date(2026, 3, 2));
    expect(toDashboardUserLookup({ id: 'worker-1', userName: null, email: 'worker@example.test' }).displayName)
      .toBe('worker@example.test');
  });

  it('shows the first server validation message and has a safe fallback', () => {
    expect(getIssueSaveError(new HttpErrorResponse({ status: 400, error: { errors: { title: ['Title is required.'] } } })))
      .toBe('Title is required.');
    expect(getIssueSaveError(new Error('offline'))).toBe('Unable to save the issue. Please review the details and try again.');
  });
});
