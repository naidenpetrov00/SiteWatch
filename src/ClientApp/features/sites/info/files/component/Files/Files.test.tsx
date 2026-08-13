import { render, screen, userEvent } from "@testing-library/react-native";

import Files from "./Files";

jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("@/hooks/useGetSearchParams", () => () => ({ siteId: "site-1" }));
jest.mock("@/hooks/useColorPalette", () => ({ useColorPalette: () => ({ background: "white", contrastText: "white", primary: "blue", secondary: "gray", text: "black" }) }));
jest.mock("react-native-safe-area-context", () => ({ useSafeAreaInsets: () => ({ bottom: 0 }) }));
jest.mock("@/features/sites/info/uploads/useUploadSiteMedia", () => ({
  usePendingSiteMediaUploads: () => [],
  useUploadSiteMedia: () => ({ isPending: false, mutateAsync: jest.fn() }),
}));
jest.mock("@/features/sites/info/uploads/SiteMediaUploadAction", () => () => null);
jest.mock("../../hooks/useGetSiteFileIdsBySiteId", () => ({
  useGetSiteFileIdsBySiteId: () => ({
    data: [
      { fileId: "file-1", fileName: "warranty.pdf", contentType: "application/pdf", documentType: "Warranty" },
      { fileId: "file-2", fileName: "diagram.pdf", contentType: "application/pdf", documentType: "DrawingOrSchema" },
      { fileId: "file-3", fileName: "permit.pdf", contentType: "application/pdf", documentType: "PermitOrApproval" },
    ],
    isLoading: false, isError: false, isRefetching: false, refetch: jest.fn(),
  }),
}));

describe("Files", () => {
  it("filters visible files by the selected document type", async () => {
    const user = userEvent.setup();
    render(<Files />);

    expect(screen.getByText("warranty.pdf")).toBeOnTheScreen();
    expect(screen.getByText("diagram.pdf")).toBeOnTheScreen();
    expect(screen.getByText("permit.pdf")).toBeOnTheScreen();

    await user.press(screen.getByText("Permit / Approval"));

    expect(screen.getByText("permit.pdf")).toBeOnTheScreen();
    expect(screen.queryByText("warranty.pdf")).toBeNull();
    expect(screen.queryByText("diagram.pdf")).toBeNull();
  });
});
