import {
  CreateDashboardPersonAddressRequest,
  CreateDashboardPersonBankAccountRequest,
  CreateDashboardPersonContactRequest
} from './create-dashboard-person-request.model';

export interface DashboardPersonDetails {
  id: string;
  numberId: number;
  type: string;
  displayName: string;
  firstName: string | null;
  middleName: string | null;
  lastName: string | null;
  companyName: string | null;
  legalForm: string | null;
  egn: string | null;
  eik: string | null;
  vatNumber: string;
  addresses: readonly CreateDashboardPersonAddressRequest[];
  contacts: readonly CreateDashboardPersonContactRequest[];
  bankAccounts: readonly CreateDashboardPersonBankAccountRequest[];
  retailers: readonly DashboardPersonRetailerSummary[];
}

export interface DashboardPersonRetailerSummary {
  id: string;
  displayName: string;
  websiteHost: string;
  isActive: boolean;
}
