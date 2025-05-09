export interface AuthenticationData {
  token: string;
  isAuthenticated: boolean;
}

export enum IdentityActionTypes {
  ADD_TOKEN = "AddToken",
  REMOVE_TOKEN = "RemoveToken",
}
