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
  user: User;
}

export interface IdentityResultWithUserToken
  extends IdentityResultWithUser,
    IdentityResultWithToken {}
import { User } from "@/types/identity";
