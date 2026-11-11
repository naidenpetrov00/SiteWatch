export interface SaveCategoryRequest {
  name: string;
}

export interface SaveProductTypeRequest {
  name: string;
  categoryId: string;
  description?: string | null;
}

interface CreatedResponse {
  id: string;
}

export type CategoryCreatedResponse = CreatedResponse;
export type ProductTypeCreatedResponse = CreatedResponse;
