import { StyleSheet } from "react-native";

export const ATTACHMENT_GRID_GAP = 12;
export const ATTACHMENT_GRID_HORIZONTAL_PADDING = 20;

const styles = StyleSheet.create({
  content: { gap: ATTACHMENT_GRID_GAP, padding: ATTACHMENT_GRID_HORIZONTAL_PADDING },
  column: { gap: ATTACHMENT_GRID_GAP },
  tile: {
    aspectRatio: 0.85,
    alignItems: "center",
    borderRadius: 12,
    justifyContent: "center",
    overflow: "hidden",
  },
  tilePressed: { opacity: 0.82 },
  image: { height: "100%", width: "100%" },
  placeholder: { alignItems: "center", gap: 8, justifyContent: "center", padding: 16 },
  fileName: { fontSize: 13, fontWeight: "700", textAlign: "center" },
  fileType: { fontSize: 11, textAlign: "center" },
  durationOverlay: {
    ...StyleSheet.absoluteFillObject,
    alignItems: "flex-end",
    justifyContent: "flex-end",
    padding: 8,
  },
  durationBadge: {
    backgroundColor: "rgba(0, 0, 0, 0.65)",
    borderRadius: 999,
    paddingHorizontal: 8,
    paddingVertical: 4,
  },
  durationText: { color: "#fff", fontSize: 12, fontWeight: "700" },
});

export default styles;
