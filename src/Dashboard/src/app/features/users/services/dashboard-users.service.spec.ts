import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardUsersService } from './dashboard-users.service';

describe('DashboardUsersService', () => {
  let service: DashboardUsersService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardUsersService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardUsersService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('returns no manager results without making a request for blank input', async () => {
    await expect(service.searchUsers('   ')).resolves.toEqual([]);

    httpTesting.expectNone(buildApiUrl('/dashboard/users/search'));
  });

  it('trims the manager search term before sending it to the dashboard lookup endpoint', async () => {
    const search = service.searchUsers('  Ada  ');
    const request = httpTesting.expectOne(
      `${buildApiUrl('/dashboard/users/search')}?searchTerm=Ada&role=`
    );

    expect(request.request.method).toBe('GET');
    request.flush([{ id: 'user-1', displayName: 'Ada Lovelace', email: 'ada@example.com' }]);

    await expect(search).resolves.toEqual([
      { id: 'user-1', displayName: 'Ada Lovelace', email: 'ada@example.com' }
    ]);
  });

  it('searches workers with the worker role filter', async () => {
    const search = service.searchWorkers('  Ada  ');
    const request = httpTesting.expectOne(
      `${buildApiUrl('/dashboard/users/search')}?searchTerm=Ada&role=Worker`
    );

    request.flush([]);

    await expect(search).resolves.toEqual([]);
  });
});
