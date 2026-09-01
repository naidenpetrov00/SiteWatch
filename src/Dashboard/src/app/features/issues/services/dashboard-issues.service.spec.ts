import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardIssuesService } from './dashboard-issues.service';

describe('DashboardIssuesService', () => {
  let service: DashboardIssuesService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DashboardIssuesService, provideTanStackQuery(new QueryClient()), provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(DashboardIssuesService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('sends normalized table filters and sorting to the dashboard issues endpoint', async () => {
    service.setTableState({
      overallRowsTotal: 0, filteredRowsTotal: 0, page: { pageIndex: 2, pageSize: 100 },
      sort: { active: 'title', direction: 'desc' }, filters: {}, draftFilters: {},
      appliedFilters: { title: '  Gate  ', worker: '   ' }, exportableColumns: []
    });

    const refetch = service.dashboardIssuesQuery.refetch();
    await Promise.resolve();
    const request = httpTesting.expectOne(`${buildApiUrl('/dashboard/issues')}?pageIndex=2&pageSize=100&sortActive=title&sortDirection=desc&title=gate`);
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
    await refetch;
  });

  it('posts creates and puts updates without leaking the route identifier into the body', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');
    const create = service.createIssue({ siteId: 'site-1', title: 'Gate', description: 'Broken', status: 'Open', startDate: null, endDate: null, assignedWorkerIds: [] });
    await Promise.resolve();
    const createRequest = httpTesting.expectOne(buildApiUrl('/issues'));
    expect(createRequest.request.method).toBe('POST');
    createRequest.flush({ id: 'issue-1' });
    await expect(create).resolves.toEqual({ id: 'issue-1' });
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['issues', 'dashboard'] });

    const update = service.updateIssue({ id: 'issue-1', siteId: 'site-1', title: 'Gate', description: 'Fixed', status: 'Completed', startDate: null, endDate: null, assignedWorkerIds: [] });
    await Promise.resolve();
    const updateRequest = httpTesting.expectOne(buildApiUrl('/issues/issue-1'));
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body).not.toHaveProperty('id');
    expect(updateRequest.request.body).toMatchObject({ status: 'Completed', description: 'Fixed' });
    updateRequest.flush(null);
    await expect(update).resolves.toBeNull();
  });
});
