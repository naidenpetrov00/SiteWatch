import { QueryConfig, queryConfig } from "@/lib/react-query";

import { Camera } from "./models";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getCameraByIdContextSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetCameraByIdContext = z.infer<typeof getCameraByIdContextSchema>;

export const getCameraById = ({
  cameraId: cameraId,
  accessToken,
}: GetCameraByIdContext): Promise<Camera> =>
  api.get(paths.cameras.getById(cameraId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseCameraByIdOptions = {
  cameraId: string;
  queryConfig?: QueryConfig<typeof getCameraById>;
};

export const useCameraById = ({ cameraId }: UseCameraByIdOptions) => {
  const { accessToken } = useAuth();
  return useQuery({
    queryKey: ["camera", cameraId],
    queryFn: () => {
      return getCameraById({ cameraId, accessToken: accessToken! });
    },
    ...queryConfig,
  });
};
