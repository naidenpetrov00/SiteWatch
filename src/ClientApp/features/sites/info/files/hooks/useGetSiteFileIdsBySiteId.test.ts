import { api } from "@/lib/api-client";

import { getSiteFileIdsBySiteId } from "./useGetSiteFileIdsBySiteId";

jest.mock("@/lib/api-client", () => ({ api: { get: jest.fn() } }));

describe("getSiteFileIdsBySiteId", () => {
  afterEach(() => jest.clearAllMocks());

  it("loads typed file metadata for the selected site with the bearer token", async () => {
    const files = [{ fileId: "file-1", fileName: "manual.pdf", contentType: "application/pdf", documentType: "InstallationOrOperationManual" }];
    jest.mocked(api.get).mockResolvedValue(files);

    await expect(getSiteFileIdsBySiteId({
      siteId: "11111111-1111-1111-1111-111111111111",
      accessToken: "header.payload.signature",
    })).resolves.toEqual(files);

    expect(api.get).toHaveBeenCalledWith(
      "/files/files11111111-1111-1111-1111-111111111111",
      { headers: { Authorization: "Bearer header.payload.signature" } },
    );
  });
});
