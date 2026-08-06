import { AUTHENTICATED_ROUTES, UserRole } from "@/types/authorization";
import { Redirect } from "expo-router";
import { ReactNode } from "react";
import { useAuth } from "@/store/auth_context";

type RoleRouteGuardProps = {
  allowedRoles: readonly UserRole[];
  children: ReactNode;
};

const RoleRouteGuard = ({ allowedRoles, children }: RoleRouteGuardProps) => {
  const { isAuthenticated, hasAnyRole } = useAuth();

  if (!isAuthenticated) return <Redirect href="/SignIn" />;
  if (!hasAnyRole(allowedRoles)) {
    return <Redirect href={AUTHENTICATED_ROUTES.accessDenied} />;
  }

  return children;
};

export default RoleRouteGuard;
