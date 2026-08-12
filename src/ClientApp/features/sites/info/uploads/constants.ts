export const UPLOAD_ACTION_BOTTOM_CLEARANCE = 112;

export const MAX_UPLOAD_BYTES = {
  image: 50 * 1024 * 1024,
  video: 500 * 1024 * 1024,
  file: 100 * 1024 * 1024,
} as const;

export const UPLOAD_ERROR_COLORS = {
  error: "#b91c1c",
  success: "#166534",
} as const;
