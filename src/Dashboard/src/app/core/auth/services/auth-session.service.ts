import { Injectable, inject, signal } from '@angular/core';
import { Router } from '@angular/router';

import { getTokenExpirationTime } from '../utils/jwt-token.utils';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly tokenStorageKey = 'sitewatch.dashboard.access-token';
  private readonly router = inject(Router, { optional: true });
  private expirationTimer: ReturnType<typeof setTimeout> | null = null;

  readonly accessToken = signal<string | null>(null);
  readonly isLoggedIn = signal(false);

  constructor() {
    const token = this.readStoredToken();
    if (token) {
      this.applySession(token);
    }
  }

  setSession(token: string): void {
    this.storeToken(token);
    this.applySession(token);
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
    this.clearExpirationTimer();
    this.accessToken.set(null);
    this.isLoggedIn.set(false);
    this.removeStoredToken();
  }

  private readStoredToken(): string | null {
    const persistentStorage = this.getStorage('local');
    const legacyStorage = this.getStorage('session');
    const legacyToken = this.readToken(legacyStorage);

    if (legacyToken !== null) {
      if (this.isValidToken(legacyToken)) {
        if (this.writeToken(persistentStorage, legacyToken)) {
          this.removeToken(legacyStorage);
        }
        return legacyToken;
      }

      this.removeToken(persistentStorage);
      this.removeToken(legacyStorage);
      return null;
    }

    const persistentToken = this.readToken(persistentStorage);
    if (this.isValidToken(persistentToken)) {
      return persistentToken;
    }

    this.removeToken(persistentStorage);
    return null;
  }

  private storeToken(token: string): void {
    const persistentStorage = this.getStorage('local');

    if (this.writeToken(persistentStorage, token)) {
      this.removeToken(this.getStorage('session'));
    } else {
      this.writeToken(this.getStorage('session'), token);
    }
  }

  private removeStoredToken(): void {
    this.removeToken(this.getStorage('local'));
    this.removeToken(this.getStorage('session'));
  }

  private applySession(token: string): void {
    this.clearExpirationTimer();
    this.accessToken.set(token);
    this.isLoggedIn.set(true);

    const expirationTime = getTokenExpirationTime(token);
    if (expirationTime === null) {
      return;
    }

    const delay = expirationTime - Date.now();
    if (delay <= 0) {
      this.expireSession();
      return;
    }

    this.expirationTimer = setTimeout(() => this.expireSession(), delay);
  }

  private expireSession(): void {
    this.clearSession();
    void this.router?.navigateByUrl('/sign-in');
  }

  private clearExpirationTimer(): void {
    if (this.expirationTimer === null) {
      return;
    }

    clearTimeout(this.expirationTimer);
    this.expirationTimer = null;
  }

  private isValidToken(token: string | null): token is string {
    if (!token) {
      return false;
    }

    const expirationTime = getTokenExpirationTime(token);
    return expirationTime !== null && Date.now() < expirationTime;
  }

  private getStorage(type: 'local' | 'session'): Storage | null {
    if (typeof window === 'undefined') {
      return null;
    }

    try {
      return type === 'local' ? window.localStorage : window.sessionStorage;
    } catch {
      return null;
    }
  }

  private readToken(storage: Storage | null): string | null {
    try {
      return storage?.getItem(this.tokenStorageKey) ?? null;
    } catch {
      return null;
    }
  }

  private writeToken(storage: Storage | null, token: string): boolean {
    if (!storage) {
      return false;
    }

    try {
      storage.setItem(this.tokenStorageKey, token);
      return true;
    } catch {
      return false;
    }
  }

  private removeToken(storage: Storage | null): void {
    try {
      storage?.removeItem(this.tokenStorageKey);
    } catch {
      // Browser storage can be unavailable. The in-memory session is still cleared.
    }
  }
}
