import { DefaultTheme, Theme } from "@react-navigation/native";

const MyTheme = {
  ...DefaultTheme,
  colors: {
    ...DefaultTheme.colors,
    background: "#1c2120",
    primary: "#feb06a",
    secondary: "#d9d9d9",
  },
} as Theme;

export const useCustomTheme = () => {
  return MyTheme;
};
