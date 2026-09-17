import { DashboardCompanyPersonLookup } from '../../persons/models/dashboard-company-person-lookup.model';

export interface DashboardRetailer {
  id: string;
  displayName: string;
  companyPersonId: string;
  companyDisplayName: string;
  baseWebsiteUrl: string;
  websiteHost: string;
  isActive: boolean;
}

export interface DashboardRetailerDetails {
  id: string;
  displayName: string;
  companyPerson: DashboardCompanyPersonLookup;
  baseWebsiteUrl: string;
  websiteHost: string;
  notes: string | null;
  isActive: boolean;
}

export interface DashboardRetailerLookup {
  id: string;
  displayName: string;
  baseWebsiteUrl: string;
  websiteHost: string;
}

export interface DashboardRetailersResponse {
  items: readonly DashboardRetailer[];
  filteredCount: number;
  totalCount: number;
}

export interface CreateDashboardRetailerRequest {
  displayName: string;
  companyPersonId: string;
  baseWebsiteUrl: string;
  notes: string | null;
}

export interface UpdateDashboardRetailerRequest
  extends CreateDashboardRetailerRequest {
  id: string;
}
