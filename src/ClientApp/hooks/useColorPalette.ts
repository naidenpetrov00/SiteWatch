import { Colors } from "@/config/constants/Colors";
import { useColorScheme } from "react-native";

export const useColorPalette = () => {
  const colorScheme = useColorScheme();
  return colorScheme === "dark" ? Colors.dark : Colors.light;
};
