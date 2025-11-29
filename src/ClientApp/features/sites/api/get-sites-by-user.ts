import { QueryConfig, queryConfig } from "@/lib/react-query";

import { Site } from "./types";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getSitesByAuthContextSchema = z.object({
  userId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

export type GetSitesByAuthContext = z.infer<typeof getSitesByAuthContextSchema>;

export const getSitesByUser = ({
  userId,
  accessToken,
}: GetSitesByAuthContext): Promise<Site[]> =>
  api.get(paths.sites.getByUserId(userId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseCommentsOptions = {
  queryConfig?: QueryConfig<typeof getSitesByUser>;
};

export const useGetSitesByUserId = ({}: UseCommentsOptions = {}) => {
  const { user, accessToken } = useAuth();
  const userId = user!.id;
  return useQuery({
    queryKey: ["sites", userId],
    queryFn: () => {
      return getSitesByUser({ userId, accessToken: accessToken! });
    },
    ...queryConfig,
  });
};
