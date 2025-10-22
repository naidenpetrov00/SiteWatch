import { Alert } from "react-native";
import { AxiosError } from "axios";
import { CreateAccountResponse } from "@/types/identity";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const verifyEmailInputSchema = z.object({
  code: z
    .string()
    .length(6, "It should be 6 characters long")
    .regex(/^\S+$/, "No spaces allowed"),
});

export type VerifyEmailInput = z.infer<typeof verifyEmailInputSchema>;

export const verifyEmail = async ({
  data,
}: {
  data: VerifyEmailInput;
}): Promise<CreateAccountResponse> => {
  return await api.post(paths.identity.verifyEmail, data);
};

type UseVerifyEmailOption = {
  mutationConfig?: MutationConfig<typeof verifyEmail>;
};

export const useVerifyEmail = ({ mutationConfig }: UseVerifyEmailOption) => {
  const config = mutationConfig || {};

  return useMutation({
    mutationFn: verifyEmail,
    onSuccess: () => {
      console.log("Email Verified");
      //   router.push({ pathname: "VerifyEmail", params: { email } });
    },
    onError: (error: AxiosError) => {
      const errors = Array.isArray(error.response?.data)
        ? error.response?.data.join("\n")
        : String(error.response?.data);
      Alert.alert("Verification Failed", errors);
    },
    ...config,
  });
};
