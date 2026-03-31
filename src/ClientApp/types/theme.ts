import { Theme } from "@react-navigation/native";

export interface CustomTheme extends Theme {}

export type ColorPalette = {
  text: string;
  contrastText: string;
  placeholderText: string;
  background: string;
  primary: string;
  secondary: string;
  icon: string;
  tabIconDefault: string;
  tabIconSelected: string;
};
