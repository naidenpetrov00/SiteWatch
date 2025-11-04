export interface IdentityResult {
  result: {
    errors: [];
    success: boolean;
  };
}

export interface IdentityResultWithToken extends IdentityResult {
  token: string;
}

export interface IdentityResultWithUser extends IdentityResult {
  user: {
    id: string;
    username: string;
    email: string;
  };
}

export interface IdentityResultWithUserToken
  extends IdentityResultWithUser,
    IdentityResultWithToken {}
