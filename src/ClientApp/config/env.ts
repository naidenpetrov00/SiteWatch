// env.ts
import Constants from "expo-constants";
import { z } from "zod";

const EnvSchema = z.object({
  API_URL: z.string().url(),
});

const extra = Constants.expoConfig?.extra ?? {}; // for Expo Router
export const env = EnvSchema.parse(extra);
