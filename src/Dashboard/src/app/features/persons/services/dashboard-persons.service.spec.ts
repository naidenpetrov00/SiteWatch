import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardPersonsService } from './dashboard-persons.service';

describe('DashboardPersonsService company search', () => {
  let service: DashboardPersonsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [DashboardPersonsService, provideTanStackQuery(new QueryClient()), provideHttpClient(), provideHttpClientTesting()] });
    service = TestBed.inject(DashboardPersonsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('searches companies with a trimmed term and avoids requests for blank input', async () => {
    await expect(service.searchCompanies('  ')).resolves.toEqual([]);
    const search = service.searchCompanies('  Example Ltd  ');
    const request = httpTesting.expectOne(buildApiUrl('/dashboard/persons/companies/search?searchTerm=Example%20Ltd'));
    expect(request.request.method).toBe('GET');
    request.flush([{ id: 'company-1', displayName: 'Example Ltd', legalForm: null, eik: '123456789', vatNumber: 'BG123' }]);
    await expect(search).resolves.toHaveLength(1);
  });
});
