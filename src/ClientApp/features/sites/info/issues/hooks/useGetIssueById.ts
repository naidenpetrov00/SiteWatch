import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { queryConfig, type QueryConfig } from "@/lib/react-query";
import { useAuth } from "@/store/auth_context";
import type { SiteIssue } from "../types";

const issueContextSchema = z.object({
  issueId: z.string().uuid("Invalid GUID format"),
  accessToken: z.string().jwt(),
});

type IssueContext = z.infer<typeof issueContextSchema>;

export const getIssueById = ({ issueId, accessToken }: IssueContext): Promise<SiteIssue> =>
  api.get(paths.issues.getById(issueId), {
    headers: { Authorization: `Bearer ${accessToken}` },
  });

type UseGetIssueByIdOptions = {
  issueId?: string;
  queryConfig?: QueryConfig<typeof getIssueById>;
};

export const useGetIssueById = ({
  issueId,
  queryConfig: customQueryConfig,
}: UseGetIssueByIdOptions) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["issue", issueId],
    queryFn: () => getIssueById({ issueId: issueId!, accessToken: accessToken! }),
    ...queryConfig,
    ...customQueryConfig,
    enabled: Boolean(issueId && accessToken),
  });
};
