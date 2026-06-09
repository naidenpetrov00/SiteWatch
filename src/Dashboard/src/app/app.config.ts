import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withComponentInputBinding } from '@angular/router';

import { routes } from './app.routes';
import { authInterceptor } from './core/auth/auth.interceptor';
import {
  provideApiClientConfig,
  resolveApiBaseUrl
} from './core/api/api-client.config';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideAnimations(),
    provideApiClientConfig({ baseUrl: resolveApiBaseUrl() }),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideRouter(routes, withComponentInputBinding()),
  ]
};
