import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

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

  it('sends normalized table filters and sorting to the dashboard invoices endpoint', () => {
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

    const request = httpTesting.expectOne('/dashboard/invoices?pageIndex=2&pageSize=100&sortActive=invoiceNumber&sortDirection=desc&invoiceNumber=inv-42');
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
  });

  it('sends uploads as multipart data with the expected invoice URL', async () => {
    const upload = service.uploadInvoiceFile('invoice-1', new File(['pdf'], 'invoice.pdf', { type: 'application/pdf' }));
    const request = httpTesting.expectOne('/invoices/invoice-1/file');

    expect(request.request.method).toBe('PUT');
    expect(request.request.body instanceof FormData).toBeTrue();
    request.flush(null);

    await expect(upload).resolves.toBeUndefined();
  });
});
