import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardRetailersService } from './dashboard-retailers.service';

describe('DashboardRetailersService', () => {
  let service: DashboardRetailersService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [DashboardRetailersService, provideTanStackQuery(new QueryClient()), provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(DashboardRetailersService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads retailers with normalized filters and sorting', async () => {
    service.setTableState({ overallRowsTotal: 0, filteredRowsTotal: 0, page: { pageIndex: 1, pageSize: 100 }, sort: { active: 'websiteHost', direction: 'asc' }, filters: {}, draftFilters: {}, appliedFilters: { displayName: '  Example Store  ', isActive: ' ' }, exportableColumns: [] });

    const refresh = service.dashboardRetailersQuery.refetch();
    const request = httpTesting.expectOne((candidate) => candidate.url === buildApiUrl('/dashboard/retailers'));
    expect(request.request.params.get('pageIndex')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('100');
    expect(request.request.params.get('sortActive')).toBe('websiteHost');
    expect(request.request.params.get('displayName')).toBe('example store');
    expect(request.request.params.has('isActive')).toBe(false);
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
    await refresh;
  });

  it('uses retailer write contracts and invalidates dashboard lists', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');
    const request = { displayName: 'Example Store', companyPersonId: 'company-1', baseWebsiteUrl: 'https://example.com', notes: null };

    const create = service.createRetailer(request);
    await Promise.resolve();
    const createRequest = httpTesting.expectOne(buildApiUrl('/retailers'));
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body).toEqual(request);
    createRequest.flush({ id: 'retailer-1' });
    await expect(create).resolves.toEqual({ id: 'retailer-1' });

    const update = service.updateRetailer({ id: 'retailer-1', ...request });
    await Promise.resolve();
    const updateRequest = httpTesting.expectOne(buildApiUrl('/retailers/retailer-1'));
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body).toEqual(request);
    updateRequest.flush(null);
    await expect(update).resolves.toBeNull();

    const status = service.setRetailerActive('retailer-1', false);
    await Promise.resolve();
    const statusRequest = httpTesting.expectOne(buildApiUrl('/retailers/retailer-1/deactivate'));
    expect(statusRequest.request.method).toBe('PATCH');
    statusRequest.flush(null);
    await expect(status).resolves.toBeNull();
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['retailers', 'dashboard'] });
  });

  it('loads details, searches trimmed terms, and skips blank retailer searches', async () => {
    const detail = service.getRetailerById('retailer-1');
    httpTesting.expectOne(buildApiUrl('/retailers/retailer-1')).flush({ id: 'retailer-1', displayName: 'Example Store' });
    await expect(detail).resolves.toMatchObject({ id: 'retailer-1' });

    await expect(service.searchRetailers('   ')).resolves.toEqual([]);
    const search = service.searchRetailers('  example  ');
    const request = httpTesting.expectOne(buildApiUrl('/dashboard/retailers/search?searchTerm=example'));
    request.flush([{ id: 'retailer-1', displayName: 'Example Store', baseWebsiteUrl: 'https://example.com', websiteHost: 'example.com' }]);
    await expect(search).resolves.toHaveLength(1);
  });
});
