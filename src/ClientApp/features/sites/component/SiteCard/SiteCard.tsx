import {Pressable, Text, View} from "react-native";

import React from "react";
import {Site} from "../../api/types";
import {useColorPalette} from "@/hooks/useColorPalette";
import siteCardStyles from "./SiteCard.styles";

interface SiteCardProps {
    site: Site;
    onPress: (site: Site) => void;
}

const SiteCard = ({site, onPress}: SiteCardProps) => {
    const colorPalette = useColorPalette();

    return (
        <Pressable
            onPress={() => onPress(site)}
            style={({pressed}) => [
                siteCardStyles.card,
                {
                    backgroundColor: colorPalette.background,
                    borderColor: colorPalette.primary,
                },
                pressed && {opacity: 0.9, transform: [{scale: 0.98}]},
            ]}
        >
            <View style={siteCardStyles.content}>
                <Text style={[siteCardStyles.title, {color: colorPalette.text}]}>
                    {site.name}
                </Text>
                <Text style={[siteCardStyles.address, {color: colorPalette.text}]}>
                    {site.address}
                </Text>
            </View>
        </Pressable>
    );
};

export default SiteCard;
