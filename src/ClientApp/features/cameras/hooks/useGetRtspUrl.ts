import { Brand, ChannelType } from "../types";

import { Camera } from "@/features/cameras/api/models";

const useGetRtspUrl = (camera: Camera, channel: ChannelType): string => {
  switch (camera.brand) {
    case Brand.Dahua:
      return `rtsp://${camera.username}:${camera.password}@${camera.ipAddress}:${camera.port}/cam/realmonitor?channel=1&subtype=${channel}`;

    case Brand.Unknown:
    default:
      throw new Error(`Unsupported camera brand: ${camera.brand}`);
  }
};

export default useGetRtspUrl;
