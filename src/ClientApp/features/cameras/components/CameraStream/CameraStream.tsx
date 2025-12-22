import React, { useState } from "react";
import { Text, View } from "react-native";

import { Camera } from "../../api/models";
import { ChannelType } from "../../types";
import { VLCPlayer } from "react-native-vlc-media-player";
import { cameraStreamStyles } from "./CameraStream.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import useGetRtspUrl from "../../hooks/useGetRtspUrl";

interface CameraStreamProps {
  camera: Camera;
}

const CameraStream: React.FC<CameraStreamProps> = async ({ camera }) => {
  const colorPalette = useColorPalette();
  const rtsp = useGetRtspUrl(camera, ChannelType.Sub);

  const [orientation, setOrientation] = useState();
  const renderCount = React.useRef(0);
  renderCount.current++;

  return (
    <View style={[cameraStreamStyles.streamWrapper]}>
      <VLCPlayer
        style={cameraStreamStyles.video}
        autoAspectRatio={true}
        muted={true}
        source={{
          uri: rtsp,
          initOptions: ["--rtsp-tcp"],
        }}
        resizeMode="fill"
      />
      <Text
        style={[
          cameraStreamStyles.streamLabel,
          { color: colorPalette.text, zIndex: 10000 },
        ]}
      >
        {renderCount.current}
      </Text>
    </View>
  );
};

export default CameraStream;

// import React from "react";
// import {
//   Modal,
//   Pressable,
//   StatusBar,
//   StyleSheet,
//   useWindowDimensions,
//   View,
// } from "react-native";
// import { VLCPlayer } from "react-native-vlc-media-player";
// import * as ScreenOrientation from "expo-screen-orientation";

// type Props = { rtsp: string };

// export default function CameraStream({ rtsp }: Props) {
//   const { width, height } = useWindowDimensions();
//   const isLandscapeBySize = width > height;

//   const [isFullscreen, setIsFullscreen] = React.useState(false);
//   const [overlayVisible, setOverlayVisible] = React.useState(false);

//   // Keep fullscreen state in sync with rotation
//   React.useEffect(() => {
//     setIsFullscreen(isLandscapeBySize);
//     if (!isLandscapeBySize) setOverlayVisible(false);
//   }, [isLandscapeBySize]);

//   // Optional: lock orientation while fullscreen (feels like DMSS)
//   React.useEffect(() => {
//     let mounted = true;

//     (async () => {
//       if (!mounted) return;

//       if (isFullscreen) {
//         await ScreenOrientation.lockAsync(
//           ScreenOrientation.OrientationLock.LANDSCAPE
//         );
//       } else {
//         await ScreenOrientation.lockAsync(
//           ScreenOrientation.OrientationLock.PORTRAIT_UP
//         );
//       }
//     })();

//     return () => {
//       mounted = false;
//     };
//   }, [isFullscreen]);

//   // Auto-hide overlay after 3s
//   React.useEffect(() => {
//     if (!overlayVisible) return;
//     const t = setTimeout(() => setOverlayVisible(false), 3000);
//     return () => clearTimeout(t);
//   }, [overlayVisible]);

//   const Player = (
//     <Pressable
//       style={isFullscreen ? styles.fullscreenWrap : styles.inlineWrap}
//       onPress={() => setOverlayVisible((v) => !v)}
//     >
//       <VLCPlayer
//         style={styles.video}
//         source={{ uri: rtsp }}
//         autoAspectRatio
//         resizeMode="contain"
//         initOptions={[
//           "--rtsp-tcp",
//           "--network-caching=300",
//           "--rtsp-caching=300",
//           "--live-caching=300",
//           "--clock-jitter=0",
//           "--clock-synchro=0",
//         ]}
//       />

//       {/* Your DMSS-like overlay buttons */}
//       {overlayVisible && (
//         <View style={styles.overlay}>
//           {/* put your buttons here */}
//           {/* e.g. <PTZControls /> <BackButton /> <HDButton /> etc */}
//         </View>
//       )}
//     </Pressable>
//   );

//   // LANDSCAPE: fullscreen modal
//   if (isFullscreen) {
//     return (
//       <Modal visible animationType="fade" presentationStyle="fullScreen">
//         <StatusBar hidden />
//         {Player}
//       </Modal>
//     );
//   }

//   // PORTRAIT: inline
//   return (
//     <View>
//       <StatusBar hidden={false} />
//       {Player}
//     </View>
//   );
// }

// const styles = StyleSheet.create({
//   inlineWrap: {
//     width: "100%",
//     aspectRatio: 16 / 9,
//     backgroundColor: "black",
//   },
//   fullscreenWrap: {
//     flex: 1,
//     backgroundColor: "black",
//   },
//   video: {
//     width: "100%",
//     height: "100%",
//   },
//   overlay: {
//     ...StyleSheet.absoluteFillObject,
//     justifyContent: "center",
//     alignItems: "center",
//     // backgroundColor: "rgba(0,0,0,0.2)", // optional dim
//   },
// });
