import { Text } from "react-native";
import { useColorPalette } from "@/hooks/useColorPalette";
import { videosHeaderStyles } from "./Header.styles";

interface IHeader {
  siteId: string | undefined;
}

const Header = ({ siteId }: IHeader) => {
  const colorPalette = useColorPalette();

  return (
    <>
      <Text style={[videosHeaderStyles.title, { color: colorPalette.text }]}>
        Site Videos
      </Text>
      <Text
        style={[videosHeaderStyles.subtitle, { color: colorPalette.secondary }]}
      >
        Site ID: {siteId ?? "Unknown"}
      </Text>
    </>
  );
};

export default Header;