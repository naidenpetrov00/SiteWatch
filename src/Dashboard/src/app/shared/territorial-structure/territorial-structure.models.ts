export type TerritorialEntryKind = 0 | 1 | 2 | 3;

export interface TerritorialEntry {
  id: string;
  name: string;
  isActive: boolean;
  parentId: string | null;
}

export interface SaveTerritorialEntryRequest {
  kind: TerritorialEntryKind;
  parentId: string | null;
  name: string;
}

export interface UpdateTerritorialEntryRequest {
  kind: TerritorialEntryKind;
  name: string;
  isActive: boolean;
}
