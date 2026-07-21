import {
  CreateDashboardPersonAddressRequest,
  CreateDashboardPersonBankAccountRequest,
  CreateDashboardPersonContactRequest
} from './create-dashboard-person-request.model';

export interface UpdateDashboardPersonRequest {
  id: string;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  companyName?: string;
  addresses?: CreateDashboardPersonAddressRequest[];
  contacts?: CreateDashboardPersonContactRequest[];
  bankAccounts?: CreateDashboardPersonBankAccountRequest[];
}
