import type { UserRole } from "@/types/authorization";

export type DetailsCardItem = {
  label: string;
  value: string;
  helper: string;
  path?: string;
  allowedRoles?: readonly UserRole[];
};
