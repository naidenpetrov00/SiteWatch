import { Text } from "react-native";
import { imagesHeaderStyles } from "./Header.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IHeader {
  siteId: string | undefined;
}

const Header = ({ siteId }: IHeader) => {
  const colorPalette = useColorPalette();

  return (
    <>
      <Text style={[imagesHeaderStyles.title, { color: colorPalette.text }]}>
        Site Images
      </Text>
      <Text
        style={[imagesHeaderStyles.subtitle, { color: colorPalette.secondary }]}
      >
        Site ID: {siteId ?? "Unknown"}
      </Text>
    </>
  );
};

export default Header;
