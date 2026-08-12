import { ActivityIndicator, Text, View } from "react-native";
import Animated, { FadeInDown } from "react-native-reanimated";

import { useColorPalette } from "@/hooks/useColorPalette";
import { FILE_DOCUMENT_TYPE_LABELS, type FileDocumentType } from "../files/types";
import filesStyles from "../files/component/Files/Files.styles";
import withOpacity from "./withOpacity";

type PendingFileUploadRowProps = {
  contentType: string;
  documentType: FileDocumentType;
  fileName: string;
};

const PendingFileUploadRow = ({
  contentType,
  documentType,
  fileName,
}: PendingFileUploadRowProps) => {
  const colorPalette = useColorPalette();

  return (
    <Animated.View
      entering={FadeInDown.duration(180)}
      accessibilityLabel={`${fileName} uploading`}
      style={[
        filesStyles.fileRow,
        {
          backgroundColor: withOpacity(colorPalette.secondary, 0.13),
          borderColor: withOpacity(colorPalette.secondary, 0.53),
          gap: 4,
        },
      ]}
    >
      <ActivityIndicator color={colorPalette.primary} />
      <Text style={[filesStyles.fileName, { color: colorPalette.text }]}>
        {fileName}
      </Text>
      <Text style={[filesStyles.metadata, { color: colorPalette.secondary }]}>
        Uploading…
      </Text>
      <Text style={[filesStyles.metadata, { color: colorPalette.secondary }]}>
        Document type: {FILE_DOCUMENT_TYPE_LABELS[documentType]}
      </Text>
      <Text style={[filesStyles.metadata, { color: colorPalette.secondary }]}>
        Content type: {contentType}
      </Text>
    </Animated.View>
  );
};

export default PendingFileUploadRow;
