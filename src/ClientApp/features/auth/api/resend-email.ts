import { Alert } from "react-native";
import { AxiosError } from "axios";
import { CreateAccountResponse } from "@/types/identity";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { paths } from "@/config/constants/paths";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const resendEmailInputSchema = z.object({
  email: z.string().email(),
});

export type ResendEmailInput = z.infer<typeof resendEmailInputSchema>;

export const resendEmail = async ({
  data,
}: {
  data: ResendEmailInput;
}): Promise<void> => {
  console.log(data);
  api.post(paths.identity.resendEmail, data);
};

type UseResendEmailOption = {
  mutationConfig?: MutationConfig<typeof resendEmail>;
};

export const useResendEmail = ({ mutationConfig }: UseResendEmailOption) => {
  const config = mutationConfig || {};

  return useMutation({
    mutationFn: resendEmail,
    onError: (error: AxiosError) => {
      const errors = Array.isArray(error.response?.data)
        ? error.response?.data.join("\n")
        : String(error.response?.data);
      Alert.alert("Verification Failed", errors);
    },
    ...config,
  });
};
