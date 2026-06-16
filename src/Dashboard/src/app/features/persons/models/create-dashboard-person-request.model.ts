export type CreateDashboardPersonType = 0 | 1;
export type CreateDashboardContactType = 0 | 1 | 2 | 3;

export interface CreateDashboardPersonAddressRequest {
  addressLine: string;
  additionalLine?: string;
  city?: string;
  postalCode?: string;
  country?: string;
  details?: string;
  isPrimary: boolean;
  isActive: boolean;
}

export interface CreateDashboardPersonContactRequest {
  contactType: CreateDashboardContactType;
  value: string;
  details?: string;
  isPrimary: boolean;
  isActive: boolean;
}

export interface CreateDashboardPersonBankAccountRequest {
  iban: string;
  bic?: string;
  bankName?: string;
  details?: string;
  isPrimary: boolean;
  isActive: boolean;
}

export interface CreateDashboardPersonRequest {
  type: CreateDashboardPersonType;
  firstName?: string;
  middleName?: string;
  lastName?: string;
  companyName?: string;
  egn?: string;
  eik?: string;
  vatNumber: string;
  addresses?: CreateDashboardPersonAddressRequest[];
  contacts?: CreateDashboardPersonContactRequest[];
  bankAccounts?: CreateDashboardPersonBankAccountRequest[];
}
