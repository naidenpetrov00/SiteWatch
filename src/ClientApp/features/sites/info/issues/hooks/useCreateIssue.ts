import { useMutation, useQueryClient } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { useAuth } from "@/store/auth_context";
import type { CreateIssueRequest, CreateIssueResponse } from "../types";

const createIssue = async (
  request: CreateIssueRequest,
  accessToken: string,
): Promise<CreateIssueResponse> => {
  const response = await api.post(paths.issues.create, request, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
      "Content-Type": "application/json",
    },
  });

  return response as unknown as CreateIssueResponse;
};

export const useCreateIssue = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["issue", "create"],
    mutationFn: (request: CreateIssueRequest) => {
      if (!accessToken) throw new Error("Authentication is required to add an issue.");
      return createIssue(request, accessToken);
    },
    onSuccess: async (_response, request) => {
      await queryClient.invalidateQueries({ queryKey: ["site-issues", request.siteId] });
    },
  });
};
