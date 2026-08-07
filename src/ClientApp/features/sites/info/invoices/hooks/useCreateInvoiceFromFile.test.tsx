import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react-native";
import { ReactNode } from "react";

import { useCreateInvoiceFromFile } from "./useCreateInvoiceFromFile";
import { useAuth } from "@/store/auth_context";

jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));

const mockedUseAuth = jest.mocked(useAuth);

describe("useCreateInvoiceFromFile", () => {
  let queryClient: QueryClient;
  let fetchMock: jest.Mock;

  beforeEach(() => {
    queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
    fetchMock = jest.fn();
    global.fetch = fetchMock;
    mockedUseAuth.mockReturnValue({ accessToken: "header.payload.signature" } as ReturnType<typeof useAuth>);
  });

  afterEach(() => {
    queryClient.clear();
    jest.clearAllMocks();
  });

  const wrapper = ({ children }: { children: ReactNode }) =>
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
  const request = { siteId: "site-1", file: { uri: "file:///invoice.pdf", fileName: "invoice.pdf", contentType: "application/pdf" } };

  it("uploads multipart data with the bearer token and refreshes the site invoice cache", async () => {
    fetchMock.mockResolvedValue({ ok: true, json: async () => ({ id: "invoice-1" }) });
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useCreateInvoiceFromFile(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(fetchMock).toHaveBeenCalledWith(
      "https://api.example.test/invoices/site/site-1/file",
      expect.objectContaining({ method: "POST", headers: { Authorization: "Bearer header.payload.signature" }, body: expect.any(FormData) }),
    );
    await waitFor(() => expect(invalidate).toHaveBeenCalledWith({ queryKey: ["site-invoices", "site-1"] }));
  });

  it.each([
    [401, "Your session has expired. Sign in again and retry."],
    [403, "Your account is not allowed to upload invoices for this site."],
    [413, "The invoice file cannot exceed 20 MB."],
  ])("maps upload status %s to a useful error", async (status, message) => {
    fetchMock.mockResolvedValue({ ok: false, status, json: async () => null });
    const { result } = renderHook(() => useCreateInvoiceFromFile(), { wrapper });

    let thrown: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync(request);
      } catch (error) {
        thrown = error;
      }
    });

    expect(thrown).toEqual(expect.objectContaining({ message }));
  });

  it("fails before network access when the user has no token", async () => {
    mockedUseAuth.mockReturnValue({ accessToken: null } as ReturnType<typeof useAuth>);
    const { result } = renderHook(() => useCreateInvoiceFromFile(), { wrapper });

    let thrown: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync(request);
      } catch (error) {
        thrown = error;
      }
    });

    expect(thrown).toEqual(expect.objectContaining({ message: "Authentication is required to upload an invoice." }));
    expect(fetchMock).not.toHaveBeenCalled();
  });
});
