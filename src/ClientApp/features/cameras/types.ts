export enum ChannelType {
  Main = 0,
  Sub = 1,
}

export enum Brand {
  Unknown = "Unknown",
  Dahua = "Dahua",
}

export const CAMERA_BRAND_OPTIONS = [Brand.Dahua] as const;
export type SupportedCameraBrand = (typeof CAMERA_BRAND_OPTIONS)[number];

export interface PlayerHandle {
  toggleRecording: () => Promise<void>;
}
