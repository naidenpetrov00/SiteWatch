import {StyleSheet} from "react-native";

const siteCardStyles = StyleSheet.create({
    content: {
        gap: 4,
    },
    title: {
        fontSize: 18,
        fontWeight: "600",
    },
    address: {
        fontSize: 14,
        opacity: 0.8,
    },
    companyStockBadge: {
        alignSelf: "flex-start",
        borderCurve: "continuous",
        borderRadius: 999,
        borderWidth: 1,
        marginTop: 4,
        paddingHorizontal: 10,
        paddingVertical: 4,
    },
    companyStockBadgeText: {
        fontSize: 12,
        fontWeight: "700",
    },
});

export default siteCardStyles;
