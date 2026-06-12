export interface DashboardUser {
  id: string | null;
  username: string | null;
  email: string | null;
  phoneNumber: string | null;
  isEmailConfirmed: boolean;
  isPhoneNumberConfirmed: boolean;
  lastLoginAt: string | null;
}
