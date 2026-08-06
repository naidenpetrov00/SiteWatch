import { useMutation } from "@tanstack/react-query";

import { paths } from "@/config/constants/paths";
import { api } from "@/lib/api-client";
import { useAuth } from "@/store/auth_context";
import type { InvoiceFileAccess } from "../types";

type InvoiceFileAccessRequest = {
  siteId: string;
  invoiceId: string;
};

const getInvoiceFileAccess = (
  request: InvoiceFileAccessRequest,
  accessToken: string,
): Promise<InvoiceFileAccess> =>
  api.get(
    paths.invoices.getFileAccess(request.siteId, request.invoiceId),
    { headers: { Authorization: `Bearer ${accessToken}` } },
  );

export const useInvoiceFileAccess = () => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationKey: ["invoice-file-access"],
    mutationFn: (request: InvoiceFileAccessRequest) => {
      if (!accessToken) {
        throw new Error("Authentication is required to open invoice files.");
      }

      return getInvoiceFileAccess(request, accessToken);
    },
  });
};
