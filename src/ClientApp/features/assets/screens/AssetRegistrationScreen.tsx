import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "expo-router";
import { useRef, useState } from "react";
import {
  ActivityIndicator,
  Pressable,
  ScrollView,
  Text,
  TextInput,
  View,
} from "react-native";
import {
  Controller,
  type Control,
  type FieldPath,
  type SubmitHandler,
  useForm,
} from "react-hook-form";

import { getAssetApiError, useRegisterMobileAsset } from "../api";
import assetStyles from "../assets.styles";
import AssetSitePicker from "../components/AssetSitePicker";
import type { AssetClassification, MobileAssetSite } from "../types";
import {
  registerAssetSchema,
  type RegisterAssetFormValues,
  toRegisterMobileAssetRequest,
} from "../validation";
import { useColorPalette } from "@/hooks/useColorPalette";

const defaultValues: RegisterAssetFormValues = {
  classification: "Machine",
  name: "",
  manufacturer: "",
  model: "",
  description: "",
  manufacturerSerialNumber: "",
  siteId: "",
  assignToMe: false,
};

type FieldProps = {
  control: Control<RegisterAssetFormValues>;
  label: string;
  multiline?: boolean;
  name: FieldPath<RegisterAssetFormValues>;
  placeholder: string;
};

function AssetTextField({ control, label, multiline = false, name, placeholder }: FieldProps) {
  const colors = useColorPalette();

  return (
    <Controller
      control={control}
      name={name}
      render={({ field: { onBlur, onChange, value }, fieldState: { error } }) => (
        <View style={assetStyles.field}>
          <Text style={[assetStyles.label, { color: colors.text }]}>{label}</Text>
          <TextInput
            accessibilityLabel={label}
            autoCapitalize="sentences"
            multiline={multiline}
            onBlur={onBlur}
            onChangeText={onChange}
            placeholder={placeholder}
            placeholderTextColor={colors.placeholderText}
            style={[
              assetStyles.input,
              multiline ? assetStyles.multilineInput : null,
              { borderColor: error ? "#B42318" : colors.secondary, color: colors.text },
            ]}
            value={typeof value === "string" ? value : ""}
          />
          {error ? <Text style={assetStyles.errorText}>{error.message}</Text> : null}
        </View>
      )}
    />
  );
}

