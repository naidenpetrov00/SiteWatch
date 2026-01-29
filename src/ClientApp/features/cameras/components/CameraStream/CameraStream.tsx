import { ChannelType, PlayerHandle } from "../../types";

import { Camera } from "../../api/models";
import Player from "./Player";
import React from "react";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
  joystick?: React.ReactNode;
  channel: ChannelType;
  isRecording: boolean;
  onRecordingChange?: (nextIsRecording: boolean) => void;
  playerRef?: React.Ref<PlayerHandle>;
  playerKey: number;
  setPlayerKey: React.Dispatch<React.SetStateAction<number>>;
}

const CameraStream: React.FC<CameraStreamProps> = ({
  camera,
  joystick,
  channel,
  isRecording,
  onRecordingChange,
  playerRef,
  playerKey,
  setPlayerKey,
}) => {
  const rtsp = useGetRtspUrl(camera, channel);

  return (
    <Player
      playerKey={playerKey}
      setPlayerKey={setPlayerKey}
      ref={playerRef}
      rtsp={rtsp}
      joystick={joystick}
      isRecording={isRecording}
      onRecordingChange={onRecordingChange}
    />
  );
};

export default CameraStream;
