export interface JwtPayload {
  exp?: number;
  [key: string]: unknown;
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  const parts = token.split('.');
  if (parts.length < 2) {
    return null;
  }

  try {
    const normalizedPayload = normalizeBase64Url(parts[1]);
    return JSON.parse(atob(normalizedPayload)) as JwtPayload;
  } catch {
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const expirationTime = getTokenExpirationTime(token);
  return expirationTime !== null && Date.now() >= expirationTime;
}

export function getTokenExpirationTime(token: string): number | null {
  const payload = decodeJwtPayload(token);
  if (
    !payload ||
    typeof payload.exp !== 'number' ||
    !Number.isFinite(payload.exp)
  ) {
    return null;
  }

  return payload.exp * 1000;
}

function normalizeBase64Url(value: string): string {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/');
  const remainder = normalized.length % 4;
  return remainder === 0
    ? normalized
    : normalized.padEnd(normalized.length + (4 - remainder), '=');
}
