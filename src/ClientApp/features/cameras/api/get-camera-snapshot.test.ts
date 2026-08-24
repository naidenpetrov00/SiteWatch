jest.mock("expo-file-system/legacy", () => ({
  cacheDirectory: "file:///cache/",
  makeDirectoryAsync: jest.fn(),
}));
jest.mock("./camera-api-fetch", () => ({ cameraApiDownload: jest.fn() }));

import * as FileSystem from "expo-file-system/legacy";

import { paths } from "@/config/constants/paths";

import { cameraApiDownload } from "./camera-api-fetch";
import { getCameraSnapshot } from "./get-camera-snapshot";

const mockedDownload = jest.mocked(cameraApiDownload);
const mockedMakeDirectory = jest.mocked(FileSystem.makeDirectoryAsync);

describe("getCameraSnapshot", () => {
  beforeEach(() => jest.clearAllMocks());

  it("downloads the camera snapshot through the authenticated API route into a stable cache path", async () => {
    const cameraId = "11111111-1111-1111-1111-111111111111";
    mockedDownload.mockResolvedValue({
      uri: "file:///cache/camera-snapshots/camera-snapshot-11111111-1111-1111-1111-111111111111.jpg",
      mimeType: "image/jpeg",
      headers: {},
    } as Awaited<ReturnType<typeof cameraApiDownload>>);

    await expect(getCameraSnapshot({ cameraId, accessToken: "token-42" }))
      .resolves.toBe("file:///cache/camera-snapshots/camera-snapshot-11111111-1111-1111-1111-111111111111.jpg");

    expect(mockedMakeDirectory).toHaveBeenCalledWith("file:///cache/camera-snapshots/", { intermediates: true });
    expect(mockedDownload).toHaveBeenCalledWith(
      paths.cameras.getSnapshot(cameraId),
      "token-42",
      "file:///cache/camera-snapshots/camera-snapshot-11111111-1111-1111-1111-111111111111.jpg",
    );
  });
});
