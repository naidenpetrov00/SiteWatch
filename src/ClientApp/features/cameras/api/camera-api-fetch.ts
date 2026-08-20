import { fetch, type FetchRequestInit } from "expo/fetch";

import { env } from "@/config/env";

export const cameraApiFetch = async (
  path: string,
  accessToken: string,
  init: FetchRequestInit = {},
): Promise<Response> => {
  const headers = new Headers(init.headers);
  headers.set("Authorization", `Bearer ${accessToken}`);

  const response = await fetch(`${env.API_URL}${path}`, {
    ...init,
    headers,
  });

  if (!response.ok) {
    const payload = await response.json().catch(() => null);
    const detail =
      payload && typeof payload === "object" && "detail" in payload
        ? String(payload.detail)
        : `Camera request failed with status ${response.status}.`;
    throw new Error(detail);
  }

  return response;
};
