import { AuthenticationData, IdentityActionTypes } from "@/types/identity";

const initalState: AuthenticationData = { token: "", isAuthenticated: false };

export const identityReducer = (state = initalState, action: string) => {
  switch (action) {
    case IdentityActionTypes.ADD_TOKEN:
      return {};
    case IdentityActionTypes.REMOVE_TOKEN:
      break;

    default:
      break;
  }
};
