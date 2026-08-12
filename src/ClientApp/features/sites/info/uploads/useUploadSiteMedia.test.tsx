import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { act, renderHook, waitFor } from "@testing-library/react-native";
import type { ReactNode } from "react";

import { useUploadSiteMedia } from "./useUploadSiteMedia";
import type { UploadSiteMediaRequest } from "./types";
import { useAuth } from "@/store/auth_context";

jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("@/store/auth_context", () => ({ useAuth: jest.fn() }));

const mockedUseAuth = jest.mocked(useAuth);

describe("useUploadSiteMedia", () => {
  let queryClient: QueryClient;
  let fetchMock: jest.Mock;

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
    });
    fetchMock = jest.fn();
    global.fetch = fetchMock;
    mockedUseAuth.mockReturnValue({
      accessToken: "header.payload.signature",
    } as ReturnType<typeof useAuth>);
  });

  afterEach(() => {
    queryClient.clear();
    jest.clearAllMocks();
  });

  const wrapper = ({ children }: { children: ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  const imageRequest: UploadSiteMediaRequest = {
    siteId: "site-1",
    kind: "image" as const,
    asset: {
      uri: "file:///photo.jpg",
      fileName: "photo.jpg",
      contentType: "image/jpeg",
    },
    category: "Pipes",
  };

  it("uploads an image with authorization, updates the image cache, and refreshes it", async () => {
    queryClient.setQueryData(["site-image-ids", "site-1"], [
      {
        imageId: "existing-image",
        thumbnailId: "existing-thumbnail",
        category: "Pipes",
        created: "2026-01-01T00:00:00Z",
      },
    ]);
    fetchMock.mockResolvedValue({
      ok: true,
      json: async () => ({
        originalFileId: "image-1",
        thumbnailFileId: "thumbnail-1",
      }),
    });
    const invalidate = jest.spyOn(queryClient, "invalidateQueries");
    const { result } = renderHook(() => useUploadSiteMedia(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync(imageRequest);
    });

    expect(fetchMock).toHaveBeenCalledWith(
      "https://api.example.test/images/site-1",
      expect.objectContaining({
        method: "POST",
        headers: { Authorization: "Bearer header.payload.signature" },
        body: expect.any(FormData),
      }),
    );
    expect(queryClient.getQueryData(["site-image-ids", "site-1"])).toEqual([
      expect.objectContaining({
        imageId: "image-1",
        thumbnailId: "thumbnail-1",
        category: "Pipes",
      }),
      expect.objectContaining({ imageId: "existing-image" }),
    ]);
    await waitFor(() =>
      expect(invalidate).toHaveBeenCalledWith({
        queryKey: ["site-image-ids", "site-1"],
      }),
    );
  });

  const successfulRequests: readonly [
    path: string,
    request: UploadSiteMediaRequest,
    response: Record<string, unknown>,
  ][] = [
    [
      "/videos/site-1",
      {
        siteId: "site-1",
        kind: "video" as const,
        asset: {
          uri: "file:///clip.mp4",
          fileName: "clip.mp4",
          contentType: "video/mp4",
        },
        category: "Pipes",
      },
      { videoFileId: "video-1", snapshotFileId: "snapshot-1", durationSeconds: 12 },
    ],
    [
      "/files/site-1",
      {
        siteId: "site-1",
        kind: "file" as const,
        asset: {
          uri: "file:///manual.pdf",
          fileName: "manual.pdf",
          contentType: "application/pdf",
        },
        documentType: "InstallationOrOperationManual",
      },
      { fileId: "file-1" },
    ],
  ];

  it.each(successfulRequests)("posts each media kind to %s and prepends its returned cache entry", async (path, request, response) => {
    fetchMock.mockResolvedValue({ ok: true, json: async () => response });
    const { result } = renderHook(() => useUploadSiteMedia(), { wrapper });

    await act(async () => {
      await result.current.mutateAsync(request);
    });

    expect(fetchMock).toHaveBeenCalledWith(
      `https://api.example.test${path}`,
      expect.objectContaining({ method: "POST", body: expect.any(FormData) }),
    );
    if (request.kind === "video") {
      expect(queryClient.getQueryData(["site-video-ids", "site-1"])).toEqual([
        expect.objectContaining({
          videoId: "video-1",
          snapshotId: "snapshot-1",
          durationSeconds: 12,
          category: "Pipes",
        }),
      ]);
    } else {
      expect(queryClient.getQueryData(["site-file-ids", "site-1"])).toEqual([
        expect.objectContaining({
          fileId: "file-1",
          fileName: "manual.pdf",
          contentType: "application/pdf",
          documentType: "InstallationOrOperationManual",
        }),
      ]);
    }
  });

  it.each([
    [401, null, "Your session has expired. Sign in again and retry."],
    [403, null, "Your account is not allowed to upload site content."],
    [413, null, "The image file cannot exceed 50 MB."],
    [400, { errors: { file: ["The image content is invalid."] } }, "The image content is invalid."],
  ])("maps upload failure status %s to a useful error", async (status, payload, message) => {
    fetchMock.mockResolvedValue({ ok: false, status, json: async () => payload });
    const { result } = renderHook(() => useUploadSiteMedia(), { wrapper });

    let thrown: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync(imageRequest);
      } catch (error) {
        thrown = error;
      }
    });

    expect(thrown).toEqual(expect.objectContaining({ message }));
  });

  it("fails before network access when the user is not authenticated", async () => {
    mockedUseAuth.mockReturnValue({ accessToken: null } as ReturnType<typeof useAuth>);
    const { result } = renderHook(() => useUploadSiteMedia(), { wrapper });

    let thrown: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync(imageRequest);
      } catch (error) {
        thrown = error;
      }
    });

    expect(thrown).toEqual(expect.objectContaining({
      message: "Authentication is required to upload site content.",
    }));
    expect(fetchMock).not.toHaveBeenCalled();
  });

  it("returns a retryable message when the server cannot be reached", async () => {
    fetchMock.mockRejectedValue(new Error("offline"));
    const { result } = renderHook(() => useUploadSiteMedia(), { wrapper });

    let thrown: unknown;
    await act(async () => {
      try {
        await result.current.mutateAsync(imageRequest);
      } catch (error) {
        thrown = error;
      }
    });

    expect(thrown).toEqual(expect.objectContaining({
      message: "Unable to reach the server. Check your connection and retry.",
    }));
  });
});
