import { HttpErrorResponse } from '@angular/common/http';

export function getIssueAttachmentErrorMessage(
  error: unknown,
  fallback: string
): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const details = error.error?.details as readonly { message?: string }[] | undefined;
  const detailMessage = details?.find((detail) => detail.message)?.message;
  if (detailMessage) return detailMessage;

  const errors = error.error?.errors as Record<string, readonly string[]> | undefined;
  const validationMessage = errors && Object.values(errors).flat().find(Boolean);
  if (validationMessage) return validationMessage;

  return error.error?.detail
    ?? error.error?.errorMessage
    ?? fallback;
}
