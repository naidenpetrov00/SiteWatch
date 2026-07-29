import { AuthContextType } from "@/types/identity";
import RoleGate from "@/features/auth/components/RoleGate/RoleGate";
import RoleRouteGuard from "@/features/auth/components/RoleRouteGuard/RoleRouteGuard";
import { Text, View } from "react-native";
import { USER_ROLES } from "@/types/authorization";
import { act, create, ReactTestRenderer } from "react-test-renderer";
import { useAuth } from "@/store/auth_context";

jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));
jest.mock("expo-router", () => ({
  Redirect: ({ href }: { href: string }) =>
    require("react").createElement("redirect", { href }),
}));

const mockedUseAuth = jest.mocked(useAuth);

const authFor = (
  roles: AuthContextType["roles"],
  isAuthenticated = true,
): AuthContextType => ({
  user: isAuthenticated
    ? { id: "user", username: "user", email: "user@example.test", roles }
    : null,
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
  let renderer: ReactTestRenderer;

  afterEach(() => {
    act(() => renderer?.unmount());
    jest.clearAllMocks();
  });

  it("renders text, actions, sections, and components for any allowed role", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.worker]));

    act(() => {
      renderer = create(
        <RoleGate allowedRoles={[USER_ROLES.administrator, USER_ROLES.worker]}>
          <View testID="protected-section">
            <Text>Worker notice</Text>
            <Text accessibilityRole="button">Worker action</Text>
          </View>
        </RoleGate>,
      );
    });

    expect(renderer.root.findByProps({ testID: "protected-section" })).toBeTruthy();
    expect(renderer.root.findAllByType(Text)).toHaveLength(2);
  });

  it("renders a fallback when the user lacks every allowed role", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.client]));

    act(() => {
      renderer = create(
        <RoleGate
          allowedRoles={[USER_ROLES.administrator, USER_ROLES.worker]}
          fallback={<Text>No permission</Text>}
        >
          <Text>Protected component</Text>
        </RoleGate>,
      );
    });

    expect(renderer.root.findByType(Text).props.children).toBe("No permission");
  });

  it.each([
    [false, [] as const, "/SignIn"],
    [true, [USER_ROLES.client] as const, "/AccessDenied"],
    [true, [USER_ROLES.worker] as const, "/AccessDenied"],
  ])(
    "redirects authenticated=%s roles=%j to %s",
    (isAuthenticated, roles, expectedRoute) => {
      mockedUseAuth.mockReturnValue(authFor(roles, isAuthenticated));

      act(() => {
        renderer = create(
          <RoleRouteGuard allowedRoles={[USER_ROLES.administrator]}>
            <Text>Administrator page</Text>
          </RoleRouteGuard>,
        );
      });

      expect(renderer.root.findByType("redirect").props.href).toBe(expectedRoute);
    },
  );

  it("renders the protected page for an Administrator", () => {
    mockedUseAuth.mockReturnValue(authFor([USER_ROLES.administrator]));

    act(() => {
      renderer = create(
        <RoleRouteGuard allowedRoles={[USER_ROLES.administrator]}>
          <Text>Administrator page</Text>
        </RoleRouteGuard>,
      );
    });

    expect(renderer.root.findByType(Text).props.children).toBe("Administrator page");
  });
});
