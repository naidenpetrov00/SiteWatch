import { Text, View } from "react-native";
import { render, screen } from "@testing-library/react-native";

import RoleGate from "@/features/auth/components/RoleGate/RoleGate";
import RoleRouteGuard from "@/features/auth/components/RoleRouteGuard/RoleRouteGuard";
import { useAuth } from "@/store/auth_context";
import { USER_ROLES } from "@/types/authorization";
import type { AuthContextType } from "@/types/identity";

jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));
jest.mock("expo-router", () => ({
  Redirect: ({ href }: { href: string }) => <Text accessibilityRole="link">{href}</Text>,
}));

const mockedUseAuth = jest.mocked(useAuth);

const authFor = (
  roles: AuthContextType["roles"],
  isAuthenticated = true,
): AuthContextType => ({
  user: isAuthenticated ? { id: "user", username: "user", email: "user@example.test", roles } : null,
  accessToken: isAuthenticated ? "token" : null,
  isAuthenticated,
  roles,
  login: jest.fn(),
  logout: jest.fn(),
  setToken: jest.fn(),
  hasRole: (role) => roles.includes(role),
  hasAnyRole: (allowedRoles) => allowedRoles.some((role) => roles.includes(role)),
});

describe("role-aware UI", () => {
  afterEach(() => jest.clearAllMocks());

  it("renders protected content for an allowed role", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.worker]));

    render(
      <RoleGate allowedRoles={[USER_ROLES.administrator, USER_ROLES.worker]}>
        <View accessibilityLabel="Protected section"><Text>Worker notice</Text></View>
      </RoleGate>,
    );

    expect(screen.getByLabelText("Protected section")).toBeOnTheScreen();
    expect(screen.getByText("Worker notice")).toBeOnTheScreen();
  });

  it("renders the supplied fallback for a disallowed role", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.client]));

    render(
      <RoleGate allowedRoles={[USER_ROLES.administrator]} fallback={<Text>No permission</Text>}>
        <Text>Protected component</Text>
      </RoleGate>,
    );

    expect(screen.getByText("No permission")).toBeOnTheScreen();
    expect(screen.queryByText("Protected component")).toBeNull();
  });

  it.each([
    [false, [] as const, "/SignIn"],
    [true, [USER_ROLES.client] as const, "/AccessDenied"],
  ])("redirects authenticated=%s roles=%j to %s", (isAuthenticated, roles, expectedRoute) => {
    mockedUseAuth.mockReturnValue(authFor(roles, isAuthenticated));

    render(
      <RoleRouteGuard allowedRoles={[USER_ROLES.administrator]}>
        <Text>Administrator page</Text>
      </RoleRouteGuard>,
    );

    expect(screen.getByRole("link")).toHaveTextContent(expectedRoute);
    expect(screen.queryByText("Administrator page")).toBeNull();
  });

  it("renders the protected route for an Administrator", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.administrator]));

    render(
      <RoleRouteGuard allowedRoles={[USER_ROLES.administrator]}>
        <Text>Administrator page</Text>
      </RoleRouteGuard>,
    );

    expect(screen.getByText("Administrator page")).toBeOnTheScreen();
  });
});
