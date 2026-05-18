import { queryConfig, type QueryConfig } from "@/lib/react-query";

import type { SiteVideoIds } from "../types";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteVideoIdsBySiteIdContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetSiteVideoIdsBySiteIdContext = z.infer<
  typeof getSiteVideoIdsBySiteIdContextSchema
>;

export const getSiteVideoIdsBySiteId = ({
  siteId,
  accessToken,
}: GetSiteVideoIdsBySiteIdContext): Promise<SiteVideoIds[]> =>
  api.get(paths.videos.getIdsBySiteId(siteId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetSiteVideoIdsBySiteIdOptions = {
  siteId?: string;
  queryConfig?: QueryConfig<typeof getSiteVideoIdsBySiteId>;
};

export const useGetSiteVideoIdsBySiteId = ({
  siteId,
  queryConfig: customQueryConfig,
}: UseGetSiteVideoIdsBySiteIdOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["site-video-ids", siteId],
    enabled: Boolean(siteId && accessToken),
    queryFn: () =>
      getSiteVideoIdsBySiteId({
        siteId: siteId!,
        accessToken: accessToken!,
      }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteVideoIdsBySiteId;
