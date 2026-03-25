import { ReactNode } from "react";
import { StyleProp, View, ViewStyle } from "react-native";
import detailCardStyles from "./DetailCard.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IDetailCard {
  children: ReactNode;
  style?: StyleProp<ViewStyle>;
}

const DetailCard = ({ children, style }: IDetailCard) => {
  const colorPalette = useColorPalette();
  return (
    <View
      style={[
        detailCardStyles.card,
        {
          backgroundColor: colorPalette.background,
          borderColor: colorPalette.primary,
        },
        style,
      ]}
    >
      {children}
    </View>
  );
};

export default DetailCard;
