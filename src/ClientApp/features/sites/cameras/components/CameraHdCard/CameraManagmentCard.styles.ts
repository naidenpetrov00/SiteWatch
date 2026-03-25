import { StyleSheet } from "react-native";

const cameraManagmentCardStyles = StyleSheet.create({
  card: {
    width: "100%",
    borderWidth: 0,
    borderTopLeftRadius: 0,
    borderTopRightRadius: 0,
    borderBottomLeftRadius: 16,
    borderBottomRightRadius: 16,
  },
  buttonsRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    justifyContent:'space-between'
  },
  textButton: {
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderWidth: 0,
  },
  iconButton: {
    width: 40,
    height: 40,
    borderRadius: 12,
    alignItems: "center",
    justifyContent: "center",
  },
});

export default cameraManagmentCardStyles;
