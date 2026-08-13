import { StyleSheet } from "react-native";

export const filtersStyles = StyleSheet.create({
  filterScroller: {
    flexGrow: 0,
    flexShrink: 0,
    height: 46,
    marginBottom: 18,
  },
  filters: {
    alignItems: "center",
    flexDirection: "row",
    gap: 10,
    paddingRight: 16,
  },
  filterChip: {
    alignItems: "center",
    borderWidth: 1,
    borderRadius: 999,
    height: 44,
    justifyContent: "center",
    paddingHorizontal: 14,
  },
  filterText: {
    fontSize: 14,
    fontWeight: "600",
    lineHeight: 20,
  },
});
