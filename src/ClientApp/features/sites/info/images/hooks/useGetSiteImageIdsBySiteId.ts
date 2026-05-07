import { QueryConfig, queryConfig } from "@/lib/react-query";

import { SiteImageIds } from "../types";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteImageIdsBySiteIdContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetSiteImageIdsBySiteIdContext = z.infer<
  typeof getSiteImageIdsBySiteIdContextSchema
>;

export const getSiteImageIdsBySiteId = ({
  siteId,
  accessToken,
}: GetSiteImageIdsBySiteIdContext): Promise<SiteImageIds[]> =>
  api.get(paths.images.getIdsBySiteId(siteId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetSiteImageIdsBySiteIdOptions = {
  siteId?: string;
  queryConfig?: QueryConfig<typeof getSiteImageIdsBySiteId>;
};

export const useGetSiteImageIdsBySiteId = ({
  siteId,
  queryConfig: customQueryConfig,
}: UseGetSiteImageIdsBySiteIdOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["site-image-ids", siteId],
    enabled: Boolean(siteId && accessToken),
    queryFn: () =>
      getSiteImageIdsBySiteId({
        siteId: siteId!,
        accessToken: accessToken!,
      }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteImageIdsBySiteId;
