import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { buildApiUrl } from '../../../core/api/api-url';
import { ActivityCatalogService } from './activity-catalog.service';

describe('ActivityCatalogService', () => {
  let service: ActivityCatalogService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ActivityCatalogService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(ActivityCatalogService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads the catalog and reads activity details from their API contracts', async () => {
    const catalog = service.catalogQuery.refetch();
    const catalogRequest = httpTesting.expectOne(buildApiUrl('/dashboard/activity-catalog'));
    expect(catalogRequest.request.method).toBe('GET');
    catalogRequest.flush([]);
    await catalog;

    const details = service.getActivity('activity-1');
    const detailRequest = httpTesting.expectOne(buildApiUrl('/activities/activity-1'));
    expect(detailRequest.request.method).toBe('GET');
    detailRequest.flush({ id: 'activity-1', numberId: 1, name: 'Inspection', description: null, status: 'Active', parentFolderId: null, sortOrder: 0 });
    await expect(details).resolves.toMatchObject({ id: 'activity-1', numberId: 1 });
  });

  it('uses all folder mutation contracts and invalidates the catalog after each success', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');

    await complete(service.createFolder({ name: 'Safety', parentFolderId: null }), 'POST', '/activity-folders', { id: 'folder-1' });
    await complete(service.renameFolder('folder-1', 'Renamed'), 'PUT', '/activity-folders/folder-1');
    await complete(service.moveFolder('folder-1', { targetParentFolderId: null, targetIndex: 0 }), 'PATCH', '/activity-folders/folder-1/move');
    await complete(service.deleteFolder('folder-1'), 'DELETE', '/activity-folders/folder-1');

    expect(invalidate).toHaveBeenCalledTimes(4);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['activity-catalog'] });
  });

  it('uses all activity mutation contracts and invalidates the catalog after each success', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');

    await complete(service.createActivity({ name: 'Inspection', description: null, parentFolderId: null }), 'POST', '/activities', { id: 'activity-1' });
    await complete(service.updateActivity('activity-1', { name: 'Updated', description: 'Daily' }), 'PUT', '/activities/activity-1');
    await complete(service.moveActivity('activity-1', { targetParentFolderId: 'folder-1', targetIndex: 1 }), 'PATCH', '/activities/activity-1/move');
    await complete(service.archiveActivity('activity-1'), 'PATCH', '/activities/activity-1/archive');
    await complete(service.restoreActivity('activity-1'), 'PATCH', '/activities/activity-1/restore');
    await complete(service.deleteActivity('activity-1'), 'DELETE', '/activities/activity-1');

    expect(invalidate).toHaveBeenCalledTimes(6);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['activity-catalog'] });
  });

  async function complete(
    operation: Promise<unknown>,
    method: string,
    path: string,
    response: unknown = null
  ): Promise<void> {
    await Promise.resolve();
    const request = httpTesting.expectOne(buildApiUrl(path));
    expect(request.request.method).toBe(method);
    request.flush(response as object | null);
    await expect(operation).resolves.toEqual(response);
  }
});
