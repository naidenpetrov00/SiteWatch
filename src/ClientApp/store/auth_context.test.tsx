import { Pressable, Text } from "react-native";
import { fireEvent, render, screen } from "@testing-library/react-native";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

import { AuthProvider, useAuth } from "@/store/auth_context";
import { USER_ROLES } from "@/types/authorization";
import type { User } from "@/types/identity";

const administrator: User = {
  id: "same-user",
  username: "user",
  email: "administrator@example.test",
  roles: [USER_ROLES.administrator],
};

const Probe = () => {
  const auth = useAuth();
  return (
    <>
      <Text testID="roles">{auth.roles.join(",")}</Text>
      <Text testID="authenticated">{String(auth.isAuthenticated)}</Text>
      <Pressable accessibilityRole="button" accessibilityLabel="Log in" onPress={() => auth.login(administrator, "token-1")} />
      <Pressable accessibilityRole="button" accessibilityLabel="Log out" onPress={auth.logout} />
    </>
  );
};

describe("AuthProvider", () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    queryClient = new QueryClient();
  });

  afterEach(() => queryClient.clear());

  const renderProvider = () => render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider><Probe /></AuthProvider>
    </QueryClientProvider>,
  );

  it("normalizes roles exposed through the authenticated context", () => {
    renderProvider();
    fireEvent.press(screen.getByRole("button", { name: "Log in" }));

    expect(screen.getByTestId("roles")).toHaveTextContent(USER_ROLES.administrator);
    expect(screen.getByTestId("authenticated")).toHaveTextContent("true");
  });

  it("clears protected query data when identity state changes", () => {
    renderProvider();
    queryClient.setQueryData(["protected"], "before-login");

    fireEvent.press(screen.getByRole("button", { name: "Log in" }));
    expect(queryClient.getQueryData(["protected"])).toBeUndefined();

    queryClient.setQueryData(["protected"], "before-logout");
    fireEvent.press(screen.getByRole("button", { name: "Log out" }));
    expect(queryClient.getQueryData(["protected"])).toBeUndefined();
    expect(screen.getByTestId("authenticated")).toHaveTextContent("false");
  });
});
