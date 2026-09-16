export type ActivityCatalogNodeKind = 'folder' | 'activity';
export type ActivityStatus = 'Active' | 'Archived';

export const ACTIVITY_MEASUREMENT_UNIT_OPTIONS = [
  { value: 'piece', label: 'Count / piece', symbol: 'piece' },
  { value: 'cm', label: 'Centimeter', symbol: 'cm' },
  { value: 'm', label: 'Meter', symbol: 'm' },
  { value: 'm2', label: 'Square meter', symbol: 'm²' },
  { value: 'm3', label: 'Cubic meter', symbol: 'm³' }
] as const;

export type ActivityMeasurementUnit =
  (typeof ACTIVITY_MEASUREMENT_UNIT_OPTIONS)[number]['value'];

export type ProductQuantityBehavior = 'proportional' | 'fixed';

interface ActivityCatalogNodeBase {
  id: string;
  kind: ActivityCatalogNodeKind;
  parentFolderId: string | null;
  name: string;
  sortOrder: number;
}

export interface ActivityFolderNode extends ActivityCatalogNodeBase {
  kind: 'folder';
  numberId: null;
  status: null;
}

export interface ActivityNode extends ActivityCatalogNodeBase {
  kind: 'activity';
  numberId: number;
  status: ActivityStatus;
}

export type ActivityCatalogNode = ActivityFolderNode | ActivityNode;

export type ActivityCatalogTreeNode = ActivityCatalogNode & {
  children: ActivityCatalogTreeNode[];
};

export interface ActivityDetails {
  id: string;
  numberId: number;
  name: string;
  description: string | null;
  status: ActivityStatus;
  parentFolderId: string | null;
  sortOrder: number;
  requirementSections: readonly ActivityRequirementSection[];
}

export interface ActivityRequirementSection {
  id: string;
  name: string | null;
  basisQuantity: number;
  measurementUnit: ActivityMeasurementUnit;
  sortOrder: number;
  productRequirements: readonly ActivityProductRequirement[];
}

export interface ActivityProductRequirement {
  id: string;
  productId: string;
  productNumberId: number;
  productTitle: string;
  productStatus: 'Active' | 'Unavailable' | 'Discontinued';
  brand: string | null;
  model: string | null;
  packageQuantity: number | null;
  packageUnit: string | null;
  quantity: number;
  isRequired: boolean;
  quantityBehavior: ProductQuantityBehavior;
  notes: string | null;
  sortOrder: number;
}

export interface CreateActivityFolderRequest {
  name: string;
  parentFolderId: string | null;
}

export interface CreateActivityRequest extends CreateActivityFolderRequest {
  description: string | null;
}

export interface UpdateActivityRequest {
  name: string;
  description: string | null;
}

export interface MoveCatalogNodeRequest {
  targetParentFolderId: string | null;
  targetIndex: number;
}

export interface UpsertActivityRequirementSectionRequest {
  name: string | null;
  basisQuantity: number;
  measurementUnit: ActivityMeasurementUnit;
}

export interface CreateActivityProductRequirementRequest {
  productId: string;
  quantity: number;
  isRequired: boolean;
  quantityBehavior: ProductQuantityBehavior;
  notes: string | null;
}

export type UpdateActivityProductRequirementRequest = Omit<
  CreateActivityProductRequirementRequest,
  'productId'
>;

export interface MoveActivityRequirementRequest {
  targetIndex: number;
}
