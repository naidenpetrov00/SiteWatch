import { api } from "@/lib/api-client";

import { createAccount } from "./sign-in-accounts";
import { verifyEmail } from "./verify-email";

jest.mock("@/lib/api-client", () => ({ api: { post: jest.fn() } }));
jest.mock("@/config/env", () => ({ env: { API_URL: "https://api.example.test" } }));

describe("identity API functions", () => {
  afterEach(() => jest.clearAllMocks());

  it("posts sign-in credentials to the configured identity endpoint", async () => {
    jest.mocked(api.post).mockResolvedValue({});
    const data = { email: "ada@example.test", password: "Password1" };

    await createAccount({ data });

    expect(api.post).toHaveBeenCalledWith("/identity/signIn", data);
  });

  it("posts the e-mail verification token to the configured endpoint", async () => {
    jest.mocked(api.post).mockResolvedValue({});
    const data = { email: "ada@example.test", token: "123456" };

    await verifyEmail({ data });

    expect(api.post).toHaveBeenCalledWith("/identity/verifyEmail", data);
  });
});
