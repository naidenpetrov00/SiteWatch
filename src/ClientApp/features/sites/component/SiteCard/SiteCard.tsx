import {Text, View} from "react-native";

import React from "react";
import {Site} from "../../api/types";
import {useColorPalette} from "@/hooks/useColorPalette";
import siteCardStyles from "./SiteCard.styles";
import Card from "@/components/ui/Card/Card";

interface SiteCardProps {
    site: Site;
    onPress: (site: Site) => void;
}

const SiteCard = ({site, onPress}: SiteCardProps) => {
    const colorPalette = useColorPalette();

    return (
        <Card
            onPress={() => onPress(site)}
            backgroundColor={colorPalette.background}
            borderColor={colorPalette.primary}
        >
            <View style={siteCardStyles.content}>
                <Text style={[siteCardStyles.title, {color: colorPalette.text}]}>
                    {site.name}
                </Text>
                <Text style={[siteCardStyles.address, {color: colorPalette.text}]}>
                    {site.address}
                </Text>
            </View>
        </Card>
    );
};

export default SiteCard;
