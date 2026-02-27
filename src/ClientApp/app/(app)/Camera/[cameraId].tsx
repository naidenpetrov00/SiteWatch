import { ChannelType, PlayerHandle } from "@/features/cameras/types";
import React, { useRef, useState } from "react";
import { ScrollView, View } from "react-native";
import { Stack, useLocalSearchParams } from "expo-router";

import CameraJoystick from "@/features/cameras/components/CameraJoystick/CameraJoystick";
import CameraManagmentCard from "@/features/cameras/components/CameraHdCard/CameraManagmentCard";
import CameraStream from "@/features/cameras/components/CameraStream/CameraStream";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";
import useGetCameraFromCacheOrApi from "@/features/cameras/hooks/useGetCameraFromCacheOrApi";

const CameraScreen = () => {
  const [isRecording, setIsRecording] = useState(false);
  const [playerKey, setPlayerKey] = useState(0);
  const playerRef = useRef<PlayerHandle>(null);
  const { cameraId, siteId } = useLocalSearchParams<{
    cameraId: string;
    siteId: string;
  }>();
  const [channel, setChannel] = useState(ChannelType.Sub);
  const camera = useGetCameraFromCacheOrApi(siteId, cameraId);

  return (
    // <SafeAreaView
    //   style={[
    //     cameraViewerStyles.container,
    //     { backgroundColor: colorPalette.background },
    //   ]}
    // >
    <>
      <Stack.Screen
        options={{
          headerTitle: camera?.name,
          headerRight:() => <HeaderButtons/> 
        }}
      />
      <ScrollView>
        <CameraStream
        playerKey={playerKey}
          channel={channel}
          camera={camera}
          setPlayerKey={setPlayerKey}
          joystick={
            <CameraJoystick
              camera={camera}
              wrapperStyle={{ flex: 0 }}
              joystickStyle={{ width: 180, height: 180, paddingVertical: 20 }}
            />
          }
          isRecording={isRecording}
          onRecordingChange={setIsRecording}
          playerRef={playerRef}
        />
        <CameraManagmentCard
          channel={channel}
          setChannel={setChannel}
          camera={camera}
          isRecording={isRecording}
          setPlayerKey={setPlayerKey}
          onToggleRecording={() => playerRef.current?.toggleRecording()}
        />
        <View style={cameraViewerStyles.content}>
          <View></View>
          <CameraJoystick camera={camera} />
        </View>
      </ScrollView>
    </>
    // </SafeAreaView>
  );
};

export default CameraScreen;

import { Button, Host, HStack, Image } from "@expo/ui/swift-ui";
import {
  cornerRadius,
  frame,
  glassEffect,
  padding,
} from "@expo/ui/swift-ui/modifiers";

function HeaderButtons() {
  return (
    <View style={{ paddingTop: 200, flex: 1, backgroundColor: "black" }}>
      <Host matchContents>
        <HStack spacing={12}>
          <Button
            onPress={() => console.log("Undo")}
            modifiers={[
              frame({ width: 44, height: 44 }),
              cornerRadius(18),
              glassEffect({ glass: { variant: "regular" } }),
            ]}
          >
            <Image systemName="arrow.uturn.backward" />
          </Button>

          <HStack
            spacing={0}
            modifiers={[
              cornerRadius(18),
              glassEffect({ glass: { variant: "regular" } }),
            ]}
          >
            <Button
              onPress={() => console.log("Share")}
              modifiers={[
                frame({ height: 44, minWidth: 44 }),
                padding({ horizontal: 12 }),
              ]}
            >
              <Image systemName="square.and.arrow.up" />
            </Button>

            <Button
              onPress={() => console.log("More")}
              modifiers={[
                frame({ height: 44, minWidth: 44 }),
                padding({ horizontal: 12 }),
              ]}
            >
              <Image systemName="ellipsis" />
            </Button>
          </HStack>
        </HStack>
      </Host>
    </View>
  );
}
