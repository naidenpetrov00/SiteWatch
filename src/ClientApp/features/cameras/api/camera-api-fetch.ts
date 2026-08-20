import * as FileSystem from "expo-file-system/legacy";
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

export const cameraApiDownload = async (
  path: string,
  accessToken: string,
  fileUri: string,
) => {
  const result = await FileSystem.downloadAsync(
    `${env.API_URL}${path}`,
    fileUri,
    {
      headers: {
        Accept: "image/jpeg",
        Authorization: `Bearer ${accessToken}`,
      },
    },
  );

  if (result.status < 200 || result.status >= 300) {
    await FileSystem.deleteAsync(fileUri, { idempotent: true });
    throw new Error(`Camera request failed with status ${result.status}.`);
  }

  return result;
};
