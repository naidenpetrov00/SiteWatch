import { Pressable, Text, TextInput, View } from "react-native";
import React, { useRef } from "react";
import {
  SignInInput,
  signInInputSchema,
  useSignIn,
} from "../../api/sign-in-accounts";
import { SubmitHandler, useForm } from "react-hook-form";

import FormField from "@/components/ui/FormField/FormField";
import signUpFormStyles from "../SignUpForm/SignUpForm.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { zodResolver } from "@hookform/resolvers/zod";

interface ISignInFormForm {}

const defaultValues: SignInInput = {
  email: "naiden.petrov.31.12.00@gmail.com",
  password: "Test@123",
};
const SignInForm = ({}: ISignInFormForm) => {
  const colorPalette = useColorPalette();

  const emailRef = useRef<TextInput>(null);
  const passRef = useRef<TextInput>(null);

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<SignInInput>({
    defaultValues,
    resolver: zodResolver(signInInputSchema),
  });

  const { mutate, isPending } = useSignIn();
  const onSignIn: SubmitHandler<SignInInput> = (data) => {
    mutate({ data });
  };

  return (
    <View style={signUpFormStyles.form}>
      <FormField
        ref={emailRef}
        control={control}
        name="email"
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
        name="password"
        validationError={errors.password}
        label="password"
        placeholder="******"
        secureTextEntry={true}
        returnKeyType="done"
        submitBehavior="blurAndSubmit"
      />
      <Pressable
        onPress={handleSubmit(onSignIn)}
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
          {isPending ? "Signing In..." : "Sign In"}
        </Text>
      </Pressable>
    </View>
  );
};

export default SignInForm;
