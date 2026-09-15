export type ActivityCatalogNodeKind = 'folder' | 'activity';
export type ActivityStatus = 'Active' | 'Archived';

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
