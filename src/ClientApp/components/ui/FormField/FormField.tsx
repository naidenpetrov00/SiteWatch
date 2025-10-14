import { Fragment, useEffect } from "react";
import {
  KeyboardTypeOptions,
  ReturnKeyType,
  ReturnKeyTypeOptions,
  SubmitBehavior,
  Text,
  TextInput,
  TextInputProps,
  TextInputSubmitEditingEvent,
  View,
} from "react-native";

import React from "react";
import formFieldStyles from "./FormField.styles";
import { useColorPalette } from "@/hooks/useColorPalette";

interface IFormField {
  label: string;
  placeholder: string;
  keyboardType?: KeyboardTypeOptions;
  secureTextEntry?: boolean;
  returnKeyType?: ReturnKeyTypeOptions;
  submitBehavior?: SubmitBehavior;
  onSubmitEditing?: (e: TextInputSubmitEditingEvent) => void;
}

const FormField = React.forwardRef<TextInput, IFormField>(
  (
    {
      label,
      placeholder,
      keyboardType,
      secureTextEntry,
      returnKeyType,
      submitBehavior,
      onSubmitEditing,
    },
    ref
  ) => {
    const colorPalette = useColorPalette();

    return (
      <Fragment>
        <Text style={[formFieldStyles.label, { color: colorPalette.text }]}>
          {label.toUpperCase()}
        </Text>

        <TextInput
          ref={ref}
          placeholder={placeholder}
          placeholderTextColor={colorPalette.placeholderText}
          secureTextEntry={secureTextEntry}
          keyboardType={keyboardType}
          returnKeyType={returnKeyType}
          submitBehavior={submitBehavior}
          onSubmitEditing={onSubmitEditing}
          style={[
            formFieldStyles.input,
            {
              backgroundColor: colorPalette.secondary,
              color: colorPalette.text,
            },
          ]}
        />
      </Fragment>
    );
  }
);

export default FormField;
