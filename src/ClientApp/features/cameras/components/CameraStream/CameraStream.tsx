import { Camera } from "../../api/models";
import { ChannelType } from "../../types";
import Player from "./Player";
import React from "react";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
}

const CameraStream: React.FC<CameraStreamProps> = ({ camera }) => {
  const rtsp = useGetRtspUrl(camera, ChannelType.Sub);

  return <Player rtsp={rtsp} />;
};

export default CameraStream;
