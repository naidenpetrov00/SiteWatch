import { ChannelType, PlayerHandle } from "@/features/cameras/types";
import React, { useRef, useState } from "react";
import { Platform, ScrollView, View } from "react-native";
import { Stack, useLocalSearchParams, useRouter } from "expo-router";

import CameraJoystick from "@/features/cameras/components/CameraJoystick/CameraJoystick";
import CameraManagmentCard from "@/features/cameras/components/CameraHdCard/CameraManagmentCard";
import CameraStream from "@/features/cameras/components/CameraStream/CameraStream";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";
import useGetCameraFromCacheOrApi from "@/features/cameras/hooks/useGetCameraFromCacheOrApi";

const CameraScreen = () => {
  const [isRecording, setIsRecording] = useState(false);
  const [playerKey, setPlayerKey] = useState(0);
  const playerRef = useRef<PlayerHandle>(null);
  const router = useRouter();
  const { cameraId, siteId, siteName } = useLocalSearchParams<{
    cameraId: string;
    siteId: string;
    siteName?: string;
  }>();
  const [channel, setChannel] = useState(ChannelType.Sub);
  const camera = useGetCameraFromCacheOrApi(siteId, cameraId);
  const handleBackPress = () => {
    if (router.canGoBack()) {
      router.back();
      return;
    }

    if (siteId) {
      router.replace({
        pathname: "/Site/[siteId]/Cameras",
        params: { siteId, siteName },
      });
      return;
    }

    router.replace("/(app)/(tabs)/Sites");
  };

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
          ...(Platform.OS === "ios"
            ? {
                headerBackVisible: false,
                unstable_headerLeftItems: () => [
                  {
                    type: "button",
                    label: siteName ?? "Cameras",
                    icon: { type: "sfSymbol", name: "chevron.left" },
                    onPress: handleBackPress,
                    sharesBackground: true,
                  },
                ],
              }
            : {}),
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
