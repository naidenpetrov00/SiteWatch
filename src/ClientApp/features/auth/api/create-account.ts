import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
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
    .min(6, "Required")
    .max(20, "Maximum 20")
    .regex(
      /^(?=.*[A-Z])(?=.*\d).+$/,
      "Must include at least one uppercase letter and one digit"
    )
    .regex(/^\S+$/, "No spaces allowed"),
});

export type CreateAccountInput = z.infer<typeof createAccountInputSchema>;

export const createAccount = ({
  data,
}: {
  data: CreateAccountInput;
}): Promise<Comment> => {
  return api.post("/identity/signUp", data);
};

type UseCreateAccountOption = {
  mutationConfig?: MutationConfig<typeof createAccount>;
};

export const useCreateAccount = ({
  mutationConfig,
}: UseCreateAccountOption) => {
  const config = mutationConfig || {};

  return useMutation({ mutationFn: createAccount, ...config });
};
