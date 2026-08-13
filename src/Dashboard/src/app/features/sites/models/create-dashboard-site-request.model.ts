export interface CreateDashboardSiteRequest {
  name: string;
  address: string;
  managerId: string;
  startDate: string;
  endDate: string | null;
  status: string;
  mediaPolicyPreset: string;
}
