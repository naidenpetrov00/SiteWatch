import { Modal } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useColorPalette } from "@/hooks/useColorPalette";
import addIssueModalStyles from "./AddIssueModal.styles";
import AddIssueForm from "./AddIssueForm";

type AddIssueModalProps = {
  siteId: string;
  visible: boolean;
  onClose: () => void;
};

const AddIssueModal = ({ siteId, visible, onClose }: AddIssueModalProps) => {
  const colorPalette = useColorPalette();

  return (
    <Modal
      animationType="slide"
      onRequestClose={onClose}
      presentationStyle="pageSheet"
      visible={visible}
    >
      <SafeAreaView
        edges={["top", "bottom"]}
        style={[addIssueModalStyles.container, { backgroundColor: colorPalette.background }]}
      >
        <AddIssueForm siteId={siteId} visible={visible} onClose={onClose} />
      </SafeAreaView>
    </Modal>
  );
};

export default AddIssueModal;
