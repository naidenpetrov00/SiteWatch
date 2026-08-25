import { MutationConfig } from "@/lib/react-query";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

import { cameraApiFetch } from "./camera-api-fetch";
import { logPtzMetric, type PtzMetricContext } from "../latency-metrics";
import { ptzDirectionSchema } from "../utils";

export const startPtzMovementSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  direction: ptzDirectionSchema,
});

export type StartPtzMovementInput = z.infer<typeof startPtzMovementSchema> & {
  metric?: PtzMetricContext;
};

const startPtzMovement = async (
  { cameraId, direction, metric }: StartPtzMovementInput,
  accessToken: string,
): Promise<void> => {
  logPtzMetric(metric, "request_start", "start");
  try {
    await cameraApiFetch(paths.cameras.startPtzMovement(cameraId), accessToken, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ direction }),
    });
    logPtzMetric(metric, "api_response", "start");
  } catch (error) {
    logPtzMetric(metric, "failure", "start");
    throw error;
  }
};

type StartPtzMovementMutation = (
  input: StartPtzMovementInput,
) => Promise<void>;

type UseStartPtzMovementOptions = {
  mutationConfig?: MutationConfig<StartPtzMovementMutation>;
};

export const useStartPtzMovement = ({
  mutationConfig,
}: UseStartPtzMovementOptions = {}) => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationFn: (data: StartPtzMovementInput) => {
      if (!accessToken) throw new Error("Authentication required.");
      return startPtzMovement(data, accessToken);
    },
    ...mutationConfig,
  });
};
