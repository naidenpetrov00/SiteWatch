import DigestClient from "digest-fetch";
import { MutationConfig } from "@/lib/react-query";
import { buildSnapshotBaseUrl } from "../utils";
import { useMutation } from "@tanstack/react-query";
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
}: GetCameraSnapshotInput): Promise<string> => {
  const client = new DigestClient(username, password);
  const query = new URLSearchParams({
    channel: String(channel),
    type: String(channel),
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

  const arrayBuffer = await response.arrayBuffer();
  const base64 = arrayBufferToBase64(arrayBuffer);

  return `data:image/jpeg;base64,${base64}`;
};

const arrayBufferToBase64 = (buffer: ArrayBuffer) => {
  return Buffer.from(buffer).toString("base64");
};

type UseGetCameraSnapshotOptions = {
  mutationConfig?: MutationConfig<typeof getCameraSnapshot>;
};

export const useGetCameraSnapshot = ({
  mutationConfig,
}: UseGetCameraSnapshotOptions = {}) => {
  return useMutation({
    mutationFn: (data) => getCameraSnapshot({ ...data }),
    ...mutationConfig,
  });
};
