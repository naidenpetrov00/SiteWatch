import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardInvoicesService } from './dashboard-invoices.service';

describe('DashboardInvoicesService', () => {
  let service: DashboardInvoicesService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardInvoicesService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardInvoicesService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('sends normalized table filters and sorting to the dashboard invoices endpoint', async () => {
    service.setTableState({
      overallRowsTotal: 0,
      filteredRowsTotal: 0,
      page: { pageIndex: 2, pageSize: 100 },
      sort: { active: 'invoiceNumber', direction: 'desc' },
      filters: {},
      draftFilters: {},
      appliedFilters: { invoiceNumber: '  INV-42  ', paymentMethod: '   ' },
      exportableColumns: []
    });

    const refetch = service.dashboardInvoicesQuery.refetch();
    await Promise.resolve();
    const request = httpTesting.expectOne(
      `${buildApiUrl('/dashboard/invoices')}?pageIndex=2&pageSize=100&sortActive=invoiceNumber&sortDirection=desc&invoiceNumber=inv-42`
    );
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
    await refetch;
  });

  it('omits sorting parameters when the table has no active sort direction', async () => {
    service.setTableState({
      overallRowsTotal: 0,
      filteredRowsTotal: 0,
      page: { pageIndex: 0, pageSize: 50 },
      sort: { active: 'invoiceNumber', direction: '' },
      filters: {}, draftFilters: {}, appliedFilters: {}, exportableColumns: []
    });

    const refetch = service.dashboardInvoicesQuery.refetch();
    await Promise.resolve();
    const request = httpTesting.expectOne(`${buildApiUrl('/dashboard/invoices')}?pageIndex=0&pageSize=50`);
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
    await refetch;
  });

  it('sends uploads as multipart data with the expected invoice URL', async () => {
    const upload = service.uploadInvoiceFile('invoice-1', new File(['pdf'], 'invoice.pdf', { type: 'application/pdf' }));
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/invoices/invoice-1/file'));

    expect(request.request.method).toBe('PUT');
    expect(request.request.body instanceof FormData).toBe(true);
    request.flush(null);

    await expect(upload).resolves.toBeUndefined();
  });

  it('sends edit requests without the route invoice identifier in the request body', async () => {
    const update = service.updateInvoice({
      invoiceId: 'invoice-1',
      supplierId: 'supplier-1',
      invoiceNumber: 'INV-42',
      date: '2026-01-02T00:00:00.000Z',
      paymentTerm: '2026-02-02T00:00:00.000Z',
      totalValue: 100,
      vatRate: 20,
      paymentMethod: 'Bank',
      siteAllocations: []
    });
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/invoices/invoice-1'));

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).not.toHaveProperty('invoiceId');
    expect(request.request.body).toMatchObject({ invoiceNumber: 'INV-42', vatRate: 20 });
    request.flush(null);

    await expect(update).resolves.toBeNull();
  });

  it('wraps allocation updates in the endpoint request contract', async () => {
    const update = service.updateSiteAllocations({
      invoiceId: 'invoice-1',
      siteAllocations: [{ siteId: 'site-1', amount: 120, direction: 'In' }]
    });
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/invoices/invoice-1/site-allocations'));

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      siteAllocations: [{ siteId: 'site-1', amount: 120, direction: 'In' }]
    });
    request.flush(null);

    await expect(update).resolves.toBeNull();
  });
});
