import { render, screen, userEvent, waitFor } from "@testing-library/react-native";
import * as DocumentPicker from "expo-document-picker";

import Invoices from "./Invoices";

const mockCreateFromFile = { isPending: false, mutateAsync: jest.fn() };

jest.mock("expo-document-picker", () => ({ getDocumentAsync: jest.fn() }));
jest.mock("expo-image-picker", () => ({ requestCameraPermissionsAsync: jest.fn(), requestMediaLibraryPermissionsAsync: jest.fn() }));
jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("@/hooks/useGetSearchParams", () => () => ({ siteId: "site-1" }));
jest.mock("@/hooks/useColorPalette", () => ({ useColorPalette: () => ({ background: "white", contrastText: "white", primary: "blue", secondary: "gray", text: "black" }) }));
jest.mock("react-native-safe-area-context", () => ({ useSafeAreaInsets: () => ({ bottom: 0 }) }));
jest.mock("../../hooks/useGetSiteInvoices", () => ({ useGetSiteInvoices: () => ({ data: [], isLoading: false, isError: false, isRefetching: false, refetch: jest.fn() }) }));
jest.mock("../../hooks/useInvoiceFileAccess", () => ({ useInvoiceFileAccess: () => ({ isPending: false, mutateAsync: jest.fn(), variables: undefined }) }));
jest.mock("../../hooks/useCreateInvoiceFromFile", () => ({ useCreateInvoiceFromFile: () => mockCreateFromFile }));

describe("Invoices", () => {
  beforeEach(() => {
    mockCreateFromFile.mutateAsync.mockReset();
    mockCreateFromFile.mutateAsync.mockResolvedValue({ id: "invoice-1" });
  });

  it("rejects oversized documents before starting the upload", async () => {
    jest.mocked(DocumentPicker.getDocumentAsync).mockResolvedValue({ canceled: false, assets: [{ uri: "file:///invoice.pdf", name: "invoice.pdf", mimeType: "application/pdf", size: 20 * 1024 * 1024 + 1 }] } as never);
    const user = userEvent.setup();
    render(<Invoices />);

    await user.press(screen.getByRole("button", { name: "Add invoice" }));
    await user.press(screen.getByRole("button", { name: "Select PDF or image" }));

    expect(await screen.findByText("The invoice file cannot exceed 20 MB.")).toBeOnTheScreen();
    expect(mockCreateFromFile.mutateAsync).not.toHaveBeenCalled();
  });

  it("uploads a supported picked document and confirms success", async () => {
    jest.mocked(DocumentPicker.getDocumentAsync).mockResolvedValue({ canceled: false, assets: [{ uri: "file:///invoice.pdf", name: "invoice.pdf", mimeType: "application/pdf", size: 20 }] } as never);
    const user = userEvent.setup();
    render(<Invoices />);

    await user.press(screen.getByRole("button", { name: "Add invoice" }));
    await user.press(screen.getByRole("button", { name: "Select PDF or image" }));

    await waitFor(() => expect(mockCreateFromFile.mutateAsync).toHaveBeenCalledWith({ siteId: "site-1", file: expect.objectContaining({ fileName: "invoice.pdf", contentType: "application/pdf" }) }));
    expect(await screen.findByText("Invoice uploaded.")).toBeOnTheScreen();
  });
});
