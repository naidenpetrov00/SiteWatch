import { api } from "@/lib/api-client";

import { getSiteInvoices } from "./useGetSiteInvoices";

jest.mock("@/lib/api-client", () => ({ api: { get: jest.fn() } }));

describe("getSiteInvoices", () => {
  afterEach(() => jest.clearAllMocks());

  it("requests invoices for the selected site with the authenticated bearer token", async () => {
    const invoices = [{ id: "invoice-1" }];
    jest.mocked(api.get).mockResolvedValue(invoices);

    await expect(getSiteInvoices({
      siteId: "11111111-1111-1111-1111-111111111111",
      accessToken: "header.payload.signature",
    })).resolves.toEqual(invoices);

    expect(api.get).toHaveBeenCalledWith(
      "/invoices/site/11111111-1111-1111-1111-111111111111",
      { headers: { Authorization: "Bearer header.payload.signature" } },
    );
  });
});
