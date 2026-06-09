import { TestBed } from '@angular/core/testing';
import { Observable, of, throwError } from 'rxjs';

import { IdentityAuthService } from './identity-auth.service';
import { AuthApiService } from '../../../core/auth/services/auth-api.service';
import { AuthSessionService } from '../../../core/auth/services/auth-session.service';

describe('IdentityAuthService', () => {
  let authApi: {
    signIn: (email: string, password: string) => Observable<unknown>;
  };
  let service: IdentityAuthService;
  let authSession: AuthSessionService;

  beforeEach(() => {
    authApi = {
      signIn: () =>
        of({
          result: { succeeded: true, errors: [] },
          token: 'jwt-token'
        })
    };

    sessionStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        IdentityAuthService,
        AuthSessionService,
        { provide: AuthApiService, useValue: authApi }
      ]
    });

    service = TestBed.inject(IdentityAuthService);
    authSession = TestBed.inject(AuthSessionService);
  });

  it('stores the token after a successful sign-in', async () => {
    const result = await service.signIn('test@example.com', 'password123');

    expect(result).toEqual({
      succeeded: true,
      errors: []
    });
    expect(authSession.accessToken()).toBe('jwt-token');
    expect(authSession.isLoggedIn()).toBeTrue();
  });

  it('returns mapped validation errors', async () => {
    authApi.signIn = () =>
      of({
        result: { succeeded: false, errors: ['Invalid credentials'] }
      });

    const result = await service.signIn('test@example.com', 'password123');

    expect(result).toEqual({
      succeeded: false,
      errors: ['Invalid credentials']
    });
    expect(authSession.accessToken()).toBeNull();
    expect(authSession.isLoggedIn()).toBeFalse();
  });

  it('falls back for malformed responses', async () => {
    authApi.signIn = () => of(null);

    const result = await service.signIn('test@example.com', 'password123');

    expect(result).toEqual({
      succeeded: false,
      errors: ['Unable to sign in.']
    });
  });

  it('maps network failures to a stable error message', async () => {
    authApi.signIn = () => throwError(() => new Error('Network failure'));

    const result = await service.signIn('test@example.com', 'password123');

    expect(result).toEqual({
      succeeded: false,
      errors: ['Unable to sign in.']
    });
  });
});
