import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { vi } from 'vitest';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardProductsService } from './dashboard-products.service';

describe('DashboardProductsService', () => {
  let service: DashboardProductsService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        DashboardProductsService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(DashboardProductsService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads dashboard products with normalized table filters and sorting', async () => {
    service.setTableState({
      overallRowsTotal: 0, filteredRowsTotal: 0, page: { pageIndex: 1, pageSize: 100 },
      sort: { active: 'title', direction: 'asc' }, filters: {}, draftFilters: {},
      appliedFilters: { title: '  Cordless Drill  ', brand: ' ' }, exportableColumns: []
    });

    const refresh = service.dashboardProductsQuery.refetch();
    const request = httpTesting.expectOne((candidate) => candidate.url === buildApiUrl('/dashboard/products'));

    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('pageIndex')).toBe('1');
    expect(request.request.params.get('pageSize')).toBe('100');
    expect(request.request.params.get('sortActive')).toBe('title');
    expect(request.request.params.get('sortDirection')).toBe('asc');
    expect(request.request.params.get('title')).toBe('cordless drill');
    expect(request.request.params.has('brand')).toBe(false);
    request.flush({ items: [], filteredCount: 0, totalCount: 0 });
    await refresh;
  });

  it('uses the product resource contracts and invalidates catalog lists after writes', async () => {
    const queryClient = TestBed.inject(QueryClient);
    const invalidate = vi.spyOn(queryClient, 'invalidateQueries');
    const create = service.createProduct(productRequest());
    await Promise.resolve();
    const createRequest = httpTesting.expectOne(buildApiUrl('/products'));
    expect(createRequest.request.method).toBe('POST');
    expect(createRequest.request.body).toEqual(productRequest());
    createRequest.flush({ id: 'product-1' });
    await expect(create).resolves.toEqual({ id: 'product-1' });
    expect(invalidate).toHaveBeenCalledWith({ queryKey: ['products', 'dashboard'] });

    const update = service.updateProduct({ id: 'product-1', ...productRequest(), title: 'Updated Drill' });
    await Promise.resolve();
    const updateRequest = httpTesting.expectOne(buildApiUrl('/products/product-1'));
    expect(updateRequest.request.method).toBe('PUT');
    expect(updateRequest.request.body).toEqual({ id: 'product-1', ...productRequest(), title: 'Updated Drill' });
    updateRequest.flush(null);
    await expect(update).resolves.toBeNull();
  });

  it('loads details, searches nonblank terms, and skips blank searches', async () => {
    const detail = service.getProductById('product-1');
    const detailRequest = httpTesting.expectOne(buildApiUrl('/products/product-1'));
    detailRequest.flush({ id: 'product-1', numberId: 1, title: 'Drill' });
    await expect(detail).resolves.toMatchObject({ id: 'product-1' });

    await expect(service.searchProducts('   ')).resolves.toEqual([]);
    const search = service.searchProducts('  drill  ');
    const searchRequest = httpTesting.expectOne(buildApiUrl('/dashboard/products/search?searchTerm=drill'));
    expect(searchRequest.request.method).toBe('GET');
    searchRequest.flush([{ id: 'product-1', numberId: 1, title: 'Drill' }]);
    await expect(search).resolves.toHaveLength(1);
  });
});

function productRequest() {
  return {
    title: 'Cordless Drill', description: null, brand: 'Bosch', model: 'GSB 18V',
    packageQuantity: 1, packageUnit: 'piece' as const, category: 'tools-equipment' as const,
    status: 'Active' as const, primarySearchPhrase: null, alternativeSearchPhrases: [],
    requiredKeywords: [], excludedKeywords: []
  };
}
