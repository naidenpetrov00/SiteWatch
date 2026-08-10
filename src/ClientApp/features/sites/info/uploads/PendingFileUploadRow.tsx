import { ActivityIndicator, Text, View } from "react-native";

import { useColorPalette } from "@/hooks/useColorPalette";
import { FILE_DOCUMENT_TYPE_LABELS, type FileDocumentType } from "../files/types";
import filesStyles from "../files/component/Files/Files.styles";

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
    <View
      accessibilityLabel={`${fileName} uploading`}
      style={[
        filesStyles.fileRow,
        {
          backgroundColor: `${colorPalette.secondary}22`,
          borderColor: `${colorPalette.secondary}88`,
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
    </View>
  );
};

export default PendingFileUploadRow;
