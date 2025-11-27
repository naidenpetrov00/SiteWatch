import {StyleSheet} from "react-native";

const siteCardStyles = StyleSheet.create({
    card: {
        borderWidth: 1,
        borderRadius: 16,
        paddingVertical: 16,
        paddingHorizontal: 18,
        marginBottom: 12,
        shadowColor: "#000",
        shadowOpacity: 0.08,
        shadowRadius: 6,
        shadowOffset: {width: 0, height: 2},
        elevation: 2,
    },
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
});

export default siteCardStyles;
