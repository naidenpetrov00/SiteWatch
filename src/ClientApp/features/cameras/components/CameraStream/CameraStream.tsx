import { Camera } from "../../api/models";
import { ChannelType } from "../../types";
import Player from "./Player";
import React from "react";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
  joystick?: React.ReactNode;
  channel: ChannelType;
}

const CameraStream: React.FC<CameraStreamProps> = ({
  camera,
  joystick,
  channel,
}) => {
  const rtsp = useGetRtspUrl(camera, channel);

  return <Player rtsp={rtsp} joystick={joystick} />;
};

export default CameraStream;