export default function AssetRegistrationScreen() {
  const colors = useColorPalette();
  const router = useRouter();
  const registration = useRegisterMobileAsset();
  const submissionLockRef = useRef(false);
  const [sitePickerVisible, setSitePickerVisible] = useState(false);
  const [selectedSite, setSelectedSite] = useState<MobileAssetSite | null>(null);
  const {
    control,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<RegisterAssetFormValues>({
    defaultValues,
    resolver: zodResolver(registerAssetSchema),
  });
  const classification = watch("classification");
  const assignToMe = watch("assignToMe");

  const chooseClassification = (value: AssetClassification) => {
    setValue("classification", value, { shouldDirty: true, shouldValidate: true });
    registration.reset();
  };

  const chooseSite = (site: MobileAssetSite) => {
    setSelectedSite(site);
    setValue("siteId", site.id, { shouldDirty: true, shouldValidate: true });
    setSitePickerVisible(false);
    registration.reset();
  };

  const submit: SubmitHandler<RegisterAssetFormValues> = async (values) => {
    if (submissionLockRef.current) return;
    submissionLockRef.current = true;
    registration.reset();
    try {
      const result = await registration.mutateAsync(toRegisterMobileAssetRequest(values));
      router.replace({ pathname: "/Assets/registrations/[id]", params: { id: result.id } });
    } catch {
      // The mutation retains the server or network error for the inline error state.
    } finally {
      submissionLockRef.current = false;
    }
  };

  return (
    <>
      <ScrollView
        contentContainerStyle={assetStyles.content}
        contentInsetAdjustmentBehavior="automatic"
        keyboardShouldPersistTaps="handled"
        style={[assetStyles.screen, { backgroundColor: colors.background }]}
      >
        <View style={{ gap: 6 }}>
          <Text style={[assetStyles.title, { color: colors.text }]}>Register one asset</Text>
          <Text style={[assetStyles.subtitle, { color: colors.secondary }]}>
            The registration will remain pending until an administrator reviews it. New registrations start in Good condition and Available status.
          </Text>
        </View>

        <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
          <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Classification</Text>
          <View style={assetStyles.choiceRow}>
            {(["Machine", "Tool"] as const).map((value) => {
              const selected = classification === value;
              return (
                <Pressable
                  accessibilityRole="radio"
                  accessibilityState={{ selected }}
                  key={value}
                  onPress={() => chooseClassification(value)}
                  style={({ pressed }) => [
                    assetStyles.choice,
                    {
                      backgroundColor: selected ? colors.primary : "transparent",
                      borderColor: selected ? colors.primary : colors.secondary,
                      opacity: pressed ? 0.78 : 1,
                    },
                  ]}
                >
                  <Text style={[assetStyles.choiceText, { color: selected ? colors.background : colors.text }]}>{value}</Text>
                </Pressable>
              );
            })}
          </View>
        </View>

        <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
          <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Asset details</Text>
          <AssetTextField control={control} label="Name (required)" name="name" placeholder="e.g. Cordless drill" />
          <AssetTextField control={control} label="Manufacturer" name="manufacturer" placeholder="Optional" />
          <AssetTextField control={control} label="Model" name="model" placeholder="Optional" />
          <AssetTextField control={control} label="Manufacturer serial number" name="manufacturerSerialNumber" placeholder="Optional" />
          <AssetTextField control={control} label="Description" multiline name="description" placeholder="Optional details" />
        </View>

        <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
          <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Site (required)</Text>
          <Pressable
            accessibilityRole="button"
            onPress={() => setSitePickerVisible(true)}
            style={({ pressed }) => [
              assetStyles.input,
              {
                borderColor: errors.siteId ? "#B42318" : colors.secondary,
                justifyContent: "center",
                opacity: pressed ? 0.78 : 1,
              },
            ]}
          >
            <Text style={{ color: selectedSite ? colors.text : colors.placeholderText }}>
              {selectedSite ? `${selectedSite.name} · ${selectedSite.address}` : "Choose an accessible Site"}
            </Text>
          </Pressable>
          {errors.siteId ? <Text style={assetStyles.errorText}>{errors.siteId.message}</Text> : null}
        </View>

        <View style={[assetStyles.section, { borderColor: colors.secondary }]}>
          <Text style={[assetStyles.sectionTitle, { color: colors.text }]}>Responsibility</Text>
          <View style={assetStyles.choiceRow}>
            {[
              { label: "Leave unassigned", value: false },
              { label: "Assign to me", value: true },
            ].map((choice) => {
              const selected = assignToMe === choice.value;
              return (
                <Pressable
                  accessibilityRole="radio"
                  accessibilityState={{ selected }}
                  key={choice.label}
                  onPress={() => {
                    setValue("assignToMe", choice.value, { shouldDirty: true, shouldValidate: true });
                    registration.reset();
                  }}
                  style={({ pressed }) => [
                    assetStyles.choice,
                    {
                      backgroundColor: selected ? colors.primary : "transparent",
                      borderColor: selected ? colors.primary : colors.secondary,
                      opacity: pressed ? 0.78 : 1,
                    },
                  ]}
                >
                  <Text style={[assetStyles.choiceText, { color: selected ? colors.background : colors.text }]}>{choice.label}</Text>
                </Pressable>
              );
            })}
          </View>
        </View>

        {registration.isError ? (
          <View style={assetStyles.errorBox}>
            <Text accessibilityRole="alert" style={assetStyles.errorText}>
              {getAssetApiError(registration.error, "The registration could not be submitted.")}
            </Text>
          </View>
        ) : null}

        <View style={assetStyles.buttonRow}>
          <Pressable
            accessibilityRole="button"
            accessibilityState={{ busy: registration.isPending }}
            disabled={registration.isPending}
            onPress={handleSubmit(submit)}
            style={({ pressed }) => [
              assetStyles.button,
              {
                backgroundColor: colors.primary,
                opacity: registration.isPending ? 0.5 : pressed ? 0.78 : 1,
              },
            ]}
          >
            {registration.isPending ? (
              <ActivityIndicator color={colors.background} />
            ) : (
              <Text style={[assetStyles.buttonText, { color: colors.background }]}>Submit for review</Text>
            )}
          </Pressable>
          <Pressable
            accessibilityRole="button"
            disabled={registration.isPending}
            onPress={() => router.back()}
            style={({ pressed }) => [
              assetStyles.button,
              {
                borderColor: colors.secondary,
                borderWidth: 1,
                opacity: registration.isPending ? 0.5 : pressed ? 0.78 : 1,
              },
            ]}
          >
            <Text style={[assetStyles.buttonText, { color: colors.text }]}>Cancel</Text>
          </Pressable>
        </View>
      </ScrollView>
      <AssetSitePicker
        onClose={() => setSitePickerVisible(false)}
        onSelect={chooseSite}
        visible={sitePickerVisible}
      />
    </>
  );
}
