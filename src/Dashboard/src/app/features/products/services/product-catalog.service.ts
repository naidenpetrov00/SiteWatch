import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  CategoryCreatedResponse,
  ProductCatalogCategory,
  ProductCatalogPage,
  ProductCatalogPageRequest,
  ProductCatalogProductType,
  ProductTypeCreatedResponse,
  ProductTypePageRequest,
  SaveCategoryRequest,
  SaveProductTypeRequest
} from '../models/product-catalog.models';

@Injectable({ providedIn: 'root' })
export class ProductCatalogService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);

  getCategories(request: ProductCatalogPageRequest = {}): Promise<ProductCatalogPage<ProductCatalogCategory>> {
    return firstValueFrom(this.http.get<ProductCatalogPage<ProductCatalogCategory>>(
      buildApiUrl('/categories'), { params: this.pageParams(request) }
    ));
  }

  getCategoryById(categoryId: string): Promise<ProductCatalogCategory> {
    return firstValueFrom(this.http.get<ProductCatalogCategory>(buildApiUrl(`/categories/${categoryId}`)));
  }

  async createCategory(request: SaveCategoryRequest): Promise<ProductCatalogCategory> {
    const created = await firstValueFrom(this.http.post<CategoryCreatedResponse>(buildApiUrl('/categories'), request));
    const category = await this.getCategoryById(created.id);
    await this.invalidateCatalog();
    return category;
  }

  async updateCategory(categoryId: string, request: SaveCategoryRequest): Promise<ProductCatalogCategory> {
    await firstValueFrom(this.http.put<void>(buildApiUrl(`/categories/${categoryId}`), request));
    const category = await this.getCategoryById(categoryId);
    await this.invalidateCatalogAndProductPresentation();
    return category;
  }

  async deleteCategory(categoryId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(`/categories/${categoryId}`)));
    await this.invalidateCatalog();
  }

  getProductTypes(request: ProductTypePageRequest = {}): Promise<ProductCatalogPage<ProductCatalogProductType>> {
    let params = this.pageParams(request);
    if (request.categoryId) params = params.set('categoryId', request.categoryId);
    return firstValueFrom(this.http.get<ProductCatalogPage<ProductCatalogProductType>>(
      buildApiUrl('/product-types'), { params }
    ));
  }

  getProductTypeById(productTypeId: string): Promise<ProductCatalogProductType> {
    return firstValueFrom(this.http.get<ProductCatalogProductType>(buildApiUrl(`/product-types/${productTypeId}`)));
  }

  async createProductType(request: SaveProductTypeRequest): Promise<ProductCatalogProductType> {
    const created = await firstValueFrom(this.http.post<ProductTypeCreatedResponse>(buildApiUrl('/product-types'), request));
    const productType = await this.getProductTypeById(created.id);
    await this.invalidateCatalog();
    return productType;
  }

  async updateProductType(productTypeId: string, request: SaveProductTypeRequest): Promise<ProductCatalogProductType> {
    await firstValueFrom(this.http.put<void>(buildApiUrl(`/product-types/${productTypeId}`), request));
    const productType = await this.getProductTypeById(productTypeId);
    await this.invalidateCatalogAndProductPresentation();
    return productType;
  }

  async deleteProductType(productTypeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(`/product-types/${productTypeId}`)));
    await this.invalidateCatalog();
  }

  private pageParams(request: ProductCatalogPageRequest): HttpParams {
    let params = new HttpParams().set('pageIndex', request.pageIndex ?? 0).set('pageSize', request.pageSize ?? 100);
    const searchTerm = request.searchTerm?.trim();
    if (searchTerm) params = params.set('searchTerm', searchTerm);
    return params;
  }

  private async invalidateCatalog(): Promise<void> {
    await this.queryClient.invalidateQueries({ queryKey: ['product-catalog'] });
  }

  private async invalidateCatalogAndProductPresentation(): Promise<void> {
    await Promise.all([
      this.invalidateCatalog(),
      this.queryClient.invalidateQueries({ queryKey: ['products', 'dashboard'] }),
      this.queryClient.invalidateQueries({ queryKey: ['products', 'detail'] })
    ]);
  }
}
