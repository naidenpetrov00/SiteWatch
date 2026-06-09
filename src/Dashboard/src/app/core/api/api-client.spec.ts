import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';

import { AuthSessionService } from '../auth/services/auth-session.service';
import { authInterceptor } from '../auth/auth.interceptor';
import { SKIP_AUTH_INTERCEPTOR } from '../auth/auth-context';
import { provideApiClientConfig } from './api-client.config';
import { ApiClient } from './api-client';
import { ApiError } from './api-error';

describe('ApiClient', () => {
  let apiClient: ApiClient;
  let httpTesting: HttpTestingController;
  let authSession: AuthSessionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthSessionService,
        provideApiClientConfig({ baseUrl: 'http://localhost:5293' }),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    apiClient = TestBed.inject(ApiClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authSession = TestBed.inject(AuthSessionService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('uses the configured base url and skips auth for public requests', () => {
    authSession.setSession('test-token');

    apiClient
      .post('/dashboard/signIn', { email: 'test@example.com', password: 'password' }, { skipAuth: true })
      .subscribe();

    const request = httpTesting.expectOne('http://localhost:5293/dashboard/signIn');
    expect(request.request.context.get(SKIP_AUTH_INTERCEPTOR)).toBeTrue();
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush({ result: { succeeded: true } });
  });

  it('attaches the bearer token for secured requests', () => {
    authSession.setSession('test-token');

    apiClient.get('/api/secure').subscribe();

    const request = httpTesting.expectOne('http://localhost:5293/api/secure');
    expect(request.request.headers.get('Authorization')).toBe('Bearer test-token');
    request.flush({ ok: true });
  });

  it('normalizes http failures into ApiError', () => {
    authSession.setSession('test-token');

    let capturedError: unknown;

    apiClient.get('/api/secure').subscribe({
      error: (error) => {
        capturedError = error;
      }
    });

    const request = httpTesting.expectOne('http://localhost:5293/api/secure');
    request.flush('Unauthorized', {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(capturedError instanceof ApiError).toBeTrue();
    const error = capturedError as ApiError;
    expect(error.name).toBe('ApiError');
    expect(error.method).toBe('GET');
    expect(error.url).toBe('http://localhost:5293/api/secure');
    expect(error.status).toBe(401);
    expect(error.body).toBe('Unauthorized');
    expect(authSession.accessToken()).toBeNull();
    expect(authSession.isLoggedIn()).toBeFalse();
  });
});
