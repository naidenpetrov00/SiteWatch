import { buildPtzBaseUrl, ptzDirectionSchema } from "../utils";

import DigestClient from "digest-fetch";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { useAuth } from "@/store/auth_context";
import { useInteropClassName } from "expo-router/build/link/useLinkHooks";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const startPtzMovementSchema = z.object({
  ipAddress: z.string().min(1),
  username: z.string().min(2),
  password: z.string().min(2),
  direction: ptzDirectionSchema,
  arg1: z.number().int().optional(),
  arg2: z.number().int().optional(),
  arg3: z.number().int().optional(),
});

export type StartPtzMovementInput = z.infer<typeof startPtzMovementSchema>;

export const startPtzMovement = async ({
  ipAddress,
  username,
  password,
  direction,
  arg1 = 0,
  arg2 = 5,
  arg3 = 0,
}: StartPtzMovementInput): Promise<void> => {
  const client = new DigestClient(username, password);
  const query = new URLSearchParams({
    action: "start",
    channel: "1",
    code: direction,
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
      `PTZ start failed (${response.status} ${response.statusText})${suffix}`
    );
  }
};

type UseStartPtzMovementOptions = {
  mutationConfig?: MutationConfig<typeof startPtzMovement>;
};

export const useStartPtzMovement = ({
  mutationConfig,
}: UseStartPtzMovementOptions = {}) => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationFn: (data) => startPtzMovement({ ...data }),
    ...mutationConfig,
  });
};
