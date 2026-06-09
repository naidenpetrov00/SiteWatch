import { InjectionToken, Provider } from '@angular/core';

export interface ApiClientConfig {
  baseUrl: string;
}

export const API_CLIENT_CONFIG = new InjectionToken<ApiClientConfig>(
  'API_CLIENT_CONFIG'
);

export function provideApiClientConfig(config: ApiClientConfig): Provider {
  return {
    provide: API_CLIENT_CONFIG,
    useValue: config
  };
}

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

  return '';
}
