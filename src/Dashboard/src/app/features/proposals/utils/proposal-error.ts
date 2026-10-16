import { HttpErrorResponse } from '@angular/common/http';

export function getProposalError(
  error: unknown,
  fallback = 'The Proposal operation could not be completed.'
): string {
  if (!(error instanceof HttpErrorResponse)) {
    return fallback;
  }

  const payload = error.error;
  if (payload && typeof payload === 'object') {
    if ('detail' in payload && typeof payload.detail === 'string') {
      return payload.detail;
    }

    if ('details' in payload && Array.isArray(payload.details)) {
      const messages = payload.details
        .map((detail: unknown) =>
          detail && typeof detail === 'object' && 'message' in detail
            ? String(detail.message)
            : ''
        )
        .filter((message: string) => message.length > 0);
      if (messages.length > 0) {
        return messages.join(' ');
      }
    }
  }

  return fallback;
}
