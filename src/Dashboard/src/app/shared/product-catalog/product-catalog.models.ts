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
  description?: string | null;
  categoryId: string;
  categoryName: string;
}

export interface ProductCatalogBrand {
  id: string;
  name: string;
}

export interface ProductCatalogPageRequest {
  searchTerm?: string;
  pageIndex?: number;
  pageSize?: number;
}

export interface ProductTypePageRequest extends ProductCatalogPageRequest {
  categoryId?: string | null;
}
