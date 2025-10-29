import { Pressable, Text, View } from "react-native";
import React, { useEffect, useState } from "react";
import { SubmitHandler, useForm } from "react-hook-form";
import {
  VerifyEmailInput,
  useVerifyEmail,
  verifyEmailInputSchema,
} from "../../api/verify-email";

import FormField from "@/components/ui/FormField/FormField";
import signUpFormStyles from "../SignUpForm/SignUpForm.styles";
import { useColorPalette } from "@/hooks/useColorPalette";
import { useResendEmail } from "../../api/resend-email";
import { zodResolver } from "@hookform/resolvers/zod";

interface VerifyEmailForm {
  email: string;
}

const RESEND_COOLDOWN = 20;

const VerifyEmailForm = ({ email }: VerifyEmailForm) => {
  const colorPalette = useColorPalette();

  const {
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<VerifyEmailInput>({
    resolver: zodResolver(verifyEmailInputSchema),
  });

  const [secondsLeft, setSecondsLeft] = useState<number>(RESEND_COOLDOWN);

  useEffect(() => {
    if (secondsLeft <= 0) return;
    const id = setInterval(() => {
      setSecondsLeft((s) => (s <= 1 ? 0 : s - 1));
    }, 1000);
    return () => clearInterval(id);
  }, [secondsLeft]);

  const { mutate: verifyMutation, isPending } = useVerifyEmail({
    mutationConfig: {},
  });
  const onVerifyEmail: SubmitHandler<VerifyEmailInput> = (data) => {
    // data.email = email;
    console.log(data);

    verifyMutation({ data });
  };

  const { mutate: resendMutation, isPending: resendIsPending } = useResendEmail(
    {
      mutationConfig: {
        onSuccess: () => {
          setSecondsLeft(RESEND_COOLDOWN);
        },
      },
    }
  );
  const onResendEmail = () => {
    if (secondsLeft > 0) return;
    resendMutation({ data: { email: email } });
  };

  return (
    <View style={signUpFormStyles.form}>
      <FormField
        name="token"
        control={control}
        validationError={errors.token}
        label="6 digit token"
        placeholder="XXXXXX"
        returnKeyType="done"
        submitBehavior="blurAndSubmit"
        keyboardType="number-pad"
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
          On the way. You can resend in {secondsLeft}s
        </Text>
      ) : (
        <Pressable
          onPress={onResendEmail}
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
            {resendIsPending ? "Resending..." : "Resend code"}
          </Text>
        </Pressable>
      )}
    </View>
  );
};

export default VerifyEmailForm;
