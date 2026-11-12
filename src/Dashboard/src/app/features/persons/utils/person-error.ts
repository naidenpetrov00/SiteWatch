import { HttpErrorResponse } from '@angular/common/http';

export function getPersonError(
  error: unknown,
  fallback = 'The person operation could not be completed. Please try again.'
): string {
  if (error instanceof HttpErrorResponse) {
    const details = error.error?.details as readonly { message?: string }[] | undefined;
    const validationMessage = details?.find((detail) => detail.message)?.message;

    if (validationMessage) return validationMessage;
    if (typeof error.error?.detail === 'string') return error.error.detail;
    if (typeof error.error?.errorMessage === 'string') return error.error.errorMessage;
    if (typeof error.error?.ErrorMessage === 'string') return error.error.ErrorMessage;
  }

  return fallback;
}
