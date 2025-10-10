import { Fragment, useEffect } from "react";
import { KeyboardTypeOptions, Text, TextInput, View } from "react-native";

import formFieldStyles from "./FormField.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IFormField {
  label: string;
  placeholder: string;
  keyboardType?: KeyboardTypeOptions;
  secureTextEntry?: boolean;
}

const FormField = ({
  label,
  placeholder,
  keyboardType,
  secureTextEntry,
}: IFormField) => {
  const colorPalette = useColorPalette();

  return (
    <Fragment>
      <Text style={[formFieldStyles.label, { color: colorPalette.text }]}>
        {label.toUpperCase()}
      </Text>

      <TextInput
        placeholder={placeholder}
        placeholderTextColor={colorPalette.placeholderText}
        secureTextEntry={secureTextEntry}
        keyboardType={keyboardType}
        style={[
          formFieldStyles.input,
          { backgroundColor: colorPalette.secondary, color: colorPalette.text },
        ]}
      />
    </Fragment>
  );
};

export default FormField;
