import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { queryConfig, type QueryConfig } from "@/lib/react-query";
import { useAuth } from "@/store/auth_context";
import type { SiteIssue } from "../types";

const siteIssuesContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

type SiteIssuesContext = z.infer<typeof siteIssuesContextSchema>;

export const getSiteIssues = ({
  siteId,
  accessToken,
}: SiteIssuesContext): Promise<SiteIssue[]> =>
  api.get(paths.issues.getBySiteId(siteId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetSiteIssuesOptions = {
  siteId?: string;
  queryConfig?: QueryConfig<typeof getSiteIssues>;
};

export const useGetSiteIssues = ({
  siteId,
  queryConfig: customQueryConfig,
}: UseGetSiteIssuesOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["site-issues", siteId],
    queryFn: () => getSiteIssues({ siteId: siteId!, accessToken: accessToken! }),
    ...queryConfig,
    ...customQueryConfig,
    enabled: Boolean(siteId && accessToken),
  });
};
