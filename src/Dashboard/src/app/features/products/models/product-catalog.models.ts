export interface SaveCategoryRequest {
  name: string;
}

export interface SaveProductTypeRequest {
  name: string;
  categoryId: string;
  description?: string | null;
}

export interface SaveProductBrandRequest {
  name: string;
}

interface CreatedResponse {
  id: string;
}

export type CategoryCreatedResponse = CreatedResponse;
export type ProductTypeCreatedResponse = CreatedResponse;
export type ProductBrandCreatedResponse = CreatedResponse;
