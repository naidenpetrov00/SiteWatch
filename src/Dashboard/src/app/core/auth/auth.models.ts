export interface AuthResult {
  succeeded: boolean;
  errors: string[];
}

export interface DashboardSignInRequest {
  email: string;
  password: string;
}

export interface DashboardSignInResponse {
  result?: {
    succeeded: boolean;
    errors?: string[];
  };
  token?: string;
  errors?: string[];
}
