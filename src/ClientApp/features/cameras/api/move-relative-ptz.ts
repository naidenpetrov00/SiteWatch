import { MutationConfig } from "@/lib/react-query";
import { paths } from "@/config/constants/paths";
import { useAuth } from "@/store/auth_context";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

import { cameraApiFetch } from "./camera-api-fetch";
import { logPtzMetric, type PtzMetricContext } from "../latency-metrics";

export const moveRelativePtzSchema = z.object({
  cameraId: z.string().uuid("Invalid GUID format"),
  horizontal: z.number().min(-1).max(1),
  vertical: z.number().min(-1).max(1),
  zoom: z.number().min(-1).max(1),
});

export type MoveRelativePtzInput = z.infer<typeof moveRelativePtzSchema> & {
  metric?: PtzMetricContext;
};

const moveRelativePtz = async (
  { cameraId, horizontal, vertical, zoom, metric }: MoveRelativePtzInput,
  accessToken: string,
): Promise<void> => {
  logPtzMetric(metric, "request_start", "relative");
  try {
    await cameraApiFetch(paths.cameras.movePtzRelatively(cameraId), accessToken, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ horizontal, vertical, zoom }),
    });
    logPtzMetric(metric, "api_response", "relative");
  } catch (error) {
    logPtzMetric(metric, "failure", "relative");
    throw error;
  }
};

type MoveRelativePtzMutation = (
  input: MoveRelativePtzInput,
) => Promise<void>;

type UseMoveRelativePtzOptions = {
  mutationConfig?: MutationConfig<MoveRelativePtzMutation>;
};

export const useMoveRelativePtz = ({
  mutationConfig,
}: UseMoveRelativePtzOptions = {}) => {
  const { accessToken } = useAuth();

  return useMutation({
    mutationFn: (data: MoveRelativePtzInput) => {
      if (!accessToken) throw new Error("Authentication required.");
      return moveRelativePtz(data, accessToken);
    },
    ...mutationConfig,
  });
};
