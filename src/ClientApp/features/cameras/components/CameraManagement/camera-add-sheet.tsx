import { useEffect, useState } from "react";
import {
  Alert,
  Modal,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";

import { CameraProtocol, CameraUpsertRequest } from "../../api/models";
import { useCreateCamera } from "../../api/manage-cameras";
import { useColorPalette } from "@/hooks/useColorPalette";
import cameraManagementStyles from "./camera-management.styles";

type CameraAddSheetProps = {
  siteId: string;
  visible: boolean;
  onClose: () => void;
};

type FormValues = {
  name: string;
  brand: string;
  model: string;
  username: string;
  password: string;
  ipAddress: string;
  rtspPort: string;
  ptzPort: string;
  protocol: CameraProtocol;
};

const initialValues: FormValues = {
  name: "",
  brand: "Dahua",
  model: "Dahua",
  username: "",
  password: "",
  ipAddress: "",
  rtspPort: "554",
  ptzPort: "443",
  protocol: "Http",
};

const errorMessage = (error: unknown, fallback: string) =>
  error instanceof Error && error.message.trim() ? error.message : fallback;

const CameraAddSheet = ({ siteId, visible, onClose }: CameraAddSheetProps) => {
  const colorPalette = useColorPalette();
  const createCamera = useCreateCamera(siteId);
  const [values, setValues] = useState<FormValues>(initialValues);
  const [validationError, setValidationError] = useState<string | null>(null);

  useEffect(() => {
    if (!visible) return;
    setValues(initialValues);
    setValidationError(null);
  }, [visible]);

  const update = (field: keyof FormValues, value: string) => {
    setValues((current) => ({ ...current, [field]: value }));
    setValidationError(null);
  };

  const submit = async () => {
    const name = values.name.trim();
    const brand = values.brand.trim();
    const model = values.model.trim();
    const rtspPort = Number(values.rtspPort);
    const ptzPort = Number(values.ptzPort);

    if (!name || name.length > 100 || !brand || !model || model.length < 2 || model.length > 100) {
      setValidationError("Enter a camera name, brand, and a model between 2 and 100 characters.");
      return;
    }
    if (!values.username.trim() || !values.password.trim() || !values.ipAddress.trim()) {
      setValidationError("Username, password, and IP address are required.");
      return;
    }
    if (values.username.trim().length > 50 || values.password.trim().length > 50 ||
      values.ipAddress.trim().length > 39) {
      setValidationError("Username and password can be up to 50 characters; IP address can be up to 39.");
      return;
    }
    if (!Number.isInteger(rtspPort) || rtspPort < 1 || rtspPort > 65535 ||
      !Number.isInteger(ptzPort) || ptzPort < 1 || ptzPort > 65535) {
      setValidationError("Enter RTSP and PTZ ports from 1 to 65535.");
      return;
    }

    const request: CameraUpsertRequest = {
      name,
      brand,
      model,
      username: values.username.trim(),
      password: values.password.trim(),
      ipAddress: values.ipAddress.trim(),
      rtspPort,
      ptzPort,
      protocol: values.protocol,
      siteId,
    };

    try {
      await createCamera.mutateAsync(request);
      onClose();
    } catch (error) {
      Alert.alert("Unable to add camera", errorMessage(error, "Please check the camera details and try again."));
    }
  };

  return (
    <Modal
      animationType="slide"
      onRequestClose={onClose}
      presentationStyle="formSheet"
      visible={visible}
    >
      <ScrollView
        contentContainerStyle={cameraManagementStyles.content}
        contentInsetAdjustmentBehavior="automatic"
        style={{ backgroundColor: colorPalette.background }}
      >
        <Text selectable style={{ color: colorPalette.text, fontSize: 22, fontWeight: "700" }}>
          Add Camera
        </Text>
        <Text selectable style={{ color: colorPalette.secondary }}>
          This camera will be added to the current site.
        </Text>
        <Field label="Camera name" value={values.name} onChangeText={(value) => update("name", value)} color={colorPalette.text} />
        <Field label="Brand" value={values.brand} onChangeText={(value) => update("brand", value)} color={colorPalette.text} />
        <Field label="Model" value={values.model} onChangeText={(value) => update("model", value)} color={colorPalette.text} />
        <Field label="Username" value={values.username} onChangeText={(value) => update("username", value)} color={colorPalette.text} autoCapitalize="none" />
        <Field label="Password" value={values.password} onChangeText={(value) => update("password", value)} color={colorPalette.text} secureTextEntry autoCapitalize="none" />
        <Field label="IP address" value={values.ipAddress} onChangeText={(value) => update("ipAddress", value)} color={colorPalette.text} autoCapitalize="none" />
        <Field label="RTSP port" value={values.rtspPort} onChangeText={(value) => update("rtspPort", value)} color={colorPalette.text} keyboardType="number-pad" />
        <Field label="PTZ port" value={values.ptzPort} onChangeText={(value) => update("ptzPort", value)} color={colorPalette.text} keyboardType="number-pad" />
        <View style={cameraManagementStyles.field}>
          <Text selectable style={{ color: colorPalette.text }}>Protocol</Text>
          <View style={{ flexDirection: "row", gap: 10 }}>
            {(["Https", "Http"] as const).map((protocol) => (
              <Pressable
                key={protocol}
                accessibilityRole="button"
                accessibilityState={{ selected: values.protocol === protocol }}
                onPress={() => update("protocol", protocol)}
                style={({ pressed }) => [
                  cameraManagementStyles.button,
                  {
                    backgroundColor: values.protocol === protocol ? colorPalette.primary : colorPalette.background,
                    borderColor: colorPalette.primary,
                    borderWidth: values.protocol === protocol ? 0 : 1,
                    opacity: pressed ? 0.78 : 1,
                  },
                ]}
              >
                <Text style={{ color: values.protocol === protocol ? colorPalette.background : colorPalette.text }}>
                  {protocol.toUpperCase()}
                </Text>
              </Pressable>
            ))}
          </View>
        </View>
        {validationError ? <Text selectable style={[cameraManagementStyles.error, { color: "#B42318" }]}>{validationError}</Text> : null}
      </ScrollView>
      <View style={[cameraManagementStyles.footer, { backgroundColor: colorPalette.background }]}>
        <Pressable onPress={onClose} style={({ pressed }) => [cameraManagementStyles.button, { backgroundColor: colorPalette.secondary, opacity: pressed ? 0.78 : 1 }]}>
          <Text style={{ color: colorPalette.background, fontWeight: "600" }}>Cancel</Text>
        </Pressable>
        <Pressable accessibilityState={{ busy: createCamera.isPending }} disabled={createCamera.isPending} onPress={() => void submit()} style={({ pressed }) => [cameraManagementStyles.button, { backgroundColor: colorPalette.primary, opacity: createCamera.isPending ? 0.55 : pressed ? 0.78 : 1 }]}>
          <Text style={{ color: colorPalette.background, fontWeight: "600" }}>{createCamera.isPending ? "Adding…" : "Add Camera"}</Text>
        </Pressable>
      </View>
    </Modal>
  );
};

type FieldProps = {
  label: string;
  value: string;
  color: string;
  editable?: boolean;
  secureTextEntry?: boolean;
  autoCapitalize?: "none";
  keyboardType?: "number-pad";
  onChangeText?: (value: string) => void;
};

const Field = ({ label, value, color, editable = true, onChangeText, ...inputProps }: FieldProps) => (
  <View style={cameraManagementStyles.field}>
    <Text selectable style={{ color }}>{label}</Text>
    <TextInput
      {...inputProps}
      editable={editable}
      onChangeText={onChangeText}
      placeholderTextColor={color}
      style={[cameraManagementStyles.input, { borderColor: color, color, opacity: editable ? 1 : 0.65 }]}
      value={value}
    />
  </View>
);

export default CameraAddSheet;
