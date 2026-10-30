export function resolveApiBaseUrl(): string {
  if (typeof window === 'undefined') {
    return '';
  }

  if (
    window.location.hostname === 'localhost' ||
    window.location.hostname === '127.0.0.1'
  ) {
    return 'http://localhost:5293';
  }

  return `${window.location.protocol}//api.${window.location.hostname}`;
}

export function buildApiUrl(path: string): string {
  if (isAbsoluteUrl(path)) {
    return path;
  }

  const baseUrl = resolveApiBaseUrl().trim();
  if (baseUrl.length === 0) {
    return path;
  }

  const normalizedBaseUrl = baseUrl.replace(/\/$/, '');
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;

  return `${normalizedBaseUrl}${normalizedPath}`;
}

function isAbsoluteUrl(path: string): boolean {
  return /^https?:\/\//i.test(path);
}
