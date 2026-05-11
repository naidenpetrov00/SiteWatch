import { StyleSheet } from "react-native";

export const imagePreviewModalStyles = StyleSheet.create({
  gestureRoot: {
    flex: 1,
  },
  container: {
    flex: 1,
    backgroundColor: "black",
  },
  header: {
    minHeight: 56,
    alignItems: "flex-end",
    justifyContent: "center",
    paddingHorizontal: 16,
    paddingTop: 8,
  },
  headerActions: {
    flexDirection: "row",
    gap: 10,
  },
  actionButton: {
    width: 44,
    height: 44,
    borderRadius: 22,
    alignItems: "center",
    justifyContent: "center",
  },
  actionButtonPressed: {
    opacity: 0.78,
  },
  imageWrapper: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
    overflow: "hidden",
  },
  image: {
    width: "100%",
    height: "100%",
  },
});
