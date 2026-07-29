import { AuthContextType, User } from "@/types/identity";
import { AuthProvider, useAuth } from "@/store/auth_context";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { ReactNode } from "react";
import { USER_ROLES } from "@/types/authorization";
import { act, create, ReactTestRenderer } from "react-test-renderer";

describe("AuthProvider", () => {
  let auth!: AuthContextType;
  let queryClient: QueryClient;
  let renderer: ReactTestRenderer;

  const Probe = () => {
    auth = useAuth();
    return null;
  };

  const Wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>
      <AuthProvider>{children}</AuthProvider>
    </QueryClientProvider>
  );

  beforeEach(() => {
    queryClient = new QueryClient();
    act(() => {
      renderer = create(
        <Wrapper>
          <Probe />
        </Wrapper>,
      );
    });
  });

  afterEach(() => {
    act(() => renderer.unmount());
    queryClient.clear();
  });

  it("normalizes roles and exposes reusable role helpers", () => {
    const rawUser = {
      id: "administrator-user",
      username: "administrator",
      email: "administrator@example.test",
      roles: [USER_ROLES.administrator, "Unknown", USER_ROLES.administrator],
    } as unknown as User;

    act(() => auth.login(rawUser, "token"));

    expect(auth.roles).toEqual([USER_ROLES.administrator]);
    expect(auth.hasRole(USER_ROLES.administrator)).toBe(true);
    expect(auth.hasAnyRole([USER_ROLES.client, USER_ROLES.administrator])).toBe(true);
  });

  it("clears query state on login, identity or role change, and logout", () => {
    const administrator: User = {
      id: "same-user",
      username: "user",
      email: "user@example.test",
      roles: [USER_ROLES.administrator],
    };

    queryClient.setQueryData(["protected"], "before-login");
    act(() => auth.login(administrator, "token-1"));
    expect(queryClient.getQueryData(["protected"])).toBeUndefined();

    queryClient.setQueryData(["protected"], "same-identity");
    act(() => auth.login(administrator, "token-2"));
    expect(queryClient.getQueryData(["protected"])).toBe("same-identity");

    queryClient.setQueryData(["protected"], "role-change");
    act(() =>
      auth.login({ ...administrator, roles: [USER_ROLES.client] }, "token-3"),
    );
    expect(queryClient.getQueryData(["protected"])).toBeUndefined();

    queryClient.setQueryData(["protected"], "logout");
    act(() => auth.logout());
    expect(queryClient.getQueryData(["protected"])).toBeUndefined();
    expect(auth.isAuthenticated).toBe(false);
    expect(auth.user).toBeNull();
    expect(auth.accessToken).toBeNull();
  });
});
