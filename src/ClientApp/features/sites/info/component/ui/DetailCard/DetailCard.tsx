import { StyleProp, ViewStyle } from "react-native";

import Card from "@/components/ui/Card/Card";
import { ReactNode } from "react";
import detailCardStyles from "./DetailCard.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useRouter } from "expo-router";

interface IDetailCard {
  path: string | undefined;
  siteId: string | undefined;
  children: ReactNode;
  style?: StyleProp<ViewStyle>;
}

const DetailCard = ({ path, siteId, children, style }: IDetailCard) => {
  const colorPalette = useColorPalette();
  const router = useRouter();

  const handleOnPress = () => {
    if (!siteId) {
      console.warn("siteId missing, cannot navigate");
      return;
    }

    router.push({
      pathname: `/Site/[siteId]/${path}`,
      params: { siteId },
    });
  };
  return (
    <Card
      backgroundColor={colorPalette.background}
      borderColor={colorPalette.primary}
      onPress={handleOnPress}
      style={[detailCardStyles.card, style]}
    >
      {children}
    </Card>
  );
};

export default DetailCard;
