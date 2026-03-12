import Constants from "expo-constants";
import { Platform } from "react-native";
import { z } from "zod";

const EnvSchema = z.object({
  API_URL: z.string().url(),
});

const extra = Constants.expoConfig?.extra ?? {};

const normalizeApiUrl = (apiUrl: string) => {
  const url = new URL(apiUrl);
  const isLocalBindAddress =
    url.hostname === "0.0.0.0" || url.hostname === "localhost";

  if (!isLocalBindAddress) {
    return url.toString().replace(/\/$/, "");
  }

  if (Platform.OS === "android") {
    url.hostname = "10.0.2.2";
  } else {
    url.hostname = "localhost";
  }

  return url.toString().replace(/\/$/, "");
};

const parsedEnv = EnvSchema.parse(extra);

export const env = {
  ...parsedEnv,
  API_URL: normalizeApiUrl(parsedEnv.API_URL),
};
