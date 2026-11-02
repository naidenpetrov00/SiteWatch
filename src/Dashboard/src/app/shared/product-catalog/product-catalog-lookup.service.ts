import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../core/api/api-url';
import {
  ProductCatalogCategory,
  ProductCatalogPage,
  ProductCatalogPageRequest,
  ProductCatalogProductType,
  ProductTypePageRequest
} from './product-catalog.models';

@Injectable({ providedIn: 'root' })
export class ProductCatalogLookupService {
  private readonly http = inject(HttpClient);

  getCategories(
    request: ProductCatalogPageRequest = {}
  ): Promise<ProductCatalogPage<ProductCatalogCategory>> {
    return firstValueFrom(
      this.http.get<ProductCatalogPage<ProductCatalogCategory>>(
        buildApiUrl('/categories'),
        { params: this.pageParams(request) }
      )
    );
  }

  getCategoryById(categoryId: string): Promise<ProductCatalogCategory> {
    return firstValueFrom(
      this.http.get<ProductCatalogCategory>(buildApiUrl(`/categories/${categoryId}`))
    );
  }

  getProductTypes(
    request: ProductTypePageRequest = {}
  ): Promise<ProductCatalogPage<ProductCatalogProductType>> {
    let params = this.pageParams(request);
    if (request.categoryId) params = params.set('categoryId', request.categoryId);
    return firstValueFrom(
      this.http.get<ProductCatalogPage<ProductCatalogProductType>>(
        buildApiUrl('/product-types'),
        { params }
      )
    );
  }

  getProductTypeById(productTypeId: string): Promise<ProductCatalogProductType> {
    return firstValueFrom(
      this.http.get<ProductCatalogProductType>(
        buildApiUrl(`/product-types/${productTypeId}`)
      )
    );
  }

  private pageParams(request: ProductCatalogPageRequest): HttpParams {
    let params = new HttpParams()
      .set('pageIndex', request.pageIndex ?? 0)
      .set('pageSize', request.pageSize ?? 100);
    const searchTerm = request.searchTerm?.trim();
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return params;
  }
}
