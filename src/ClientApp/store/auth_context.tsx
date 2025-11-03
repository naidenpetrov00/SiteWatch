import { AuthAction, AuthContextType, AuthState, User } from "@/types/identity";
import { ReactNode, createContext, useContext, useReducer } from "react";

const initialState: AuthState = {
  user: null,
  accessToken: null,
  isAuthenticated: false,
};

const authReducer = (state: AuthState, action: AuthAction): AuthState => {
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

  const login = (user: User, token: string) =>
    dispatch({ type: "LOGIN", payload: { user, accessToken: token } });

  const logout = () => dispatch({ type: "LOGOUT" });

  const setToken = (token: string | null) =>
    dispatch({ type: "SET_TOKEN", payload: token });

  return (
    <AuthContext.Provider value={{ ...state, login, logout, setToken }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = (): AuthContextType => {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within <AuthProvider>");
  return ctx;
};
