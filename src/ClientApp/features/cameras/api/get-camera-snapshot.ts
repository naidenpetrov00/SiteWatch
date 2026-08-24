import * as FileSystem from "expo-file-system/legacy";

import { QueryConfig } from "@/lib/react-query";
import { useAuth } from "@/store/auth_context";
import { paths } from "@/config/constants/paths";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

import { cameraApiDownload } from "./camera-api-fetch";

const SNAPSHOT_CACHE_DIRECTORY = `${FileSystem.cacheDirectory ?? ""}camera-snapshots/`;

const getSnapshotFileUri = (cameraId: string) =>
  `${SNAPSHOT_CACHE_DIRECTORY}camera-snapshot-${cameraId}.jpg`;

export const getCameraSnapshotSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetCameraSnapshotInput = z.infer<typeof getCameraSnapshotSchema>;

export const getCameraSnapshot = async ({
  cameraId,
  accessToken,
}: GetCameraSnapshotInput): Promise<string> => {
  if (!FileSystem.cacheDirectory) {
    throw new Error("Snapshot cache directory is unavailable.");
  }

  const snapshotUri = getSnapshotFileUri(cameraId);

  try {
    await FileSystem.makeDirectoryAsync(SNAPSHOT_CACHE_DIRECTORY, {
      intermediates: true,
    });

    const result = await cameraApiDownload(
      paths.cameras.getSnapshot(cameraId),
      accessToken,
      snapshotUri,
    );

    console.info("Camera snapshot downloaded to cache.", {
      cameraId,
      contentType:
        result.mimeType ??
        result.headers["content-type"] ??
        result.headers["Content-Type"],
      status: result.status,
    });

    return result.uri;
  } catch (error) {
    console.warn("Camera snapshot download failed.", {
      cameraId,
      error: error instanceof Error ? error.message : String(error),
    });
    throw error;
  }
};

type UseGetCameraSnapshotOptions = {
  cameraId: string;
  queryConfig?: QueryConfig<typeof getCameraSnapshot>;
};

export const useGetCameraSnapshot = ({
  cameraId,
  queryConfig,
}: UseGetCameraSnapshotOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["camera-snapshot", cameraId],
    queryFn: () =>
      getCameraSnapshot({ cameraId, accessToken: accessToken! }),
    enabled: Boolean(cameraId && accessToken),
    retry: false,
    refetchOnWindowFocus: false,
    staleTime: 5_000,
    gcTime: 20_000,
    ...queryConfig,
  });
};
