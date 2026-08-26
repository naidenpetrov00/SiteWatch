import { Platform } from "react-native";
import type { VLCPlayerSource } from "react-native-vlc-media-player";

export type VlcRtspProfileId =
  | "vlc-tcp-500"
  | "vlc-tcp-300"
  | "vlc-udp-500";

type VlcRtspProfile = {
  networkCachingMs: number;
  transport: "tcp" | "udp";
};

export const VLC_RTSP_PROFILES = {
  "vlc-tcp-500": {
    networkCachingMs: 500,
    transport: "tcp",
  },
  "vlc-tcp-300": {
    networkCachingMs: 300,
    transport: "tcp",
  },
  "vlc-udp-500": {
    networkCachingMs: 500,
    transport: "udp",
  },
} as const satisfies Readonly<Record<VlcRtspProfileId, VlcRtspProfile>>;

export const SELECTED_VLC_RTSP_PROFILE_ID: VlcRtspProfileId = "vlc-tcp-500";

type VlcRtspSource = VLCPlayerSource & {
  mediaOptions?: string[];
};

/**
 * react-native-vlc-media-player 1.0.98 iterates Android initOptions and
 * mediaOptions only to `size() - 1`. Repeating the final valid option makes
 * the wrapper discard the duplicate while every intended option reaches
 * LibVLC. Remove this guard after an authorized wrapper fix or upgrade.
 */
const guardAndroidWrapperOptions = (options: readonly string[]): string[] => {
  const lastOption = options[options.length - 1];

  return lastOption ? [...options, lastOption] : [];
};

export const createVlcRtspSource = (uri: string): VlcRtspSource => {
  if (Platform.OS !== "android") {
    // Keep the existing iOS behavior; the comparison profiles target Android.
    return {
      uri,
      initOptions: ["--rtsp-tcp"],
    };
  }

  const profile = VLC_RTSP_PROFILES[SELECTED_VLC_RTSP_PROFILE_ID];
  const initOptions = profile.transport === "tcp" ? ["--rtsp-tcp"] : [];
  // Applying this before setMedia keeps LibVLC's automatic, non-forced
  // hardware decoder from substituting its 1500 ms Android media cache.
  const mediaOptions = [`:network-caching=${profile.networkCachingMs}`];

  return {
    uri,
    initType: 2,
    initOptions: guardAndroidWrapperOptions(initOptions),
    mediaOptions: guardAndroidWrapperOptions(mediaOptions),
  };
};
