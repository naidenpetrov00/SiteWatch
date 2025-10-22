import {
  CreateAccountInput,
  createAccountInputSchema,
  useCreateAccount,
} from "../../api/create-account";
import { Pressable, Text, TextInput, View } from "react-native";
import React, { useEffect, useRef, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import {
  VerifyEmailInput,
  useVerifyEmail,
  verifyEmailInputSchema,
} from "../../api/verify-email";

import FormField from "@/components/ui/FormField/FormField";
import signUpFormStyles from "../SignUpForm/SignUpForm.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useRouter } from "expo-router";
import { zodResolver } from "@hookform/resolvers/zod";

interface VerifyEmailForm {}

const defaultValues: VerifyEmailInput = {
  code: "123456",
};

const RESEND_COOLDOWN = 15;

const VerifyEmailForm = ({}: VerifyEmailForm) => {
  const colorPalette = useColorPalette();
  const router = useRouter();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<VerifyEmailInput>({
    defaultValues,
    resolver: zodResolver(verifyEmailInputSchema),
  });

  const [secondsLeft, setSecondsLeft] = useState<number>(RESEND_COOLDOWN);
  const [isResending, setIsResending] = useState(false);

  useEffect(() => {
    if (secondsLeft <= 0) return;
    const id = setInterval(() => setSecondsLeft((s) => s - 1), 1000);
    return () => clearInterval(id);
  }, []);

  const { mutate, isPending } = useVerifyEmail({ mutationConfig: {} });
  const onVerifyEmail: SubmitHandler<VerifyEmailInput> = (data) => {
    mutate({ data });
  };

  const handleResend = async () => {
    if (secondsLeft > 0 || isResending) return;
    try {
      setIsResending(true);
      // await api.post(paths.identity.resendCode, { email: ... });
      // On success, restart cooldown:
      setSecondsLeft(RESEND_COOLDOWN);
    } catch (e) {
      // Alert.alert("Resend failed", String(e));
    } finally {
      setIsResending(false);
    }
  };

  return (
    <View style={signUpFormStyles.form}>
      <FormField
        control={control}
        validationError={errors.code}
        label="6 digit code"
        placeholder="XXX XXX"
        returnKeyType="done"
        submitBehavior="blurAndSubmit"
      />

      <Pressable
        onPress={handleSubmit(onVerifyEmail)}
        style={({ pressed }) => [
          signUpFormStyles.cta,
          { backgroundColor: colorPalette.primary },
          isPending
            ? { opacity: 0.5 }
            : pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
        ]}
        disabled={isPending}
      >
        <Text
          style={[signUpFormStyles.ctaText, { color: colorPalette.background }]}
        >
          {isPending ? "Verifying..." : "Verify"}
        </Text>
      </Pressable>

      {/* --- timer / resend row --- */}
      {secondsLeft > 0 ? (
        <Text
          style={{
            marginTop: 10,
            textAlign: "center",
            opacity: 0.7,
          }}
        >
          You can resend in {secondsLeft}s
        </Text>
      ) : (
        <Pressable
          onPress={handleResend}
          disabled={isResending}
          style={({ pressed }) => [
            signUpFormStyles.cta,
            { backgroundColor: colorPalette.primary },
            isPending
              ? { opacity: 0.5 }
              : pressed && { opacity: 0.9, transform: [{ scale: 0.995 }] },
          ]}
        >
          <Text
            style={[signUpFormStyles.ctaText, { color: colorPalette.text }]}
          >
            {isResending ? "Resending..." : "Resend code"}
          </Text>
        </Pressable>
      )}
    </View>
  );
};

export default VerifyEmailForm;
