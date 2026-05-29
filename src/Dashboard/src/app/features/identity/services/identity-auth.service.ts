import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

import { AuthApiService } from '../../../core/auth/services/auth-api.service';
import { AuthSessionService } from '../../../core/auth/services/auth-session.service';
import { AuthResult } from '../../../core/auth/auth.models';
import {
  mapAuthErrors,
  mapDashboardSignInResult
} from '../../../core/auth/utils/auth-response.mapper';

@Injectable({
  providedIn: 'root'
})
export class IdentityAuthService {
  private readonly authApi = inject(AuthApiService);
  private readonly authSession = inject(AuthSessionService);

  readonly accessToken = this.authSession.accessToken;
  readonly isLoggedIn = this.authSession.isLoggedIn;

  async signIn(email: string, password: string): Promise<AuthResult> {
    try {
      const response = await this.authApi.signIn(email, password);
      const result = mapDashboardSignInResult(response);

      if (result.succeeded && response.token) {
        this.setSession(response.token);
        return result;
      }

      this.authSession.clearSession();
      return result;
    } catch (error: unknown) {
      this.authSession.clearSession();

      const payload = error instanceof HttpErrorResponse ? error.error : error;

      return {
        succeeded: false,
        errors: mapAuthErrors(payload)
      };
    }
  }

  logIn(token?: string): void {
    if (token) {
      this.authSession.setSession(token);
      return;
    }

    this.authSession.setLoggedIn(true);
  }

  logOut(): void {
    this.authSession.logOut();
  }

  setLoggedIn(isLoggedIn: boolean): void {
    this.authSession.setLoggedIn(isLoggedIn);
  }

  setSession(token: string): void {
    this.authSession.setSession(token);
  }
}
