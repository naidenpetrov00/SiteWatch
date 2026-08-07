import { TestBed } from '@angular/core/testing';
import {
  Route,
  Router,
  UrlSegment,
  UrlTree,
  provideRouter
} from '@angular/router';

import { authGuard } from './auth.guard';
import { IdentityAuthService } from '../services/identity-auth.service';

describe('authGuard', () => {
  const authService = { isLoggedIn: () => false };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: IdentityAuthService, useValue: authService }
      ]
    });
  });

  it('allows navigation when logged in', () => {
    authService.isLoggedIn = () => true;

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as Route, [] as UrlSegment[])
    );

    expect(result).toBe(true);
  });

  it('redirects to sign-in when logged out', () => {
    authService.isLoggedIn = () => false;

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as Route, [] as UrlSegment[])
    );

    expect(result instanceof UrlTree).toBe(true);
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/sign-in'
    );
  });
});
