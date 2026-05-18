import { StyleSheet } from "react-native";

export const HORIZONTAL_PADDING = 16;
export const GRID_GAP = 12;

export const siteVideosStyles = StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: HORIZONTAL_PADDING,
  },
  galleryContent: {
    gap: GRID_GAP,
  },
  columnWrapper: {
    gap: GRID_GAP,
  },
  galleryTile: {
    aspectRatio: 0.85,
    borderRadius: 8,
    elevation: 3,
    shadowColor: "#000",
    shadowOpacity: 0.12,
    shadowRadius: 6,
    shadowOffset: { width: 0, height: 3 },
    overflow: "hidden",
    alignItems: "center",
    justifyContent: "center",
  },
  galleryImage: {
    width: "100%",
    height: "100%",
  },
  durationOverlay: {
    ...StyleSheet.absoluteFillObject,
    alignItems: "flex-end",
    justifyContent: "flex-end",
    padding: 8,
  },
  durationBadge: {
    alignItems: "center",
    borderRadius: 999,
    backgroundColor: "rgba(0, 0, 0, 0.65)",
    paddingHorizontal: 8,
    paddingVertical: 4,
  },
  durationText: {
    color: "#fff",
    fontSize: 12,
    fontWeight: "700",
    letterSpacing: 0.2,
  },
});
