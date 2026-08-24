export const USER_ROLES = {
  administrator: "Administrator",
  client: "Client",
  worker: "Worker",
} as const;

export type UserRole = (typeof USER_ROLES)[keyof typeof USER_ROLES];

const SUPPORTED_USER_ROLES = new Set<UserRole>(Object.values(USER_ROLES));

export const isUserRole = (value: unknown): value is UserRole =>
  typeof value === "string" && SUPPORTED_USER_ROLES.has(value as UserRole);

export const parseUserRoles = (value: unknown): readonly UserRole[] => {
  if (!Array.isArray(value)) return [];

  return [...new Set(value.filter(isUserRole))];
};

export const hasRole = (
  roles: readonly UserRole[],
  role: UserRole,
): boolean => roles.includes(role);

export const hasAnyRole = (
  roles: readonly UserRole[],
  allowedRoles: readonly UserRole[],
): boolean => allowedRoles.some((role) => hasRole(roles, role));

export const ACCESS_POLICIES = {
  currentApp: [
    USER_ROLES.administrator,
    USER_ROLES.worker,
    USER_ROLES.client,
  ],
  siteInvoices: [USER_ROLES.administrator, USER_ROLES.worker],
  siteMediaUpload: [USER_ROLES.administrator, USER_ROLES.worker],
  cameraManagement: [USER_ROLES.administrator, USER_ROLES.worker],
} as const satisfies Record<string, readonly UserRole[]>;

export const AUTHENTICATED_ROUTES = {
  currentApp: "/Sites",
  accessDenied: "/AccessDenied",
} as const;

export const getPostSignInRoute = (roles: unknown) =>
  hasAnyRole(parseUserRoles(roles), ACCESS_POLICIES.currentApp)
    ? AUTHENTICATED_ROUTES.currentApp
    : AUTHENTICATED_ROUTES.accessDenied;
