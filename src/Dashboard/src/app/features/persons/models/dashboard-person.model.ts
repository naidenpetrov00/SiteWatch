export interface DashboardPerson {
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
}
