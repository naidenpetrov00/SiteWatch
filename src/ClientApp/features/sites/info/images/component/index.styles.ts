import { StyleSheet } from "react-native";

export const HORIZONTAL_PADDING = 16;
export const GRID_GAP = 12;

export const siteImagesStyles =  StyleSheet.create({
  container: {
    flex: 1,
    paddingHorizontal: HORIZONTAL_PADDING,
  },
  title: {
    fontSize: 24,
    fontWeight: "700",
  },
  subtitle: {
    marginTop: 4,
    marginBottom: 20,
    fontSize: 14,
  },
  filters: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 10,
    marginBottom: 18,
  },
  filterChip: {
    borderWidth: 1,
    borderRadius: 999,
    paddingHorizontal: 14,
    paddingVertical: 10,
  },
  filterText: {
    fontSize: 14,
    fontWeight: "600",
  },
  galleryContent: {
    gap: GRID_GAP,
  },
  columnWrapper: {
    gap: GRID_GAP,
  },
  galleryTile: {
    height: 170,
    borderRadius: 18,
    overflow: "hidden",
    justifyContent: "flex-end",
  },
  tileOverlay: {
    flex: 1,
    justifyContent: "flex-end",
    padding: 14,
    backgroundColor: "rgba(255,255,255,0.22)",
  },
  tileCategory: {
    fontSize: 12,
    fontWeight: "600",
    marginBottom: 6,
  },
  tileTitle: {
    fontSize: 16,
    fontWeight: "700",
  },
  emptyState: {
    borderWidth: 1,
    borderStyle: "dashed",
    borderRadius: 16,
    padding: 24,
    alignItems: "center",
  },
  emptyText: {
    fontSize: 14,
    textAlign: "center",
  },
});
