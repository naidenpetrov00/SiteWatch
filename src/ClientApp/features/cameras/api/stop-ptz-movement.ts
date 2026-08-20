import { MutationConfig } from "@/lib/react-query";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

import { cameraApiFetch } from "./camera-api-fetch";
import { ptzDirectionSchema } from "../utils";

export const stopPtzMovementSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  direction: ptzDirectionSchema,
});

export type StopPtzMovementInput = z.infer<typeof stopPtzMovementSchema>;

const stopPtzMovement = async (
  { cameraId, direction }: StopPtzMovementInput,
  accessToken: string,
): Promise<void> => {
  await cameraApiFetch(paths.cameras.stopPtzMovement(cameraId), accessToken, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ direction }),
  });
};

type StopPtzMovementMutation = (
  input: StopPtzMovementInput,
) => Promise<void>;

type UseStopPtzMovementOptions = {
  mutationConfig?: MutationConfig<StopPtzMovementMutation>;
};

export const useStopPtzMovement = ({
  mutationConfig,
}: UseStopPtzMovementOptions = {}) => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationFn: (data: StopPtzMovementInput) => {
      if (!accessToken) throw new Error("Authentication required.");
      return stopPtzMovement(data, accessToken);
    },
    ...mutationConfig,
  });
};
