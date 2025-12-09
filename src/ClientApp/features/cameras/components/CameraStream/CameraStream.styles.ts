import {StyleSheet} from "react-native";

export const cameraStreamStyles = StyleSheet.create({
    streamWrapper: {
        width: "100%",
        borderWidth: 2,
        borderRadius: 16,
        alignItems: "center",
        justifyContent: "center",
        backgroundColor: "rgba(0,0,0,0.2)",
        overflow: "hidden",
    },
    video: {
        width: "100%",
        aspectRatio: 16 / 10,
    },
    streamLabel: {
        fontSize: 18,
    },
});
