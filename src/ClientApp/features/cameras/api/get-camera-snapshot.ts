import { blobToDataUrl, buildSnapshotBaseUrl } from "../utils";

import DigestClient from "digest-fetch";
import { QueryConfig } from "@/lib/react-query";
import { useQuery } from "@tanstack/react-query";
import { z } from "zod";

export const getCameraSnapshotSchema = z.object({
  ipAddress: z.string().min(1),
  username: z.string().min(2),
  password: z.string().min(2),
  channel: z.number().int().optional(),
  type: z.number().int().optional(),
});

export type GetCameraSnapshotInput = z.infer<typeof getCameraSnapshotSchema>;

export const getCameraSnapshot = async ({
  ipAddress,
  username,
  password,
  channel = 1,
  type = 0,
}: GetCameraSnapshotInput): Promise<string> => {
  const client = new DigestClient(username, password);
  const query = new URLSearchParams({
    channel: String(channel),
    type: String(type),
  }).toString();
  const url = buildSnapshotBaseUrl(ipAddress, query);
  const response = await client.fetch(url);

  if (!response.ok) {
    const responseText = await response.text().catch(() => "");
    const suffix = responseText ? `: ${responseText}` : "";
    throw new Error(
      `PTZ start failed (${response.status} ${response.statusText})${suffix}`
    );
  }

  const blob = await response.blob();
  return await blobToDataUrl(blob);
};

type UseGetCameraSnapshotOptions = {
  data: GetCameraSnapshotInput;
  queryConfig?: QueryConfig<typeof getCameraSnapshot>;
};

export const useGetCameraSnapshot = ({
  data,
  queryConfig,
}: UseGetCameraSnapshotOptions) => {
  return useQuery({
    queryKey: ["camera-snapshot", data.ipAddress],
    queryFn: () => getCameraSnapshot(data),
    retry: false,
    refetchOnWindowFocus: false,
    staleTime: 5_000,
    gcTime: 20_000,
    ...queryConfig,
  });
};
