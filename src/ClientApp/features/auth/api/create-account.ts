import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { env } from "@/config/env";
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
}): Promise<void> => {
  await api.post("/identity/signUp", data);
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
    onSuccess: () => {
      console.log("Account created successfully!");
    },
    onError: (error) => {
      console.error("Failed to create account:", error);
    },
    ...config,
  });
};
