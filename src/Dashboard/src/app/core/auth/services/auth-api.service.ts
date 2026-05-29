import { HttpClient, HttpContext } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

import {
  DashboardSignInRequest,
  DashboardSignInResponse
} from '../auth.models';
import { SKIP_AUTH_INTERCEPTOR } from '../auth-context';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly http = inject(HttpClient);
  private readonly apiBaseUrl = this.resolveApiBaseUrl();
  private readonly publicRequestContext = new HttpContext().set(
    SKIP_AUTH_INTERCEPTOR,
    true
  );

  signIn(
    email: string,
    password: string
  ): Promise<DashboardSignInResponse> {
    const request: DashboardSignInRequest = { email, password };

    return firstValueFrom(
      this.http.post<DashboardSignInResponse>(
        `${this.apiBaseUrl}/dashboard/signIn`,
        request,
        {
          context: this.publicRequestContext
        }
      )
    );
  }

  private resolveApiBaseUrl(): string {
    if (typeof window === 'undefined') {
      return '';
    }

    if (
      window.location.hostname === 'localhost' ||
      window.location.hostname === '127.0.0.1'
    ) {
      return 'http://localhost:5293';
    }

    return '';
  }
}
