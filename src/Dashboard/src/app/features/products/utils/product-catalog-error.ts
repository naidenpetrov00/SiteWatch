import { HttpErrorResponse } from '@angular/common/http';

export function getProductCatalogError(
  error: unknown,
  fallback = 'The catalog could not be updated. Please try again.'
): string {
  if (error instanceof HttpErrorResponse) {
    const details = error.error?.details as readonly { message?: string }[] | undefined;
    const validationMessage = details?.find((detail) => detail.message)?.message;
    if (validationMessage) return validationMessage;

    if (typeof error.error?.detail === 'string') return error.error.detail;
    if (error.status === 404) return 'This catalog record no longer exists. Refresh the list and try again.';
  }

  return fallback;
}
