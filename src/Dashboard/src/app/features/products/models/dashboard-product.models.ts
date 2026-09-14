export const PRODUCT_STATUSES = [
  'Active',
  'Unavailable',
  'Discontinued'
] as const;

export type ProductStatus = (typeof PRODUCT_STATUSES)[number];

export const PRODUCT_PACKAGE_UNIT_OPTIONS = [
  { value: 'piece', label: 'Piece' },
  { value: 'pack', label: 'Pack' },
  { value: 'box', label: 'Box' },
  { value: 'set', label: 'Set' },
  { value: 'kg', label: 'Kilogram (kg)' },
  { value: 'g', label: 'Gram (g)' },
  { value: 'l', label: 'Liter (l)' },
  { value: 'ml', label: 'Milliliter (ml)' },
  { value: 'm', label: 'Meter (m)' },
  { value: 'cm', label: 'Centimeter (cm)' },
  { value: 'm2', label: 'Square meter (m²)' },
  { value: 'm3', label: 'Cubic meter (m³)' },
  { value: 'roll', label: 'Roll' },
  { value: 'bag', label: 'Bag' }
] as const;

export type ProductPackageUnit =
  (typeof PRODUCT_PACKAGE_UNIT_OPTIONS)[number]['value'];

export interface DashboardProduct {
  id: string;
  numberId: number;
  title: string;
  category: string;
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: ProductPackageUnit | null;
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
  packageUnit: ProductPackageUnit | null;
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
  packageUnit: ProductPackageUnit | null;
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
