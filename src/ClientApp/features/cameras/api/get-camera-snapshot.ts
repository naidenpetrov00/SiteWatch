import { QueryConfig } from "@/lib/react-query";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

import { blobToDataUrl } from "../utils";
import { cameraApiFetch } from "./camera-api-fetch";

export const getCameraSnapshotSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetCameraSnapshotInput = z.infer<typeof getCameraSnapshotSchema>;

export const getCameraSnapshot = async ({
  cameraId,
  accessToken,
}: GetCameraSnapshotInput): Promise<string> => {
  const response = await cameraApiFetch(
    paths.cameras.getSnapshot(cameraId),
    accessToken,
    { headers: { Accept: "image/jpeg" } },
  );

  try {
    const blob = await response.blob();
    console.info("Camera snapshot received.", {
      cameraId,
      contentType: response.headers.get("content-type"),
      byteLength: blob.size,
    });

    const snapshotUri = await blobToDataUrl(blob);
    console.info("Camera snapshot converted to an image URI.", {
      cameraId,
      uriLength: snapshotUri.length,
    });

    return snapshotUri;
  } catch (error) {
    console.warn("Camera snapshot could not be converted for display.", {
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
