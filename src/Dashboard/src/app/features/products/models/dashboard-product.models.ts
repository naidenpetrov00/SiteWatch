export const PRODUCT_STATUSES = [
  'Active',
  'Unavailable',
  'Discontinued'
] as const;

export type ProductStatus = (typeof PRODUCT_STATUSES)[number];

export interface DashboardProduct {
  id: string;
  numberId: number;
  title: string;
  category: string;
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: string | null;
  status: ProductStatus;
}

export interface DashboardProductDetails extends DashboardProduct {
  description: string | null;
  primarySearchPhrase: string | null;
  effectivePrimarySearchPhrase: string;
  alternativeSearchPhrases: readonly string[];
  requiredKeywords: readonly string[];
  excludedKeywords: readonly string[];
}

export interface DashboardProductLookup {
  id: string;
  numberId: number;
  title: string;
  category: string;
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: string | null;
}

export interface DashboardProductsResponse {
  items: readonly DashboardProduct[];
  filteredCount: number;
  totalCount: number;
}

export interface CreateDashboardProductRequest {
  title: string;
  description: string | null;
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: string | null;
  category: string;
  status: ProductStatus;
  primarySearchPhrase: string | null;
  alternativeSearchPhrases: readonly string[];
  requiredKeywords: readonly string[];
  excludedKeywords: readonly string[];
}

export interface UpdateDashboardProductRequest
  extends CreateDashboardProductRequest {
  id: string;
}
