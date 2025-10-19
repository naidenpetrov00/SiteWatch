import {
  CreateAccountInput,
  createAccountInputSchema,
} from "../../api/create-account";
import { Pressable, Text, TextInput, View } from "react-native";
import React, { useRef } from "react";
import { SubmitHandler, useForm } from "react-hook-form";

import FormField from "@/components/ui/FormField/FormField";
import signUpFormStyles from "./SignUpForm.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { zodResolver } from "@hookform/resolvers/zod";

interface ISignUpForm {}

const SignUpForm = ({}: ISignUpForm) => {
  const colorPalette = useColorPalette();

  const nameRef = useRef<TextInput>(null);
  const emailRef = useRef<TextInput>(null);
  const passRef = useRef<TextInput>(null);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateAccountInput>({
    resolver: zodResolver(createAccountInputSchema),
  });

  const onSignUp: SubmitHandler<CreateAccountInput> = (data) => {
    console.log(data);
  };

  return (
    <View style={signUpFormStyles.form}>
      <FormField
        ref={nameRef}
        control={control}
        validationError={errors.username}
        label="username"
        placeholder="Jiara Martins"
        returnKeyType="next"
        submitBehavior="submit"
        onSubmitEditing={() => emailRef.current?.focus()}
      />
      <FormField
        ref={emailRef}
        control={control}
        validationError={errors.email}
        label="email"
        placeholder="whataGreatApp@email.com"
        keyboardType="email-address"
        returnKeyType="next"
        submitBehavior="submit"
        onSubmitEditing={() => passRef.current?.focus()}
      />
      <FormField
        ref={passRef}
        control={control}
        validationError={errors.password}
        label="password"
        placeholder="******"
        secureTextEntry={true}
        returnKeyType="done"
        submitBehavior="blurAndSubmit"
      />
      <Pressable
        onPress={handleSubmit(onSignUp)}
        style={({ pressed }) => [
          signUpFormStyles.cta,
          {
            backgroundColor: colorPalette.primary,
          },
          pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
        ]}
      >
        <Text
          style={[signUpFormStyles.ctaText, { color: colorPalette.background }]}
        >
          Sign up
        </Text>
      </Pressable>
    </View>
  );
};

export default SignUpForm;
