import { api } from "@/lib/api-client";

import { getInvoiceFileAccess } from "./useInvoiceFileAccess";

jest.mock("@/lib/api-client", () => ({ api: { get: jest.fn() } }));

describe("getInvoiceFileAccess", () => {
  afterEach(() => jest.clearAllMocks());

  it("uses the site-scoped temporary-access endpoint with the bearer token", async () => {
    const access = { url: "/invoices/file?ticket=protected", fileName: "invoice.pdf" };
    jest.mocked(api.get).mockResolvedValue(access);

    await expect(getInvoiceFileAccess(
      { siteId: "site-1", invoiceId: "invoice-1" },
      "header.payload.signature",
    )).resolves.toEqual(access);

    expect(api.get).toHaveBeenCalledWith(
      "/invoices/site/site-1/invoice-1/file-access",
      { headers: { Authorization: "Bearer header.payload.signature" } },
    );
  });
});
