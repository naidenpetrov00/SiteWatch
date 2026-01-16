import { z } from "zod";

export const ptzDirectionSchema = z.enum(["Up", "Down", "Left", "Right"]);

export type PtzDirection = z.infer<typeof ptzDirectionSchema>;

export const buildPtzBaseUrl = (ipAddress: string, query: string) =>
  `http://${ipAddress}/cgi-bin/ptz.cgi?${query}`;

export const buildSnapshotBaseUrl = (ipAddress: string, query: string) =>
  `http://${ipAddress}/cgi-bin/snapshot.cgi?${query}`;
