import { buildPtzBaseUrl } from "../utils";

import DigestClient from "digest-fetch";
import { MutationConfig } from "@/lib/react-query";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const moveRelativePtzSchema = z.object({
  ipAddress: z.string().min(1),
  username: z.string().min(2),
  password: z.string().min(2),
  arg1: z.number().optional(),
  arg2: z.number().optional(),
  arg3: z.number().optional(),
});

export type MoveRelativePtzInput = z.infer<typeof moveRelativePtzSchema>;

export const moveRelativePtz = async ({
  ipAddress,
  username,
  password,
  arg1 = 0,
  arg2 = 0,
  arg3 = 0,
}: MoveRelativePtzInput): Promise<void> => {
  const client = new DigestClient(username, password);
  const query = new URLSearchParams({
    action: "moveRelatively",
    channel: "1",
    arg1: String(arg1),
    arg2: String(arg2),
    arg3: String(arg3),
  }).toString();
  const url = buildPtzBaseUrl(ipAddress, query);
  const response = await client.fetch(url);

  if (!response.ok) {
    const responseText = await response.text().catch(() => "");
    const suffix = responseText ? `: ${responseText}` : "";
    throw new Error(
      `PTZ relative move failed (${response.status} ${response.statusText})${suffix}`
    );
  }
};

type UseMoveRelativePtzOptions = {
  mutationConfig?: MutationConfig<typeof moveRelativePtz>;
};

export const useMoveRelativePtz = ({
  mutationConfig,
}: UseMoveRelativePtzOptions = {}) => {
  return useMutation({
    mutationFn: (data) => moveRelativePtz({ ...data }),
    ...mutationConfig,
  });
};
