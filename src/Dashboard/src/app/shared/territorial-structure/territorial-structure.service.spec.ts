import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { buildApiUrl } from '../../core/api/api-url';
import { TerritorialStructureService } from './territorial-structure.service';

describe('TerritorialStructureService', () => {
  let service: TerritorialStructureService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        TerritorialStructureService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(TerritorialStructureService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('searches a territorial level with trimmed optional filters', async () => {
    const search = service.getEntries(1, 'municipality-1', '  sof  ', true);
    const request = httpTesting.expectOne((candidate) =>
      candidate.url === buildApiUrl('/territorial-structure')
      && candidate.params.get('kind') === '1'
      && candidate.params.get('parentId') === 'municipality-1'
      && candidate.params.get('searchTerm') === 'sof'
      && candidate.params.get('includeInactive') === 'true');
    expect(request.request.method).toBe('GET');
    request.flush([
      { id: 'settlement-1', name: 'Sofia', isActive: true, parentId: 'municipality-1' }
    ]);

    await expect(search).resolves.toEqual([
      { id: 'settlement-1', name: 'Sofia', isActive: true, parentId: 'municipality-1' }
    ]);
  });

  it('omits blank optional search parameters', async () => {
    const search = service.getEntries(0, null, '   ');
    const request = httpTesting.expectOne((candidate) =>
      candidate.url === buildApiUrl('/territorial-structure'));

    expect(request.request.params.get('kind')).toBe('0');
    expect(request.request.params.get('includeInactive')).toBe('false');
    expect(request.request.params.has('parentId')).toBe(false);
    expect(request.request.params.has('searchTerm')).toBe(false);
    request.flush([]);
    await expect(search).resolves.toEqual([]);
  });

  it('uses mutation contracts and invalidates territorial queries after success', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');

    await complete(
      service.create({ kind: 1, parentId: 'municipality-1', name: 'Sofia' }),
      'POST',
      '/territorial-structure',
      { kind: 1, parentId: 'municipality-1', name: 'Sofia' });
    await complete(
      service.update('settlement-1', { kind: 1, name: 'Sofia City', isActive: false }),
      'PUT',
      '/territorial-structure/settlement-1',
      { kind: 1, name: 'Sofia City', isActive: false });
    await complete(
      service.delete('settlement-1', 1),
      'DELETE',
      '/territorial-structure/settlement-1');

    expect(invalidate).toHaveBeenCalledTimes(3);
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['territorial-structure'] });
  });

  async function complete(
    operation: Promise<void>,
    method: string,
    path: string,
    body?: unknown
  ): Promise<void> {
    await Promise.resolve();
    const request = httpTesting.expectOne((candidate) => candidate.url === buildApiUrl(path));
    expect(request.request.method).toBe(method);
    if (body !== undefined) expect(request.request.body).toEqual(body);
    if (method === 'DELETE') expect(request.request.params.get('kind')).toBe('1');
    request.flush(null);
    await expect(operation).resolves.toBeUndefined();
  }
});
