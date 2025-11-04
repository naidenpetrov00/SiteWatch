import { Alert } from "react-native";
import { AxiosError } from "axios";
import { IdentityResultWithUserToken } from "@/types/api";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const verifyEmailSchema = z.object({
  token: z.string().regex(/^\d{6}$/, "Enter exactly 6 digits"),
});

export type VerifyEmailInput = z.infer<typeof verifyEmailSchema>;

export const emailSchema = verifyEmailSchema.extend({
  email: z.string().email(),
});
export type EmailSchema = z.infer<typeof emailSchema>;

export const verifyEmail = async ({
  data,
}: {
  data: EmailSchema;
}): Promise<IdentityResultWithUserToken> =>
  await api.post(paths.identity.verifyEmail, data);

type UseVerifyEmailOption = {
  mutationConfig?: MutationConfig<typeof verifyEmail>;
};

export const useVerifyEmail = ({ mutationConfig }: UseVerifyEmailOption) => {
  const config = mutationConfig || {};

  return useMutation({
    mutationFn: verifyEmail,
    onError: (error: AxiosError) => {
      const errors = Array.isArray(error.response?.data)
        ? error.response?.data.join("\n")
        : String(error.response?.data);
      Alert.alert("Verification Failed", errors);
    },
    ...config,
  });
};
