import QRCode from "react-native-qrcode-svg";
import { Text, View } from "react-native";

import assetStyles from "../assets.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

type AssetQrCodeProps = {
  qrIdentifier: string;
  unavailableMessage?: string;
};

export default function AssetQrCode({ qrIdentifier, unavailableMessage }: AssetQrCodeProps) {
  const colors = useColorPalette();

  if (!qrIdentifier.trim()) {
    return (
      <View style={[assetStyles.qrSection, { borderColor: colors.secondary }]}>
        <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Asset QR code</Text>
        <Text style={[assetStyles.subtitle, { color: colors.text }]}>
          The QR identifier is unavailable.
        </Text>
      </View>
    );
  }

  return (
    <View style={[assetStyles.qrSection, { borderColor: colors.secondary }]}>
      <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Asset QR code</Text>
      <View
        accessible
        accessibilityLabel="Asset QR code"
        style={assetStyles.qrCodeFrame}
      >
        <QRCode
          backgroundColor="#FFFFFF"
          color="#000000"
          size={220}
          value={qrIdentifier}
        />
      </View>
      {unavailableMessage ? (
        <Text style={[assetStyles.subtitle, { color: colors.text }]}>{unavailableMessage}</Text>
      ) : null}
      <Text selectable style={[assetStyles.qrIdentifier, { color: colors.secondary }]}>
        {qrIdentifier}
      </Text>
    </View>
  );
}
