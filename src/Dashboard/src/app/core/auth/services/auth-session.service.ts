import { Injectable, signal } from '@angular/core';

import { isTokenExpired } from '../utils/jwt-token.utils';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly tokenStorageKey = 'sitewatch.dashboard.access-token';

  readonly accessToken = signal<string | null>(this.readStoredToken());
  readonly isLoggedIn = signal(this.accessToken() !== null);

  setSession(token: string): void {
    this.accessToken.set(token);
    this.isLoggedIn.set(true);
    this.storeToken(token);
  }

  logIn(token?: string): void {
    if (token) {
      this.setSession(token);
      return;
    }

    this.isLoggedIn.set(true);
  }

  logOut(): void {
    this.clearSession();
  }

  setLoggedIn(isLoggedIn: boolean): void {
    this.isLoggedIn.set(isLoggedIn);

    if (!isLoggedIn) {
      this.clearSession();
    }
  }

  clearSession(): void {
    this.accessToken.set(null);
    this.isLoggedIn.set(false);
    this.removeStoredToken();
  }

  private readStoredToken(): string | null {
    if (typeof sessionStorage === 'undefined') {
      return null;
    }

    const token = sessionStorage.getItem(this.tokenStorageKey);
    if (!token || isTokenExpired(token)) {
      this.removeStoredToken();
      return null;
    }

    return token;
  }

  private storeToken(token: string): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.setItem(this.tokenStorageKey, token);
  }

  private removeStoredToken(): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.removeItem(this.tokenStorageKey);
  }
}
