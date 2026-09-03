import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { queryConfig } from "@/lib/react-query";
import { useAuth } from "@/store/auth_context";
import type { IssueAttachment, IssueAttachmentAccess, PendingIssueAttachment } from "../types";

const authorizationHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

const attachmentError = async (response: Response) => {
  const payload = await response.json().catch(() => null) as { detail?: string; title?: string } | null;
  return payload?.detail ?? payload?.title ?? "Unable to upload the attachment.";
};

export const getIssueAttachments = (issueId: string, accessToken: string): Promise<IssueAttachment[]> =>
  api.get(paths.issues.attachments(issueId), { headers: authorizationHeaders(accessToken) });

export const getIssueAttachmentAccess = (
  issueId: string,
  attachmentId: string,
  accessToken: string,
  preview = false,
  download = false,
): Promise<IssueAttachmentAccess> =>
  api.get(paths.issues.attachmentAccess(issueId, attachmentId), {
    headers: authorizationHeaders(accessToken),
    params: { preview, download },
  }).then((access: IssueAttachmentAccess) => ({
    ...access,
    url: new URL(access.url, env.API_URL).toString(),
  }));

const uploadIssueAttachment = async (
  issueId: string,
  attachment: PendingIssueAttachment,
  accessToken: string,
): Promise<IssueAttachment> => {
  const formData = new FormData();
  formData.append("file", {
    uri: attachment.uri,
    name: attachment.fileName,
    type: attachment.contentType,
  } as unknown as Blob);

  let response: Response;
  try {
    response = await fetch(new URL(paths.issues.attachments(issueId), env.API_URL).toString(), {
      method: "POST",
      headers: authorizationHeaders(accessToken),
      body: formData,
    });
  } catch {
    throw new Error("Unable to reach the server. Check your connection and retry.");
  }

  if (!response.ok) throw new Error(await attachmentError(response));
  return response.json() as Promise<IssueAttachment>;
};

export const useGetIssueAttachments = (issueId?: string) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["issue-attachments", issueId],
    queryFn: () => getIssueAttachments(issueId!, accessToken!),
    ...queryConfig,
    enabled: Boolean(issueId && accessToken),
  });
};

export const useGetIssueAttachmentPreview = (
  issueId?: string,
  attachmentId?: string,
  hasPreview = false,
) => {
  const { accessToken } = useAuth();

  return useQuery({
    queryKey: ["issue-attachment-preview", issueId, attachmentId],
    queryFn: () => getIssueAttachmentAccess(issueId!, attachmentId!, accessToken!, true),
    ...queryConfig,
    enabled: Boolean(issueId && attachmentId && accessToken && hasPreview),
  });
};

export const useUploadIssueAttachment = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["issue-attachment", "upload"],
    mutationFn: ({ issueId, attachment }: { issueId: string; attachment: PendingIssueAttachment }) => {
      if (!accessToken) throw new Error("Authentication is required to upload an attachment.");
      return uploadIssueAttachment(issueId, attachment, accessToken);
    },
    onSuccess: async (_attachment, { issueId }) => {
      await queryClient.invalidateQueries({ queryKey: ["issue-attachments", issueId] });
    },
  });
};

export const useIssueAttachmentAccess = () => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationKey: ["issue-attachment", "access"],
    mutationFn: ({ issueId, attachmentId, preview, download }: {
      issueId: string;
      attachmentId: string;
      preview?: boolean;
      download?: boolean;
    }) => {
      if (!accessToken) throw new Error("Authentication is required to open an attachment.");
      return getIssueAttachmentAccess(issueId, attachmentId, accessToken, preview, download);
    },
  });
};
