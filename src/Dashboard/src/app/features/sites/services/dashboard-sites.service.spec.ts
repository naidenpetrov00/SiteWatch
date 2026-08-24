import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardSitesService } from './dashboard-sites.service';

describe('DashboardSitesService', () => {
  let service: DashboardSitesService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardSitesService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardSitesService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads media policy presets from the dashboard contract', async () => {
    const requestPromise = service.getMediaPolicyPresets();
    const request = httpTesting.expectOne(buildApiUrl('/dashboard/sites/media-policy-presets'));
    const presets = [
      {
        preset: 'ApartmentRenovation' as const,
        displayName: 'Apartment Renovation',
        categories: ['Design', 'Other']
      }
    ];

    expect(request.request.method).toBe('GET');
    request.flush(presets);

    await expect(requestPromise).resolves.toEqual(presets);
  });

  it('creates a site with its selected preset and shared media categories', async () => {
    const create = service.createSite({
      name: 'Apartment 42',
      address: '42 Main Street',
      managerId: 'manager-1',
      startDate: '2026-03-03',
      endDate: null,
      status: 'Planning',
      mediaPolicyPreset: 'Custom',
      mediaCategories: ['HVAC Controls', 'Other']
    });
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/sites'));

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual({
      name: 'Apartment 42',
      address: '42 Main Street',
      managerId: 'manager-1',
      startDate: '2026-03-03',
      endDate: null,
      status: 'Planning',
      mediaPolicyPreset: 'Custom',
      mediaCategories: ['HVAC Controls', 'Other']
    });
    request.flush({ id: 'site-42' });

    await expect(create).resolves.toEqual({ id: 'site-42' });
  });

  it('updates a site with category additions at the route identifier', async () => {
    const update = service.updateSite({
      id: 'site-42',
      name: 'Apartment 42',
      address: '42 Main Street',
      managerId: 'manager-1',
      startDate: '2026-03-03',
      endDate: null,
      status: 'Planning',
      mediaPolicyPreset: 'Custom',
      mediaCategoriesToAdd: ['Access Control']
    });
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/dashboard/sites/site-42'));

    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual({
      id: 'site-42',
      name: 'Apartment 42',
      address: '42 Main Street',
      managerId: 'manager-1',
      startDate: '2026-03-03',
      endDate: null,
      status: 'Planning',
      mediaPolicyPreset: 'Custom',
      mediaCategoriesToAdd: ['Access Control']
    });
    request.flush(null);

    await expect(update).resolves.toBeNull();
  });
});
