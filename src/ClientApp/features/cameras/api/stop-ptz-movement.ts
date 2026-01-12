import { buildPtzBaseUrl, ptzDirectionSchema } from "../utils";

import DigestClient from "digest-fetch";
import { MutationConfig } from "@/lib/react-query";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const stopPtzMovementSchema = z.object({
  ipAddress: z.string().min(1),
  username: z.string().min(2),
  password: z.string().min(2),
  direction: ptzDirectionSchema,
  arg1: z.number().int().optional(),
  arg2: z.number().int().optional(),
  arg3: z.number().int().optional(),
});

export type StopPtzMovementInput = z.infer<typeof stopPtzMovementSchema>;

export const stopPtzMovement = async ({
  ipAddress,
  username,
  password,
  direction,
  arg1 = 0,
  arg2 = 0,
  arg3 = 0,
}: StopPtzMovementInput): Promise<void> => {
  const client = new DigestClient(username, password);
  const query = new URLSearchParams({
    action: "stop",
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
      `PTZ stop failed (${response.status} ${response.statusText})${suffix}`
    );
  }
};

type UseStopPtzMovementOptions = {
  mutationConfig?: MutationConfig<typeof stopPtzMovement>;
};

export const useStopPtzMovement = ({
  mutationConfig,
}: UseStopPtzMovementOptions = {}) => {
  return useMutation({
    mutationFn: (data) => stopPtzMovement({ ...data }),
    ...mutationConfig,
  });
};
