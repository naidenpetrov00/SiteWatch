import axios, { AxiosError } from "axios";

import { Alert } from "react-native";
import { CreateAccountResponse } from "@/types/identity";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const createAccountInputSchema = z.object({
  username: z
    .string()
    .min(4, "Minimum 4")
    .max(20, "Maximum 20")
    .regex(/^\S+$/, "No spaces allowed"),
  email: z.string().email(),
  password: z
    .string()
    .min(6, "Minimum 6")
    .max(20, "Maximum 20")
    .regex(
      /^(?=.*[A-Z])(?=.*\d).+$/,
      "Must include at least one uppercase letter and one digit"
    )
    .regex(/^\S+$/, "No spaces allowed"),
});

export type CreateAccountInput = z.infer<typeof createAccountInputSchema>;

export const createAccount = async ({
  data,
}: {
  data: CreateAccountInput;
}): Promise<CreateAccountResponse> => {
  return await api.post(paths.identity.signUp, data);
};

type UseCreateAccountOption = {
  mutationConfig?: MutationConfig<typeof createAccount>;
};

export const useCreateAccount = ({
  mutationConfig,
}: UseCreateAccountOption) => {
  const config = mutationConfig || {};

  return useMutation({
    mutationFn: createAccount,
    onSuccess: (data) => {
      console.log(data);
    },
    onError: (error: AxiosError) => {
      const errors = Array.isArray(error.response?.data)
        ? error.response?.data.join("\n")
        : String(error.response?.data);
      Alert.alert("Sign up failed", errors);
    },
    ...config,
  });
};
