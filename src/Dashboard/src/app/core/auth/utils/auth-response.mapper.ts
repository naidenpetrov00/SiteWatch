import { AuthResult, DashboardSignInResponse } from '../auth.models';

export function mapDashboardSignInResult(
  response: DashboardSignInResponse | null | undefined
): AuthResult {
  if (
    response?.result?.succeeded === true &&
    typeof response.token === 'string' &&
    response.token.length > 0
  ) {
    return {
      succeeded: true,
      errors: []
    };
  }

  return {
    succeeded: false,
    errors: mapAuthErrors(response)
  };
}

export function mapAuthErrors(
  payload: unknown,
  fallbackMessage = 'Unable to sign in.'
): string[] {
  const errors = extractStringArray(payload);
  if (errors.length > 0) {
    return errors;
  }

  if (typeof payload === 'string' && payload.trim().length > 0) {
    return [payload];
  }

  return [fallbackMessage];
}

function extractStringArray(payload: unknown): string[] {
  if (Array.isArray(payload)) {
    return payload.filter((item): item is string => typeof item === 'string' && item.length > 0);
  }

  if (payload === null || typeof payload !== 'object') {
    return [];
  }

  const response = payload as {
    errors?: unknown;
    result?: {
      errors?: unknown;
    };
  };

  const directErrors = readStringArray(response.errors);
  if (directErrors.length > 0) {
    return directErrors;
  }

  return readStringArray(response.result?.errors);
}

function readStringArray(value: unknown): string[] {
  if (!Array.isArray(value)) {
    return [];
  }

  return value.filter((item): item is string => typeof item === 'string' && item.length > 0);
}
