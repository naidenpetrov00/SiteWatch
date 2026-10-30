export interface ProductCatalogPage<T> {
  items: readonly T[];
  filteredCount: number;
  totalCount: number;
}

export interface ProductCatalogCategory {
  id: string;
  name: string;
}

export interface ProductCatalogProductType {
  id: string;
  name: string;
  categoryId: string;
  categoryName: string;
}

export interface ProductCatalogPageRequest {
  searchTerm?: string;
  pageIndex?: number;
  pageSize?: number;
}

export interface ProductTypePageRequest extends ProductCatalogPageRequest {
  categoryId?: string | null;
}

export interface SaveCategoryRequest {
  name: string;
}

export interface SaveProductTypeRequest {
  name: string;
  categoryId: string;
}

interface CreatedResponse {
  id: string;
}

export type CategoryCreatedResponse = CreatedResponse;
export type ProductTypeCreatedResponse = CreatedResponse;
