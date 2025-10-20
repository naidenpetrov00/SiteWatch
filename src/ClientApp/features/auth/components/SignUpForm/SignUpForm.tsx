import {
  CreateAccountInput,
  createAccountInputSchema,
  useCreateAccount,
} from "../../api/create-account";
import { Pressable, Text, TextInput, View } from "react-native";
import React, { useRef } from "react";
import { SubmitHandler, useForm } from "react-hook-form";

import FormField from "@/components/ui/FormField/FormField";
import { create } from "react-test-renderer";
import signUpFormStyles from "./SignUpForm.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { zodResolver } from "@hookform/resolvers/zod";

interface ISignUpForm {}

const defaultValues: CreateAccountInput = {
  email: "naiden.petrov.31.12.00@gmail.com",
  username: "Test.2010",
  password: "Test.2010",
};
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
    defaultValues,
    resolver: zodResolver(createAccountInputSchema),
  });

  const { mutate, isPending } = useCreateAccount({ mutationConfig: {} });
  const onSignUp: SubmitHandler<CreateAccountInput> = (data) => {
    mutate({ data });
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
          isPending
            ? { opacity: 0.5 }
            : pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
        ]}
        disabled={isPending}
      >
        <Text
          style={[signUpFormStyles.ctaText, { color: colorPalette.background }]}
        >
          {isPending ? "Creating..." : "Sign up"}
        </Text>
      </Pressable>
    </View>
  );
};

export default SignUpForm;
