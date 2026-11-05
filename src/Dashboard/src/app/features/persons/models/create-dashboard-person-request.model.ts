export type CreateDashboardPersonType = 0 | 1;
export type CreateDashboardContactType = 0 | 1 | 2 | 3;

export interface CreateDashboardPersonAddressRequest {
  country: string;
  municipalityId?: string;
  settlementId?: string;
  districtId?: string;
  quarterId?: string;
  municipalityName?: string;
  settlementName?: string;
  districtName?: string;
  quarterName?: string;
  postalCode?: string;
  street?: string;
  number?: string;
  building?: string;
  entrance?: string;
  floor?: string;
  apartment?: string;
  addressDetails?: string;
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
  legalForm?: string;
  egn?: string;
  eik?: string;
  vatNumber: string;
  addresses?: CreateDashboardPersonAddressRequest[];
  contacts?: CreateDashboardPersonContactRequest[];
  bankAccounts?: CreateDashboardPersonBankAccountRequest[];
}
