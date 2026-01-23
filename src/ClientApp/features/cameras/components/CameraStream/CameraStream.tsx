import { Camera } from "../../api/models";
import { ChannelType } from "../../types";
import Player, { PlayerHandle } from "./Player";
import React from "react";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
  joystick?: React.ReactNode;
  channel: ChannelType;
  isRecording: boolean;
  onRecordingChange?: (nextIsRecording: boolean) => void;
  playerRef?: React.Ref<PlayerHandle>;
}

const CameraStream: React.FC<CameraStreamProps> = ({
  camera,
  joystick,
  channel,
  isRecording,
  onRecordingChange,
  playerRef,
}) => {
  const rtsp = useGetRtspUrl(camera, channel);

  return (
    <Player
      ref={playerRef}
      rtsp={rtsp}
      joystick={joystick}
      isRecording={isRecording}
      onRecordingChange={onRecordingChange}
    />
  );
};

export default CameraStream;
