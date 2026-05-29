import { TestBed } from '@angular/core/testing';

import { AuthSessionService } from './auth-session.service';

function createJwt(expirationOffsetSeconds: number): string {
  const payload = {
    exp: Math.floor(Date.now() / 1000) + expirationOffsetSeconds
  };

  return `${toBase64Url(JSON.stringify({ alg: 'none', typ: 'JWT' }))}.${toBase64Url(JSON.stringify(payload))}.`;
}

function toBase64Url(value: string): string {
  return btoa(value).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '');
}

describe('AuthSessionService', () => {
  beforeEach(() => {
    sessionStorage.clear();
  });

  it('restores a stored token when it is valid', () => {
    const token = createJwt(60);
    sessionStorage.setItem('sitewatch.dashboard.access-token', token);

    TestBed.configureTestingModule({});
    const service = TestBed.runInInjectionContext(() => new AuthSessionService());

    expect(service.accessToken()).toBe(token);
    expect(service.isLoggedIn()).toBeTrue();
  });

  it('removes an expired token on startup', () => {
    const token = createJwt(-60);
    sessionStorage.setItem('sitewatch.dashboard.access-token', token);

    TestBed.configureTestingModule({});
    const service = TestBed.runInInjectionContext(() => new AuthSessionService());

    expect(service.accessToken()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(sessionStorage.getItem('sitewatch.dashboard.access-token')).toBeNull();
  });

  it('clears the stored token when logging out', () => {
    TestBed.configureTestingModule({});
    const service = TestBed.runInInjectionContext(() => new AuthSessionService());

    service.setSession(createJwt(60));
    service.logOut();

    expect(service.accessToken()).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
    expect(sessionStorage.getItem('sitewatch.dashboard.access-token')).toBeNull();
  });
});
