import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { queryConfig, type QueryConfig } from "@/lib/react-query";
import { useAuth } from "@/store/auth_context";
import type { SiteInvoice } from "../types";

const siteInvoicesContextSchema = z.object({
  siteId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

type SiteInvoicesContext = z.infer<typeof siteInvoicesContextSchema>;

export const getSiteInvoices = ({
  siteId,
  accessToken,
}: SiteInvoicesContext): Promise<SiteInvoice[]> =>
  api.get(paths.invoices.getBySiteId(siteId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetSiteInvoicesOptions = {
  siteId?: string;
  enabled?: boolean;
  queryConfig?: QueryConfig<typeof getSiteInvoices>;
};

export const useGetSiteInvoices = ({
  siteId,
  enabled = true,
  queryConfig: customQueryConfig,
}: UseGetSiteInvoicesOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["site-invoices", siteId],
    queryFn: () =>
      getSiteInvoices({
        siteId: siteId!,
        accessToken: accessToken!,
      }),
    ...queryConfig,
    ...customQueryConfig,
    enabled: enabled && Boolean(siteId && accessToken),
  });
};
