import { useEffect, useState } from "react";
import { Modal, Pressable, ScrollView, Text, TextInput, View } from "react-native";
import { SafeAreaView } from "react-native-safe-area-context";

import { useColorPalette } from "@/hooks/useColorPalette";
import { useCreateIssue } from "../hooks/useCreateIssue";
import addIssueModalStyles from "./AddIssueModal.styles";

type AddIssueModalProps = {
  siteId: string;
  visible: boolean;
  onClose: () => void;
};

const getErrorMessage = (error: unknown) =>
  error instanceof Error && error.message.trim()
    ? error.message
    : "Unable to add the issue. Please try again.";

const AddIssueModal = ({ siteId, visible, onClose }: AddIssueModalProps) => {
  const colorPalette = useColorPalette();
  const createIssue = useCreateIssue();
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!visible) return;
    setTitle("");
    setDescription("");
    setError(null);
  }, [visible]);

  const submit = async () => {
    const trimmedTitle = title.trim();
    const trimmedDescription = description.trim();

    if (!trimmedTitle || trimmedTitle.length > 200) {
      setError("Enter an issue title of up to 200 characters.");
      return;
    }
    if (!trimmedDescription || trimmedDescription.length > 4000) {
      setError("Enter an issue description of up to 4,000 characters.");
      return;
    }

    try {
      await createIssue.mutateAsync({
        siteId,
        title: trimmedTitle,
        description: trimmedDescription,
      });
      onClose();
    } catch (submissionError) {
      setError(getErrorMessage(submissionError));
    }
  };

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
        <ScrollView
          contentContainerStyle={addIssueModalStyles.content}
          contentInsetAdjustmentBehavior="automatic"
        >
          <Text style={[addIssueModalStyles.title, { color: colorPalette.text }]}>Add Issue</Text>
          <Text style={[addIssueModalStyles.subtitle, { color: colorPalette.secondary }]}>
            Describe a problem for this site. New issues are created with an Open status.
          </Text>
          <View style={addIssueModalStyles.field}>
            <Text style={[addIssueModalStyles.label, { color: colorPalette.text }]}>Title</Text>
            <TextInput
              accessibilityLabel="Issue title"
              autoFocus
              maxLength={200}
              onChangeText={(value) => {
                setTitle(value);
                setError(null);
              }}
              placeholder="Describe the issue"
              placeholderTextColor={colorPalette.secondary}
              style={[addIssueModalStyles.input, { borderColor: colorPalette.secondary, color: colorPalette.text }]}
              value={title}
            />
          </View>
          <View style={addIssueModalStyles.field}>
            <Text style={[addIssueModalStyles.label, { color: colorPalette.text }]}>Description</Text>
            <TextInput
              accessibilityLabel="Issue description"
              maxLength={4000}
              multiline
              onChangeText={(value) => {
                setDescription(value);
                setError(null);
              }}
              placeholder="Add the relevant details"
              placeholderTextColor={colorPalette.secondary}
              style={[
                addIssueModalStyles.input,
                addIssueModalStyles.descriptionInput,
                { borderColor: colorPalette.secondary, color: colorPalette.text },
              ]}
              value={description}
            />
          </View>
          {error ? (
            <Text
              accessibilityRole="alert"
              style={[addIssueModalStyles.error, { borderColor: "#B42318", color: "#B42318" }]}
            >
              {error}
            </Text>
          ) : null}
        </ScrollView>
        <View style={[addIssueModalStyles.footer, { backgroundColor: colorPalette.background }]}>
          <Pressable
            accessibilityRole="button"
            disabled={createIssue.isPending}
            onPress={onClose}
            style={({ pressed }) => [
              addIssueModalStyles.button,
              { backgroundColor: colorPalette.secondary, opacity: createIssue.isPending ? 0.55 : pressed ? 0.78 : 1 },
            ]}
          >
            <Text style={[addIssueModalStyles.buttonText, { color: colorPalette.background }]}>Cancel</Text>
          </Pressable>
          <Pressable
            accessibilityRole="button"
            accessibilityState={{ busy: createIssue.isPending }}
            disabled={createIssue.isPending}
            onPress={() => void submit()}
            style={({ pressed }) => [
              addIssueModalStyles.button,
              { backgroundColor: colorPalette.primary, opacity: createIssue.isPending ? 0.55 : pressed ? 0.78 : 1 },
            ]}
          >
            <Text style={[addIssueModalStyles.buttonText, { color: colorPalette.background }]}>
              {createIssue.isPending ? "Adding…" : "Add Issue"}
            </Text>
          </Pressable>
        </View>
      </SafeAreaView>
    </Modal>
  );
};

export default AddIssueModal;
