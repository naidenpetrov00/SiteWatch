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
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([])]
    });
  });

  it('allows navigation when logged in', () => {
    const authService = TestBed.inject(IdentityAuthService);
    authService.setLoggedIn(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as Route, [] as UrlSegment[])
    );

    expect(result).toBeTrue();
  });

  it('redirects to sign-in when logged out', () => {
    const authService = TestBed.inject(IdentityAuthService);
    authService.setLoggedIn(false);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as Route, [] as UrlSegment[])
    );

    expect(result instanceof UrlTree).toBeTrue();
    expect(TestBed.inject(Router).serializeUrl(result as UrlTree)).toBe(
      '/sign-in'
    );
  });
});
