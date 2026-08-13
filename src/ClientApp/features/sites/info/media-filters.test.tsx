import { render, screen, userEvent } from "@testing-library/react-native";

import ImageFilters from "./images/component/Filters/Filters";
import VideoFilters from "./videos/component/Filters/Filters";

jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({
    background: "white",
    contrastText: "white",
    primary: "blue",
    secondary: "gray",
    text: "black",
  }),
}));

describe("site media filters", () => {
  it.each([
    ["image", ImageFilters],
    ["video", VideoFilters],
  ] as const)("renders accessible horizontally available %s categories", async (_, Filters) => {
    const setActiveFilter = jest.fn();
    const user = userEvent.setup();

    render(
      <Filters
        activeFilter="All"
        filters={["All", "HVAC Controls", "Access Control", "Other"]}
        setActiveFilter={setActiveFilter}
      />,
    );

    expect(screen.getByRole("button", { name: "All" })).toHaveProp(
      "accessibilityState",
      { selected: true },
    );
    expect(screen.getByRole("button", { name: "Access Control" })).toHaveProp(
      "accessibilityState",
      { selected: false },
    );

    await user.press(screen.getByRole("button", { name: "HVAC Controls" }));

    expect(setActiveFilter).toHaveBeenCalledWith("HVAC Controls");
  });
});
