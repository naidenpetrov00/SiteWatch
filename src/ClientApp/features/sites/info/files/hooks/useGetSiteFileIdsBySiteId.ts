import { queryConfig, type QueryConfig } from "@/lib/react-query";

import type { SiteFileIds } from "../types";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSiteFileIdsBySiteIdContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetSiteFileIdsBySiteIdContext = z.infer<
  typeof getSiteFileIdsBySiteIdContextSchema
>;

export const getSiteFileIdsBySiteId = ({
  siteId,
  accessToken,
}: GetSiteFileIdsBySiteIdContext): Promise<SiteFileIds[]> =>
  api.get(paths.files.getIdsBySiteId(siteId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetSiteFileIdsBySiteIdOptions = {
  siteId?: string;
  queryConfig?: QueryConfig<typeof getSiteFileIdsBySiteId>;
};

export const useGetSiteFileIdsBySiteId = ({
  siteId,
  queryConfig: customQueryConfig,
}: UseGetSiteFileIdsBySiteIdOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["site-file-ids", siteId],
    enabled: Boolean(siteId && accessToken),
    queryFn: () =>
      getSiteFileIdsBySiteId({
        siteId: siteId!,
        accessToken: accessToken!,
      }),
    ...queryConfig,
    ...customQueryConfig,
  });
};

export default useGetSiteFileIdsBySiteId;
