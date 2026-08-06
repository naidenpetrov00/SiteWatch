import { AuthAction, AuthContextType, AuthState, User } from "@/types/identity";
import {
  ReactNode,
  createContext,
  useCallback,
  useContext,
  useReducer,
} from "react";
import {
  UserRole,
  hasAnyRole as userHasAnyRole,
  hasRole as userHasRole,
  parseUserRoles,
} from "@/types/authorization";
import { useQueryClient } from "@tanstack/react-query";

const EMPTY_ROLES: readonly UserRole[] = [];

const initialState: AuthState = {
  user: null,
  accessToken: null,
  isAuthenticated: false,
};

export const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case "LOGIN":
      return {
        ...state,
        user: action.payload.user,
        accessToken: action.payload.accessToken,
        isAuthenticated: true,
      };
    case "SET_TOKEN":
      return { ...state, accessToken: action.payload };
    case "LOGOUT":
      return { ...initialState };
    default:
      return state;
  }
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [state, dispatch] = useReducer(authReducer, initialState);
  const queryClient = useQueryClient();

  const login = useCallback(
    (user: User, token: string) => {
      const normalizedUser = { ...user, roles: parseUserRoles(user.roles) };
      const previousRoles = state.user?.roles ?? EMPTY_ROLES;
      const identityChanged =
        state.user?.id !== normalizedUser.id ||
        previousRoles.length !== normalizedUser.roles.length ||
        previousRoles.some((role) => !normalizedUser.roles.includes(role));

      if (identityChanged) queryClient.removeQueries();

      dispatch({
        type: "LOGIN",
        payload: { user: normalizedUser, accessToken: token },
      });
    },
    [queryClient, state.user],
  );

  const logout = useCallback(() => {
    queryClient.removeQueries();
    dispatch({ type: "LOGOUT" });
  }, [queryClient]);

  const setToken = (token: string | null) =>
    dispatch({ type: "SET_TOKEN", payload: token });

  const roles = state.user?.roles ?? EMPTY_ROLES;
  const hasRole = useCallback(
    (role: UserRole) => userHasRole(roles, role),
    [roles],
  );
  const hasAnyRole = useCallback(
    (allowedRoles: readonly UserRole[]) =>
      userHasAnyRole(roles, allowedRoles),
    [roles],
  );

  return (
    <AuthContext.Provider
      value={{ ...state, roles, login, logout, setToken, hasRole, hasAnyRole }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within <AuthProvider>");
  return ctx;
};
