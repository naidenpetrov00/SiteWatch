import { HttpClient, HttpContext } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import {
  HttpTestingController,
  provideHttpClientTesting
} from '@angular/common/http/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { AuthSessionService } from './services/auth-session.service';
import { authInterceptor } from './auth.interceptor';
import { SKIP_AUTH_INTERCEPTOR } from './auth-context';

describe('authInterceptor', () => {
  let httpClient: HttpClient;
  let httpTesting: HttpTestingController;
  let authSession: AuthSessionService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthSessionService,
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting()
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpTesting = TestBed.inject(HttpTestingController);
    authSession = TestBed.inject(AuthSessionService);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('attaches the bearer token to outgoing requests', () => {
    authSession.setSession('test-token');

    httpClient.get('/api/secure').subscribe();

    const request = httpTesting.expectOne('/api/secure');
    expect(request.request.headers.get('Authorization')).toBe('Bearer test-token');
    request.flush({});
  });

  it('skips auth handling for public requests', () => {
    authSession.setSession('test-token');

    httpClient
      .post(
        '/dashboard/signIn',
        { email: 'test@example.com', password: 'password' },
        {
          context: new HttpContext().set(SKIP_AUTH_INTERCEPTOR, true)
        }
      )
      .subscribe();

    const request = httpTesting.expectOne('/dashboard/signIn');
    expect(request.request.headers.has('Authorization')).toBeFalse();
    request.flush({});
  });

  it('clears the session on auth failures', () => {
    authSession.setSession('test-token');

    httpClient.get('/api/secure').subscribe({
      error: () => undefined
    });

    const request = httpTesting.expectOne('/api/secure');
    request.flush('Unauthorized', {
      status: 401,
      statusText: 'Unauthorized'
    });

    expect(authSession.accessToken()).toBeNull();
    expect(authSession.isLoggedIn()).toBeFalse();
  });

  it('clears the session on forbidden responses', () => {
    authSession.setSession('test-token');

    httpClient.get('/api/secure').subscribe({
      error: () => undefined
    });

    const request = httpTesting.expectOne('/api/secure');
    request.flush('Forbidden', {
      status: 403,
      statusText: 'Forbidden'
    });

    expect(authSession.accessToken()).toBeNull();
    expect(authSession.isLoggedIn()).toBeFalse();
  });
});
