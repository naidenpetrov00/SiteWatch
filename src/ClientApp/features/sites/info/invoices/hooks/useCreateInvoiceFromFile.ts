import { useMutation, useMutationState, useQueryClient } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { env } from "@/config/env";
import { useAuth } from "@/store/auth_context";
import type { InvoiceUploadAsset } from "../types";

export type CreateInvoiceFromFileRequest = {
  siteId: string;
  file: InvoiceUploadAsset;
};

export type CreateInvoiceFromFileResponse = {
  id: string;
};

export type PendingInvoiceUpload = {
  mutationId: number;
  request: CreateInvoiceFromFileRequest;
};

const getUploadErrorMessage = async (response: Response): Promise<string> => {
  if (response.status === 401) {
    return "Your session has expired. Sign in again and retry.";
  }

  if (response.status === 403) {
    return "Your account is not allowed to upload invoices for this site.";
  }

  if (response.status === 413) {
    return "The invoice file cannot exceed 20 MB.";
  }

  const payload = await response.json().catch(() => null) as {
    detail?: string;
    details?: { message?: string }[];
    errors?: Record<string, string[]>;
  } | null;
  const validationMessage = payload?.errors
    ? Object.values(payload.errors).flat().find((message) => message.trim().length > 0)
    : payload?.details?.find((detail) => detail.message?.trim().length)?.message;

  return validationMessage ?? payload?.detail ?? "Unable to upload the invoice.";
};

export const useCreateInvoiceFromFile = () => {
  const { accessToken } = useAuth();
  const queryClient = useQueryClient();

  return useMutation({
    mutationKey: ["invoice", "create-from-file"],
    mutationFn: async ({ siteId, file }: CreateInvoiceFromFileRequest) => {
      if (!accessToken) {
        throw new Error("Authentication is required to upload an invoice.");
      }

      const formData = new FormData();
      formData.append(
        "file",
        {
          uri: file.uri,
          name: file.fileName,
          type: file.contentType,
        } as unknown as Blob,
      );

      let response: Response;
      try {
        response = await fetch(
          new URL(paths.invoices.createFromFile(siteId), env.API_URL).toString(),
          {
            method: "POST",
            headers: { Authorization: `Bearer ${accessToken}` },
            body: formData,
          },
        );
      } catch {
        throw new Error("Unable to reach the server. Check your connection and retry.");
      }

      if (!response.ok) {
        throw new Error(await getUploadErrorMessage(response));
      }

      return await response.json() as CreateInvoiceFromFileResponse;
    },
    onSuccess: async (_response, request) => {
      await queryClient.invalidateQueries({
        queryKey: ["site-invoices", request.siteId],
      });
    },
  });
};

export const usePendingInvoiceUploads = (siteId?: string) => {
  const mutations = useMutationState({
    filters: { mutationKey: ["invoice", "create-from-file"], status: "pending" },
    select: (mutation): PendingInvoiceUpload => ({
      mutationId: mutation.mutationId,
      request: mutation.state.variables as CreateInvoiceFromFileRequest,
    }),
  });

  return mutations.filter(({ request }) => request.siteId === siteId);
};
