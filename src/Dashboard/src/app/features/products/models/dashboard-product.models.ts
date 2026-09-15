export const PRODUCT_STATUSES = [
  'Active',
  'Unavailable',
  'Discontinued'
] as const;

export type ProductStatus = (typeof PRODUCT_STATUSES)[number];

export const PRODUCT_CATEGORY_OPTIONS = [
  { value: 'building-construction', label: 'Building & Construction' },
  { value: 'tools-equipment', label: 'Tools & Equipment' },
  { value: 'electrical-lighting', label: 'Electrical & Lighting' },
  { value: 'plumbing-hvac', label: 'Plumbing & HVAC' },
  { value: 'hardware-fasteners', label: 'Hardware & Fasteners' },
  { value: 'paints-finishes', label: 'Paints & Finishes' },
  { value: 'safety-security', label: 'Safety & Security' },
  { value: 'cleaning-maintenance', label: 'Cleaning & Maintenance' },
  { value: 'fixtures-appliances', label: 'Fixtures & Appliances' },
  { value: 'outdoor-landscaping', label: 'Outdoor & Landscaping' },
  { value: 'office-general-supplies', label: 'Office & General Supplies' },
  { value: 'other', label: 'Other' }
] as const;

export type ProductCategory =
  (typeof PRODUCT_CATEGORY_OPTIONS)[number]['value'];

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
  category: ProductCategory;
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
  category: ProductCategory;
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
  category: ProductCategory;
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
