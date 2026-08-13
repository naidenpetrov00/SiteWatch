import { render, screen } from "@testing-library/react-native";

import SiteImages from "@/app/(app)/Site/[siteId]/Images/SiteImages";
import SiteVideos from "@/app/(app)/Site/[siteId]/Videos/SiteVideos";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";

jest.mock("@/features/sites/api/get-sites-by-user", () => ({
  useGetSitesByUserId: jest.fn(),
}));
jest.mock("@/hooks/useGetSearchParams", () => () => ({ siteId: "site-42" }));
jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({ background: "white" }),
}));
jest.mock("@/features/sites/info/images/component/Header/Header", () => () => null);
jest.mock("@/features/sites/info/images/component/Images/Images", () => () => null);
jest.mock("@/features/sites/info/videos/component/Header/Header", () => () => null);
jest.mock("@/features/sites/info/videos/component/Videos/Videos", () => () => null);
jest.mock("@/features/sites/info/images/component/Filters/Filters", () => ({
  __esModule: true,
  default: ({ filters }: { filters: readonly string[] }) => {
    const React = require("react");
    const { Text } = require("react-native");
    return React.createElement(Text, null, `Filters: ${filters.join(" | ")}`);
  },
}));
jest.mock("@/features/sites/info/videos/component/Filters/Filters", () => ({
  __esModule: true,
  default: ({ filters }: { filters: readonly string[] }) => {
    const React = require("react");
    const { Text } = require("react-native");
    return React.createElement(Text, null, `Filters: ${filters.join(" | ")}`);
  },
}));
jest.mock("@/features/sites/info/uploads/SiteMediaUploadAction", () => ({
  __esModule: true,
  default: ({
    kind,
    allowedCategories = [],
  }: {
    kind: string;
    allowedCategories?: readonly string[];
  }) => {
    const React = require("react");
    const { Text } = require("react-native");
    return React.createElement(
      Text,
      null,
      `Upload ${kind}: ${allowedCategories.join(" | ")}`,
    );
  },
}));

const mockedUseGetSitesByUserId = jest.mocked(useGetSitesByUserId);

describe("site media routes", () => {
  beforeEach(() => {
    mockedUseGetSitesByUserId.mockReturnValue({
      data: [
        {
          id: "site-42",
          name: "House 42",
          address: "42 Main Street",
          mediaPolicy: {
            preset: "Custom",
            categories: ["HVAC Controls", "Access Control", "Other"],
          },
        },
      ],
    } as ReturnType<typeof useGetSitesByUserId>);
  });

  it.each([
    ["image", SiteImages],
    ["video", SiteVideos],
  ] as const)("uses the shared policy categories for the %s screen", (kind, Screen) => {
    render(<Screen />);

    expect(
      screen.getByText("Filters: All | HVAC Controls | Access Control | Other"),
    ).toBeOnTheScreen();
    expect(
      screen.getByText(`Upload ${kind}: HVAC Controls | Access Control | Other`),
    ).toBeOnTheScreen();
  });
});
