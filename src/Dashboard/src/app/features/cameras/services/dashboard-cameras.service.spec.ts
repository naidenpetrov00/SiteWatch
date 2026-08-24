import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardCamerasService } from './dashboard-cameras.service';

describe('DashboardCamerasService', () => {
  let service: DashboardCamerasService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardCamerasService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardCamerasService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads dashboard cameras with normalized table filters', async () => {
    service.setTableState({
      overallRowsTotal: 0,
      filteredRowsTotal: 0,
      page: { pageIndex: 1, pageSize: 100 },
      sort: { active: 'name', direction: 'asc' },
      filters: {},
      draftFilters: {},
      appliedFilters: { name: '  North Gate  ', brand: ' ' },
      exportableColumns: []
    });

    const refresh = service.dashboardCamerasQuery.refetch();
    const request = httpTesting.expectOne((candidate) => candidate.url === buildApiUrl('/dashboard/cameras'));

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('pageIndex')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('100');
    expect(request.request.params.get('sortActive')).toBe('name');
    expect(request.request.params.get('sortDirection')).toBe('asc');
    expect(request.request.params.get('name')).toBe('north gate');
    expect(request.request.params.has('brand')).toBe(false);
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });

    await refresh;
  });

  it('creates a camera with the dashboard request contract', async () => {
    const create = service.createCamera(cameraRequest());
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl('/cameras'));

    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(cameraRequest());
    request.flush({ id: 'camera-42' });

    await expect(create).resolves.toEqual({ id: 'camera-42' });
  });

  it('updates and deletes camera resources at their route identifiers', async () => {
    const update = service.updateCamera({ id: 'camera-42', ...cameraRequest() });
    await Promise.resolve();
    const updateRequest = httpTesting.expectOne(buildApiUrl('/cameras/camera-42'));
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body).toEqual(cameraRequest());
    updateRequest.flush(null);
    await expect(update).resolves.toBeNull();

    const remove = service.deleteCamera('camera-42');
    await Promise.resolve();
    const deleteRequest = httpTesting.expectOne(buildApiUrl('/cameras/camera-42'));
    expect(deleteRequest.request.method).toBe('DELETE');
    deleteRequest.flush(null);
    await expect(remove).resolves.toBeNull();
  });

  it('loads editable camera details from the dashboard route', async () => {
    const load = service.getCameraById('camera-42');
    const request = httpTesting.expectOne(buildApiUrl('/dashboard/cameras/camera-42'));
    const details = { id: 'camera-42', numberId: 42, name: 'North gate', brand: 'Dahua', model: 'IPC-HDW', username: null, password: null, ipAddress: null, rtspPort: 554, ptzPort: 443, protocol: 'Https', siteId: 'site-42', siteName: 'Head office' };

    expect(request.request.method).toBe('GET');
    request.flush(details);

    await expect(load).resolves.toEqual(details);
  });
});

function cameraRequest() {
  return {
    name: 'North gate', brand: 'Dahua', model: 'IPC-HDW', username: 'operator', password: 'secret', ipAddress: '192.0.2.10', rtspPort: 554, ptzPort: 443, protocol: 'Https', siteId: 'site-42'
  };
}
