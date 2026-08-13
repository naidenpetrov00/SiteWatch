import { render, screen, userEvent, waitFor } from "@testing-library/react-native";
import * as DocumentPicker from "expo-document-picker";
import { Alert } from "react-native";

import Invoices from "./Invoices";

const mockCreateFromFile = { isPending: false, mutateAsync: jest.fn() };

jest.mock("expo-document-picker", () => ({ getDocumentAsync: jest.fn() }));
jest.mock("expo-image-picker", () => ({ requestCameraPermissionsAsync: jest.fn(), requestMediaLibraryPermissionsAsync: jest.fn() }));
jest.mock("react-native", () => {
  const reactNative = jest.requireActual("react-native");

  reactNative.Platform.OS = "android";
  return reactNative;
});
jest.mock("expo-glass-effect", () => {
  const { View } = require("react-native");

  return { GlassView: View, isLiquidGlassAvailable: () => false };
});
jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("@/hooks/useGetSearchParams", () => () => ({ siteId: "site-1" }));
jest.mock("@/hooks/useColorPalette", () => ({ useColorPalette: () => ({ background: "white", contrastText: "white", primary: "blue", secondary: "gray", text: "black" }) }));
jest.mock("@/store/auth_context", () => ({
  useAuth: () => ({ hasAnyRole: () => true }),
}));
jest.mock("react-native-safe-area-context", () => ({ useSafeAreaInsets: () => ({ bottom: 0 }) }));
jest.mock("../../hooks/useGetSiteInvoices", () => ({ useGetSiteInvoices: () => ({ data: [], isLoading: false, isError: false, isRefetching: false, refetch: jest.fn() }) }));
jest.mock("../../hooks/useInvoiceFileAccess", () => ({ useInvoiceFileAccess: () => ({ isPending: false, mutateAsync: jest.fn(), variables: undefined }) }));
jest.mock("../../hooks/useCreateInvoiceFromFile", () => ({
  useCreateInvoiceFromFile: () => mockCreateFromFile,
  usePendingInvoiceUploads: () => [],
}));

describe("Invoices", () => {
  beforeEach(() => {
    jest.spyOn(Alert, "alert").mockImplementation(jest.fn());
    mockCreateFromFile.mutateAsync.mockReset();
    mockCreateFromFile.mutateAsync.mockResolvedValue({ id: "invoice-1" });
  });

  afterEach(() => jest.restoreAllMocks());

  it("rejects oversized documents before starting the upload", async () => {
    jest.mocked(DocumentPicker.getDocumentAsync).mockResolvedValue({ canceled: false, assets: [{ uri: "file:///invoice.pdf", name: "invoice.pdf", mimeType: "application/pdf", size: 20 * 1024 * 1024 + 1 }] } as never);
    const user = userEvent.setup();
    render(<Invoices />);

    await user.press(screen.getByRole("button", { name: "Add invoice" }));
    await user.press(screen.getByRole("button", { name: "Browse files" }));

    expect(Alert.alert).toHaveBeenCalledWith(
      "Upload failed",
      "The invoice file cannot exceed 20 MB.",
      undefined,
    );
    expect(mockCreateFromFile.mutateAsync).not.toHaveBeenCalled();
  });

  it("uploads a supported picked document and closes the picker", async () => {
    jest.mocked(DocumentPicker.getDocumentAsync).mockResolvedValue({ canceled: false, assets: [{ uri: "file:///invoice.pdf", name: "invoice.pdf", mimeType: "application/pdf", size: 20 }] } as never);
    const user = userEvent.setup();
    render(<Invoices />);

    await user.press(screen.getByRole("button", { name: "Add invoice" }));
    await user.press(screen.getByRole("button", { name: "Browse files" }));

    await waitFor(() => expect(mockCreateFromFile.mutateAsync).toHaveBeenCalledWith({ siteId: "site-1", file: expect.objectContaining({ fileName: "invoice.pdf", contentType: "application/pdf" }) }));
    expect(screen.queryByRole("button", { name: "Browse files" })).toBeNull();
  });
});
