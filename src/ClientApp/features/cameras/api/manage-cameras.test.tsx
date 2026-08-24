jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));
jest.mock("./camera-api-fetch", () => ({ cameraApiFetch: jest.fn() }));

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react-native";
import type { ReactNode } from "react";

import { useAuth } from "@/store/auth_context";

import { cameraApiFetch } from "./camera-api-fetch";
import {
  useCreateCamera,
  useDeleteCamera,
  useMoveCameraToSite,
} from "./manage-cameras";

const mockedUseAuth = jest.mocked(useAuth);
const mockedCameraApiFetch = jest.mocked(cameraApiFetch);
const accessToken = "header.payload.signature";
const cameraId = "11111111-1111-1111-1111-111111111111";
const sourceSiteId = "22222222-2222-2222-2222-222222222222";
const destinationSiteId = "33333333-3333-3333-3333-333333333333";

describe("camera management mutations", () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    mockedUseAuth.mockReturnValue({ accessToken } as ReturnType<typeof useAuth>);
  });

  afterEach(() => queryClient.clear());

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it("creates a camera through the proxy and refreshes its site's camera list", async () => {
    mockedCameraApiFetch.mockResolvedValue({
      json: jest.fn().mockResolvedValue({ id: cameraId }),
    } as unknown as Response);
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useCreateCamera(sourceSiteId), { wrapper });
    const request = cameraRequest(sourceSiteId);

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith("/cameras", accessToken, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(request),
    });
    await waitFor(() =>
      expect(invalidate).toHaveBeenCalledWith({ queryKey: ["cameras", sourceSiteId] }),
    );
  });

  it("moves a camera through the proxy and refreshes both site camera lists", async () => {
    mockedCameraApiFetch.mockResolvedValue({} as unknown as Response);
    const remove = jest.spyOn(queryClient, "removeQueries");
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useMoveCameraToSite(sourceSiteId), { wrapper });

    await act(async () => {
      await result.current.mutateAsync({ cameraId, siteId: destinationSiteId });
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith(
      `/cameras/${cameraId}/site`,
      accessToken,
      {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ siteId: destinationSiteId }),
      },
    );
    expect(remove).toHaveBeenCalledWith({ queryKey: ["camera", cameraId] });
    await waitFor(() => {
      expect(invalidate).toHaveBeenCalledWith({ queryKey: ["cameras", sourceSiteId] });
      expect(invalidate).toHaveBeenCalledWith({ queryKey: ["cameras", destinationSiteId] });
    });
  });

  it("deletes a camera through the proxy and refreshes its owning site's camera list", async () => {
    mockedCameraApiFetch.mockResolvedValue({} as unknown as Response);
    const remove = jest.spyOn(queryClient, "removeQueries");
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useDeleteCamera(sourceSiteId), { wrapper });

    await act(async () => {
      await result.current.mutateAsync(cameraId);
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith(`/cameras/${cameraId}`, accessToken, {
      method: "DELETE",
    });
    expect(remove).toHaveBeenCalledWith({ queryKey: ["camera", cameraId] });
    await waitFor(() =>
      expect(invalidate).toHaveBeenCalledWith({ queryKey: ["cameras", sourceSiteId] }),
    );
  });
});

function cameraRequest(siteId: string) {
  return {
    name: "North gate",
    brand: "Dahua" as const,
    model: "IPC-HDW",
    username: "operator",
    password: "secret",
    ipAddress: "192.0.2.10",
    rtspPort: 554,
    ptzPort: 443,
    protocol: "Https" as const,
    siteId,
  };
}
