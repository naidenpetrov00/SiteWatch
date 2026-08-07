import { fireEvent, render, screen } from "@testing-library/react-native";

import AccessDenied from "./AccessDenied";
import { useAuth } from "@/store/auth_context";

jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));
jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({ background: "white", primary: "blue", text: "black" }),
}));

describe("AccessDenied", () => {
  it("explains the denied state and signs the user out on request", () => {
    const logout = jest.fn();
    jest.mocked(useAuth).mockReturnValue({ logout } as ReturnType<typeof useAuth>);

    render(<AccessDenied />);
    fireEvent.press(screen.getByRole("button", { name: "Sign out" }));

    expect(screen.getByText("Access denied")).toBeOnTheScreen();
    expect(logout).toHaveBeenCalledTimes(1);
  });
});
