import { provideHttpClient, withInterceptors } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { authInterceptor } from '../../../core/auth/auth.interceptor';
import { SKIP_AUTH_INTERCEPTOR } from '../../../core/auth/auth-context';
import { AuthSessionService } from '../../../core/auth/services/auth-session.service';
import { IdentityAuthService } from './identity-auth.service';

describe('IdentityAuthService', () => {
  let service: IdentityAuthService;
  let authSession: AuthSessionService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    sessionStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthSessionService,
        IdentityAuthService,
        provideTanStackQuery(new QueryClient()),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(IdentityAuthService);
    authSession = TestBed.inject(AuthSessionService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('stores the token after a successful sign-in', async () => {
    const signInPromise = service.signIn('test@example.com', 'password123');
    const request = httpTesting.expectOne((httpRequest) =>
      httpRequest.url === buildApiUrl('/dashboard/signIn')
    );

    expect(request.request.context.get(SKIP_AUTH_INTERCEPTOR)).toBeTrue();
    expect(request.request.headers.has('Authorization')).toBeFalse();

    request.flush({
      result: { succeeded: true, errors: [] },
      token: 'jwt-token'
    });

    const result = await signInPromise;

    expect(result).toEqual({
      succeeded: true,
      errors: []
    });
    expect(authSession.accessToken()).toBe('jwt-token');
    expect(authSession.isLoggedIn()).toBeTrue();
  });

  it('returns mapped validation errors', async () => {
    const signInPromise = service.signIn('test@example.com', 'password123');
    const request = httpTesting.expectOne((httpRequest) =>
      httpRequest.url === buildApiUrl('/dashboard/signIn')
    );

    request.flush({
      result: { succeeded: false, errors: ['Invalid credentials'] }
    });

    const result = await signInPromise;

    expect(result).toEqual({
      succeeded: false,
      errors: ['Invalid credentials']
    });
    expect(authSession.accessToken()).toBeNull();
    expect(authSession.isLoggedIn()).toBeFalse();
  });

  it('falls back for malformed responses', async () => {
    const signInPromise = service.signIn('test@example.com', 'password123');
    const request = httpTesting.expectOne((httpRequest) =>
      httpRequest.url === buildApiUrl('/dashboard/signIn')
    );

    request.flush(null);

    const result = await signInPromise;

    expect(result).toEqual({
      succeeded: false,
      errors: ['Unable to sign in.']
    });
  });

  it('maps network failures to a stable error message', async () => {
    const signInPromise = service.signIn('test@example.com', 'password123');
    const request = httpTesting.expectOne((httpRequest) =>
      httpRequest.url === buildApiUrl('/dashboard/signIn')
    );

    request.flush('Network failure', {
      status: 500,
      statusText: 'Server Error'
    });

    const result = await signInPromise;

    expect(result).toEqual({
      succeeded: false,
      errors: ['Unable to sign in.']
    });
  });
});
