import { CreateAccountResponse, SignInResponse } from "@/types/identity";
import axios, { AxiosError } from "axios";

import { Alert } from "react-native";
import { IdentityResultWithUserToken } from "@/types/api";
import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { env } from "@/config/env";
import { paths } from "@/config/constants/paths";
import { router } from "expo-router";
import { useAuth } from "@/store/auth_context";
import { useMutation } from "@tanstack/react-query";
import { z } from "zod";

export const signInInputSchema = z.object({
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

export type SignInInput = z.infer<typeof signInInputSchema>;

export const createAccount = async ({
  data,
}: {
  data: SignInInput;
}): Promise<IdentityResultWithUserToken> => {
  return await api.post(paths.identity.signIn, data);
};

type UseSignInOption = {
  mutationConfig?: MutationConfig<typeof createAccount>;
};

export const useSignIn = ({ mutationConfig }: UseSignInOption = {}) => {
  const { login } = useAuth();
  return useMutation({
    mutationFn: createAccount,
    onSuccess: ({ user, token }) => {
      login(user, token);
      router.push("/Home");
    },
    onError: (error: AxiosError) => {
      const errors = Array.isArray(error.response?.data)
        ? error.response?.data.join("\n")
        : String(error.response?.data);
      Alert.alert("Sign in failed", errors);
    },
    ...mutationConfig,
  });
};
