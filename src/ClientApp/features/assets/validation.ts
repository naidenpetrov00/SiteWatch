import { z } from "zod";

import type { RegisterMobileAssetRequest } from "./types";

export const collapseWhitespace = (value: string) =>
  value.trim().replace(/\s+/gu, " ");

const optionalCollapsed = (maximum: number, label: string) =>
  z.string().refine(
    (value) => !value.trim() || collapseWhitespace(value).length <= maximum,
    `${label} cannot exceed ${maximum} characters.`,
  );

const optionalTrimmed = (maximum: number, label: string) =>
  z.string().refine(
    (value) => !value.trim() || value.trim().length <= maximum,
    `${label} cannot exceed ${maximum.toLocaleString()} characters.`,
  );

export const registerAssetSchema = z.object({
  classification: z.enum(["Machine", "Tool"]),
  name: z.string().refine(
    (value) => {
      const normalized = collapseWhitespace(value);
      return normalized.length > 0 && normalized.length <= 200;
    },
    "Name is required and cannot exceed 200 characters.",
  ),
  manufacturer: optionalCollapsed(100, "Manufacturer"),
  model: optionalCollapsed(100, "Model"),
  description: optionalTrimmed(2000, "Description"),
  manufacturerSerialNumber: optionalTrimmed(100, "Serial number"),
  siteId: z.string().uuid("Choose an accessible Site."),
  assignToMe: z.boolean(),
});

export type RegisterAssetFormValues = z.infer<typeof registerAssetSchema>;

const optionalValue = (value: string, collapse: boolean) => {
  const normalized = collapse ? collapseWhitespace(value) : value.trim();
  return normalized || null;
};

export const toRegisterMobileAssetRequest = (
  value: RegisterAssetFormValues,
): RegisterMobileAssetRequest => ({
  classification: value.classification,
  name: collapseWhitespace(value.name),
  manufacturer: optionalValue(value.manufacturer, true),
  model: optionalValue(value.model, true),
  description: optionalValue(value.description, false),
  manufacturerSerialNumber: optionalValue(value.manufacturerSerialNumber, false),
  siteId: value.siteId,
  assignToMe: value.assignToMe,
});

export const isCanonicalQrIdentifier = (value: string) =>
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/u.test(value);
