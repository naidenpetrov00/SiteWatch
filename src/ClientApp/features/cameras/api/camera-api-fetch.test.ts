jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));
jest.mock("expo/fetch", () => ({ fetch: jest.fn() }));
jest.mock("expo-file-system/legacy", () => ({
  downloadAsync: jest.fn(),
  deleteAsync: jest.fn(),
}));

import * as FileSystem from "expo-file-system/legacy";
import { fetch } from "expo/fetch";

import { cameraApiDownload, cameraApiFetch } from "./camera-api-fetch";

const mockedFetch = jest.mocked(fetch);
const mockedDownload = jest.mocked(FileSystem.downloadAsync);
const mockedDelete = jest.mocked(FileSystem.deleteAsync);

describe("camera API transport", () => {
  beforeEach(() => jest.clearAllMocks());

  it("adds the access token to proxied camera requests", async () => {
    const response = { ok: true } as Response;
    mockedFetch.mockResolvedValue(response);

    await expect(cameraApiFetch("/cameras/camera-42/ptz/start", "token-42", { method: "POST" })).resolves.toBe(response);

    expect(mockedFetch).toHaveBeenCalledWith("https://api.example.test/cameras/camera-42/ptz/start", expect.objectContaining({ method: "POST" }));
    const [, init] = mockedFetch.mock.calls[0];
    expect(new Headers(init?.headers).get("Authorization")).toBe("Bearer token-42");
  });

  it("returns the server problem detail when a proxied camera request fails", async () => {
    mockedFetch.mockResolvedValue({
      ok: false,
      status: 502,
      json: jest.fn().mockResolvedValue({ detail: "The camera did not accept the request." }),
    } as unknown as Response);

    await expect(cameraApiFetch("/cameras/camera-42/snapshot", "token-42"))
      .rejects.toThrow("The camera did not accept the request.");
  });

  it("removes a failed snapshot download from the local cache", async () => {
    mockedDownload.mockResolvedValue({ status: 502, uri: "file:///cache/camera.jpg" } as Awaited<ReturnType<typeof FileSystem.downloadAsync>>);

    await expect(cameraApiDownload("/cameras/camera-42/snapshot", "token-42", "file:///cache/camera.jpg"))
      .rejects.toThrow("Camera request failed with status 502.");

    expect(mockedDownload).toHaveBeenCalledWith(
      "https://api.example.test/cameras/camera-42/snapshot",
      "file:///cache/camera.jpg",
      { headers: { Accept: "image/jpeg", Authorization: "Bearer token-42" } },
    );
    expect(mockedDelete).toHaveBeenCalledWith("file:///cache/camera.jpg", { idempotent: true });
  });
});
