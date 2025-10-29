import { Controller, FieldError } from "react-hook-form";
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
  control?: any;
  name: string;
  validationError?: FieldError;
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
      control,
      name,
      validationError,
    },
    ref
  ) => {
    const colorPalette = useColorPalette();

    return (
      <Fragment>
        <Text style={[formFieldStyles.label, { color: colorPalette.text }]}>
          {label.toUpperCase()}
        </Text>

        <Controller
          name={name}
          control={control}
          render={({ field: { value, onBlur, onChange } }) => (
            <TextInput
              ref={ref}
              value={value}
              onBlur={onBlur}
              onChangeText={onChange}
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
          )}
        />
        {validationError && (
          <Text style={formFieldStyles.errorText}>
            {validationError.message ?? "Invalid value"}
          </Text>
        )}
      </Fragment>
    );
  }
);

export default FormField;
