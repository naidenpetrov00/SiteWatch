import * as FileSystem from "expo-file-system/legacy";
import * as Sharing from "expo-sharing";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { env } from "@/config/env";
import { api } from "@/lib/api-client";
import { useAuth } from "@/store/auth_context";
import type {
  ClientProposalDetails,
  ClientProposalSummary,
  ProposalPdfAccess,
  ProposalResponseDecision,
} from "./types";

const authHeaders = (accessToken: string) => ({
  Authorization: `Bearer ${accessToken}`,
});

export const proposalKeys = {
  site: (siteId: string) => ["client-proposals", "site", siteId] as const,
  detail: (siteId: string, proposalId: string) =>
    ["client-proposals", "site", siteId, "detail", proposalId] as const,
};

export const useClientProposals = (siteId?: string) => {
  const { accessToken } = useAuth();
  return useQuery({
    queryKey: proposalKeys.site(siteId ?? ""),
    queryFn: (): Promise<ClientProposalSummary[]> =>
      api.get(paths.proposals.getBySiteId(siteId!), {
        headers: authHeaders(accessToken!),
      }),
    enabled: Boolean(siteId && accessToken),
  });
};

export const useClientProposal = (siteId?: string, proposalId?: string) => {
  const { accessToken } = useAuth();
  return useQuery({
    queryKey: proposalKeys.detail(siteId ?? "", proposalId ?? ""),
    queryFn: (): Promise<ClientProposalDetails> =>
      api.get(paths.proposals.getById(siteId!, proposalId!), {
        headers: authHeaders(accessToken!),
      }),
    enabled: Boolean(siteId && proposalId && accessToken),
  });
};

export const useRespondToProposal = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: ["client-proposals", "respond"],
    mutationFn: ({
      siteId,
      proposalId,
      decision,
      comment,
    }: {
      siteId: string;
      proposalId: string;
      decision: ProposalResponseDecision;
      comment: string | null;
    }): Promise<ClientProposalDetails> =>
      api.patch(
        paths.proposals.respond(siteId, proposalId),
        { decision, comment },
        { headers: authHeaders(accessToken!) },
      ),
    onSuccess: (proposal, variables) => {
      queryClient.setQueryData(
        proposalKeys.detail(variables.siteId, variables.proposalId),
        proposal,
      );
      return queryClient.invalidateQueries({
        queryKey: proposalKeys.site(variables.siteId),
      });
    },
  });
};

export const useShareProposalPdf = () => {
  const { accessToken } = useAuth();
  return useMutation({
    mutationKey: ["client-proposals", "share-pdf"],
    mutationFn: async ({ siteId, proposalId }: { siteId: string; proposalId: string }) => {
      if (!accessToken || !FileSystem.cacheDirectory) {
        throw new Error("Proposal PDF sharing is unavailable.");
      }

      const access: ProposalPdfAccess = await api.get(
        paths.proposals.getPdfAccess(siteId, proposalId),
        { headers: authHeaders(accessToken) },
      );
      const safeFileName = access.fileName.replace(/[^a-zA-Z0-9._-]/g, "_");
      const fileUri = `${FileSystem.cacheDirectory}${proposalId}-${safeFileName}`;
      try {
        const result = await FileSystem.downloadAsync(
          `${env.API_URL}${access.url}`,
          fileUri,
          { headers: authHeaders(accessToken) },
        );
        if (result.status < 200 || result.status >= 300) {
          throw new Error(`PDF download failed with status ${result.status}.`);
        }
        if (!(await Sharing.isAvailableAsync())) {
          throw new Error("Sharing is not available on this device.");
        }
        await Sharing.shareAsync(result.uri, {
          dialogTitle: "Share Proposal PDF",
          mimeType: access.contentType,
          UTI: "com.adobe.pdf",
        });
      } catch (error) {
        await FileSystem.deleteAsync(fileUri, { idempotent: true });
        throw error;
      }
    },
  });
};
