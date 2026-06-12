import { HttpClient, HttpContext, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { injectMutation } from '@tanstack/angular-query-experimental';

import {
  AuthResult,
  DashboardSignInRequest,
  DashboardSignInResponse
} from '../../../core/auth/auth.models';
import { SKIP_AUTH_INTERCEPTOR } from '../../../core/auth/auth-context';
import { AuthSessionService } from '../../../core/auth/services/auth-session.service';
import {
  mapAuthErrors,
  mapDashboardSignInResult
} from '../../../core/auth/utils/auth-response.mapper';
import { buildApiUrl } from '../../../core/api/api-url';

@Injectable({
  providedIn: 'root'
})
export class IdentityAuthService {
  private readonly http = inject(HttpClient);
  private readonly authSession = inject(AuthSessionService);

  readonly accessToken = this.authSession.accessToken;
  readonly isLoggedIn = this.authSession.isLoggedIn;
  readonly signInMutation = injectMutation<
    DashboardSignInResponse,
    Error,
    DashboardSignInRequest
  >(() => ({
    mutationKey: ['identity', 'signIn'],
    mutationFn: async (request: DashboardSignInRequest) =>
      firstValueFrom(
        this.http.post<DashboardSignInResponse>(
          buildApiUrl('/dashboard/signIn'),
          request,
          {
            context: this.createPublicRequestContext()
          }
        )
      ),
    onSuccess: (response) => {
      const result = mapDashboardSignInResult(response);

      if (result.succeeded && response.token) {
        this.setSession(response.token);
        return;
      }

      this.authSession.clearSession();
    },
    onError: () => {
      this.authSession.clearSession();
    }
  }));

  async signIn(email: string, password: string): Promise<AuthResult> {
    try {
      const response = await this.signInMutation.mutateAsync({ email, password });

      return mapDashboardSignInResult(response);
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

  private createPublicRequestContext(): HttpContext {
    return new HttpContext().set(SKIP_AUTH_INTERCEPTOR, true);
  }
}
