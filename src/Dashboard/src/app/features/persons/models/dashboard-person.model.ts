export interface DashboardPerson {
  id: string;
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
}
