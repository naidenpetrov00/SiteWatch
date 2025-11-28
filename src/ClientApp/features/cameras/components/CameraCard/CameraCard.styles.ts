import { StyleSheet } from "react-native";

export const cameraCardStyles = StyleSheet.create({
  card:{
    borderBottomLeftRadius:0,
    borderBottomRightRadius:0,
    paddingTop: 8,
    paddingBottom: 0,
    paddingLeft: 0,
    paddingRight: 0,
  },
  header: {
    paddingHorizontal: 12,
    paddingVertical: 8,
    flexDirection: "row",
    alignItems: "center",
  },
  title: {
    fontWeight: "600",
    fontSize: 14,
  },
  snapshotWrapper: {
    width: "100%",
    aspectRatio: 16 / 9, // "window" feel
    backgroundColor: "#111",
  },
  snapshot: {
    width: "100%",
    height: "100%",
  },
});
