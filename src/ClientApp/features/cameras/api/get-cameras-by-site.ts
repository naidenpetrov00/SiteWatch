import { QueryConfig, queryConfig } from "@/lib/react-query";

import { Camera } from "./models";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getCamerasBySiteContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetCamerasBySiteContext = z.infer<
  typeof getCamerasBySiteContextSchema
>;

export const getCameraBySite = ({
  siteId: cameraId,
  accessToken,
}: GetCamerasBySiteContext): Promise<Camera[]> =>
  api.get(paths.cameras.getBySiteId(cameraId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseCamerasBySiteOptions = {
  siteId: string;
  queryConfig?: QueryConfig<typeof getCameraBySite>;
};

export const useCamerasBySite = ({ siteId }: UseCamerasBySiteOptions) => {
  const { accessToken } = useAuth();
  return useQuery({
    queryKey: ["cameras", siteId],
    queryFn: () => {
      return getCameraBySite({ siteId, accessToken: accessToken! });
    },
    ...queryConfig,
  });
};
