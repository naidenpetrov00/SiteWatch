import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  ProductCatalogBrand,
  ProductCatalogCategory,
  ProductCatalogProductType
} from '../../../shared/product-catalog/product-catalog.models';
import { ProductCatalogLookupService } from '../../../shared/product-catalog/product-catalog-lookup.service';
import {
  CategoryCreatedResponse,
  ProductBrandCreatedResponse,
  ProductTypeCreatedResponse,
  SaveCategoryRequest,
  SaveProductBrandRequest,
  SaveProductTypeRequest
} from '../models/product-catalog.models';

@Injectable({ providedIn: 'root' })
export class ProductCatalogService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly catalogLookupService = inject(ProductCatalogLookupService);

  async createCategory(request: SaveCategoryRequest): Promise<ProductCatalogCategory> {
    const created = await firstValueFrom(this.http.post<CategoryCreatedResponse>(buildApiUrl('/categories'), request));
    const category = await this.catalogLookupService.getCategoryById(created.id);
    await this.invalidateCatalog();
    return category;
  }

  async updateCategory(categoryId: string, request: SaveCategoryRequest): Promise<ProductCatalogCategory> {
    await firstValueFrom(this.http.put<void>(buildApiUrl(`/categories/${categoryId}`), request));
    const category = await this.catalogLookupService.getCategoryById(categoryId);
    await this.invalidateCatalogAndProductPresentation();
    return category;
  }

  async deleteCategory(categoryId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(`/categories/${categoryId}`)));
    await this.invalidateCatalog();
  }

  async createProductType(request: SaveProductTypeRequest): Promise<ProductCatalogProductType> {
    const created = await firstValueFrom(this.http.post<ProductTypeCreatedResponse>(buildApiUrl('/product-types'), request));
    const productType = await this.catalogLookupService.getProductTypeById(created.id);
    await this.invalidateCatalog();
    return productType;
  }

  async updateProductType(productTypeId: string, request: SaveProductTypeRequest): Promise<ProductCatalogProductType> {
    await firstValueFrom(this.http.put<void>(buildApiUrl(`/product-types/${productTypeId}`), request));
    const productType = await this.catalogLookupService.getProductTypeById(productTypeId);
    await this.invalidateCatalogAndProductPresentation();
    return productType;
  }

  async deleteProductType(productTypeId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(`/product-types/${productTypeId}`)));
    await this.invalidateCatalog();
  }

  async createProductBrand(request: SaveProductBrandRequest): Promise<ProductCatalogBrand> {
    const created = await firstValueFrom(
      this.http.post<ProductBrandCreatedResponse>(buildApiUrl('/product-brands'), request)
    );
    const brand = await this.catalogLookupService.getProductBrandById(created.id);
    await this.invalidateCatalog();
    return brand;
  }

  async updateProductBrand(
    productBrandId: string,
    request: SaveProductBrandRequest
  ): Promise<ProductCatalogBrand> {
    await firstValueFrom(
      this.http.put<void>(buildApiUrl(`/product-brands/${productBrandId}`), request)
    );
    const brand = await this.catalogLookupService.getProductBrandById(productBrandId);
    await this.invalidateCatalogAndProductPresentation();
    return brand;
  }

  async deleteProductBrand(productBrandId: string): Promise<void> {
    await firstValueFrom(
      this.http.delete<void>(buildApiUrl(`/product-brands/${productBrandId}`))
    );
    await this.invalidateCatalog();
  }

  private async invalidateCatalog(): Promise<void> {
    await this.queryClient.invalidateQueries({ queryKey: ['product-catalog'] });
  }

  private async invalidateCatalogAndProductPresentation(): Promise<void> {
    await Promise.all([
      this.invalidateCatalog(),
      this.queryClient.invalidateQueries({ queryKey: ['products', 'dashboard'] }),
      this.queryClient.invalidateQueries({ queryKey: ['products', 'detail'] }),
      this.queryClient.invalidateQueries({ queryKey: ['activity-catalog'] })
    ]);
  }
}
