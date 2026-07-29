import { ReactNode } from "react";
import { UserRole } from "@/types/authorization";
import { useAuth } from "@/store/auth_context";

type RoleGateProps = {
  allowedRoles: readonly UserRole[];
  children: ReactNode;
  fallback?: ReactNode;
};

const RoleGate = ({
  allowedRoles,
  children,
  fallback = null,
}: RoleGateProps) => {
  const { hasAnyRole } = useAuth();

  return hasAnyRole(allowedRoles) ? children : fallback;
};

export default RoleGate;
