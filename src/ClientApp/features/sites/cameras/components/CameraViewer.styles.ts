import { StyleSheet } from "react-native";

const cameraViewerStyles = StyleSheet.create({
  container: {
    flex: 1,
  },
  content: {
    flex: 1,
    paddingHorizontal: 20,
    paddingBottom: 20,
    marginTop: 24,
    gap: 24,
  },
  fullWidthCard: {
    marginHorizontal: -20,
  },
  cameraName: {
    fontSize: 20,
    fontWeight: "600",
  },
});

export default cameraViewerStyles;
