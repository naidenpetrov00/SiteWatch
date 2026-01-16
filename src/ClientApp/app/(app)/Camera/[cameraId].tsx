import React, { useState } from "react";
import { ScrollView, Text, View } from "react-native";
import { Stack, useLocalSearchParams } from "expo-router";

import CameraHdCard from "@/features/cameras/components/CameraHdCard/CameraHdCard";
import CameraJoystick from "@/features/cameras/components/CameraJoystick/CameraJoystick";
import CameraManagmentCard from "@/features/cameras/components/CameraHdCard/CameraHdCard";
import CameraStream from "@/features/cameras/components/CameraStream/CameraStream";
import { ChannelType } from "@/features/cameras/types";
import { SafeAreaView } from "react-native-safe-area-context";
import cameraViewerStyles from "@/features/cameras/components/CameraViewer.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetCameraFromCacheOrApi from "@/features/cameras/hooks/useGetCameraFromCacheOrApi";

const CameraScreen = () => {
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
        }}
      />
      <ScrollView>
        <CameraStream
          channel={channel}
          camera={camera}
          joystick={
            <CameraJoystick
              camera={camera}
              wrapperStyle={{ flex: 0 }}
              joystickStyle={{ width: 180, height: 180, paddingVertical: 20 }}
            />
          }
        />
        <CameraManagmentCard channel={channel} setChannel={setChannel} />
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
