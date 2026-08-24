import { fireEvent, render, screen } from "@testing-library/react-native";
import { RefreshControl } from "react-native";

import SiteInfo from "@/app/(app)/Site/[siteId]/(tabs)/Info";
import { useGetSitesByUserId } from "@/features/sites/api/get-sites-by-user";

jest.mock("@/features/sites/api/get-sites-by-user", () => ({
  useGetSitesByUserId: jest.fn(),
}));
jest.mock("@/hooks/useGetSearchParams", () => () => ({ siteId: "site-1" }));
jest.mock("@/features/sites/info/component/Details/Details/Details", () => () => null);
jest.mock("@/features/sites/info/component/Summary/Summary", () => () => null);
jest.mock("@/hooks/useColorPalette", () => ({
  useColorPalette: () => ({ background: "white", primary: "blue" }),
}));
jest.mock("react-native-safe-area-context", () => ({
  useSafeAreaInsets: () => ({ bottom: 0 }),
}));

const mockedUseGetSitesByUserId = jest.mocked(useGetSitesByUserId);

describe("site Info route", () => {
  it("reflects refresh state and refetches site data on pull-to-refresh", () => {
    const refetch = jest.fn();
    mockedUseGetSitesByUserId.mockReturnValue({
      data: [{ id: "site-1" }],
      isLoading: false,
      isRefetching: true,
      refetch,
    } as ReturnType<typeof useGetSitesByUserId>);

    render(<SiteInfo />);
    const refreshControl = screen.UNSAFE_getByType(RefreshControl);

    expect(refreshControl.props.refreshing).toBe(true);
    fireEvent(refreshControl, "refresh");
    expect(refetch).toHaveBeenCalledTimes(1);
  });
});
