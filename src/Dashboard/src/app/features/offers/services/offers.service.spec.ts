import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { OffersService } from './offers.service';

describe('OffersService requirement Product selection', () => {
  let service: OffersService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        OffersService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(OffersService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpTesting.verify());

  it('loads offer-scoped Product candidates with an optional server search term', async () => {
    const candidates = service.getRequirementProductCandidates(
      'site-1', 'offer-1', 'requirement-1', '  mapei  '
    );
    const request = httpTesting.expectOne(
      buildApiUrl('/sites/site-1/offers/offer-1/requirements/requirement-1/product-candidates?searchTerm=mapei')
    );

    expect(request.request.method).toBe('GET');
    request.flush([{ id: 'product-1', numberId: 102, title: 'Mapei Keraflex' }]);

    await expect(candidates).resolves.toMatchObject([
      { id: 'product-1', numberId: 102 }
    ]);
  });

  it('selects and clears the exact Product through the requirement-scoped routes', async () => {
    const select = service.selectRequirementProduct({
      siteId: 'site-1', offerId: 'offer-1', offerRequirementId: 'requirement-1', productId: 'product-1'
    });
    await Promise.resolve();
    const selectRequest = httpTesting.expectOne(
      buildApiUrl('/sites/site-1/offers/offer-1/requirements/requirement-1/product-line')
    );
    expect(selectRequest.request.method).toBe('PUT');
    expect(selectRequest.request.body).toEqual({ productId: 'product-1' });
    selectRequest.flush({ id: 'line-1', offerProductRequirementId: 'requirement-1', productId: 'product-1' });
    await expect(select).resolves.toMatchObject({ id: 'line-1' });

    const clear = service.clearRequirementProduct({
      siteId: 'site-1', offerId: 'offer-1', offerRequirementId: 'requirement-1'
    });
    await Promise.resolve();
    const clearRequest = httpTesting.expectOne(
      buildApiUrl('/sites/site-1/offers/offer-1/requirements/requirement-1/product-line')
    );
    expect(clearRequest.request.method).toBe('DELETE');
    clearRequest.flush(null);
    await expect(clear).resolves.toBeNull();
  });
});
