import React, {useRef} from "react";
import {Text, View} from "react-native";
import {VLCPlayer, VlCPlayerView} from "react-native-vlc-media-player";

import {cameraStreamStyles} from "./CameraStream.styles";
import {useColorPalette} from "@/hooks/useColorPalette";

interface CameraStreamProps {
    label?: string;
}

// const videoSource =
//     "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4";
const videoSource =
    "rtsp://admin:L264B20A@192.168.100.23:554/cam/realmonitor?channel=1&subtype=0";

const CameraStream: React.FC<CameraStreamProps> = ({
                                                       label = "Camera Stream",
                                                   }) => {
    const colorPalette = useColorPalette();
    const sourceRef = useRef(videoSource);
    const renderCount = React.useRef(0);
    renderCount.current++;

    return (
        <View
            style={[
                cameraStreamStyles.streamWrapper,
                {borderColor: colorPalette.primary},
            ]}
        >
            <VLCPlayer
                style={cameraStreamStyles.video}
                autoAspectRatio={true}
                source={{
                    uri: sourceRef.current,
                }}
                resizeMode="fill"
            />
            {/*<VlCPlayerView*/}
            {/*    autoplay={true}*/}
            {/*    url="https://www.radiantmediaplayer.com/media/big-buck-bunny-360p.mp4"*/}
            {/*    ggUrl=""*/}
            {/*    showGG={true}*/}
            {/*    showTitle={true}*/}
            {/*    title="Big Buck Bunny"*/}
            {/*    showBack={true}*/}
            {/*    onLeftPress={() => {*/}
            {/*    }}*/}
            {/*/>*/}
            <Text
                style={[cameraStreamStyles.streamLabel, {color: colorPalette.text, zIndex: 10000}]}
            >
                {renderCount.current}
            </Text>
        </View>
    );
};

export default CameraStream;
