import { useMutation, useQueries, useQueryClient } from "@tanstack/react-query";

import { MutationConfig } from "@/lib/react-query";
import { api } from "@/lib/api-client";
import { z } from "zod";

export const createAccountInputSchema = z.object({
  username: z.string().min(6, "Minimum 6").max(10, "Maximum 10"),
  email: z.string().email(),
  password: z.string().min(6, "Required"),
});

export type CreateAccountInput = z.infer<typeof createAccountInputSchema>;

export const createAccount = ({
  data,
}: {
  data: CreateAccountInput;
}): Promise<Comment> => {
  return api.post("/comments", data);
};

type UseCreateAccountOption = {
  mutationConfig?: MutationConfig<typeof createAccount>;
};

export const useCreateAccount = ({
  mutationConfig,
}: UseCreateAccountOption) => {
  return useMutation({ mutationFn: createAccount });
};
