export interface AuthenticationData {
  token: string;
  isAuthenticated: boolean;
}

export enum IdentityActionTypes {
  ADD_TOKEN = "AddToken",
  REMOVE_TOKEN = "RemoveToken",
}

export type CreateAccountResponse = {
  token: string;
  email: string;
};

export interface User {
  id: string;
  username: string;
  email: string;
}

export interface AuthState {
  user: User | null;
  accessToken: string | null;
  isAuthenticated: boolean;
}

export type AuthAction =
  | { type: "LOGIN"; payload: { user: User; accessToken: string } }
  | { type: "LOGOUT" }
  | { type: "SET_TOKEN"; payload: string | null };

export interface AuthContextType extends AuthState {
  login: (user: User, token: string) => void;
  logout: () => void;
  setToken: (token: string | null) => void;
}
