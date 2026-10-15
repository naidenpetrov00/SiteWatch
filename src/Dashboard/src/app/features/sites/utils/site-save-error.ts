import { HttpErrorResponse } from '@angular/common/http';

export function getSiteSaveError(error: unknown): string {
  if (error instanceof HttpErrorResponse) {
    const details = error.error?.details as readonly { message?: string }[] | undefined;
    const detailMessage = details?.find((detail) => detail.message)?.message;
    if (detailMessage) return detailMessage;

    const errors = error.error?.errors as Record<string, string[]> | undefined;
    const validationMessage = errors && Object.values(errors).flat().find(Boolean);
    if (validationMessage) return validationMessage;

    if (typeof error.error?.detail === 'string') return error.error.detail;
    if (typeof error.error?.errorMessage === 'string') return error.error.errorMessage;
  }

  return 'Unable to save the Site. Please review the details and try again.';
}
