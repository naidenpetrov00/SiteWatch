export interface UpdateDashboardSiteRequest {
  id: string;
  name: string;
  address: string;
  mediaCategoriesToAdd: readonly string[];
}
