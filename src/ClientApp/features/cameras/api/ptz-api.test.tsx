jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));
jest.mock("./camera-api-fetch", () => ({ cameraApiFetch: jest.fn() }));

import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook } from "@testing-library/react-native";
import type { ReactNode } from "react";

import { useAuth } from "@/store/auth_context";

import { cameraApiFetch } from "./camera-api-fetch";
import { useMoveRelativePtz } from "./move-relative-ptz";
import { useStartPtzMovement } from "./start-ptz-movement";
import { useStopPtzMovement } from "./stop-ptz-movement";

const mockedUseAuth = jest.mocked(useAuth);
const mockedCameraApiFetch = jest.mocked(cameraApiFetch);
const accessToken = "header.payload.signature";
const cameraId = "11111111-1111-1111-1111-111111111111";

describe("camera PTZ mutations", () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    jest.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    mockedUseAuth.mockReturnValue({ accessToken } as ReturnType<typeof useAuth>);
    mockedCameraApiFetch.mockResolvedValue({} as unknown as Response);
  });

  afterEach(() => queryClient.clear());

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it("starts PTZ movement through the authenticated proxy route", async () => {
    const { result } = renderHook(() => useStartPtzMovement(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync({ cameraId, direction: "Left" });
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith(
      `/cameras/${cameraId}/ptz/start`,
      accessToken,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ direction: "Left" }),
      },
    );
  });

  it("stops PTZ movement through the authenticated proxy route", async () => {
    const { result } = renderHook(() => useStopPtzMovement(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync({ cameraId, direction: "Down" });
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith(
      `/cameras/${cameraId}/ptz/stop`,
      accessToken,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ direction: "Down" }),
      },
    );
  });

  it("moves PTZ relatively through the authenticated proxy route", async () => {
    const { result } = renderHook(() => useMoveRelativePtz(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync({ cameraId, horizontal: -1, vertical: 0.5, zoom: 1 });
    });

    expect(mockedCameraApiFetch).toHaveBeenCalledWith(
      `/cameras/${cameraId}/ptz/relative`,
      accessToken,
      {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ horizontal: -1, vertical: 0.5, zoom: 1 }),
      },
    );
  });
});
