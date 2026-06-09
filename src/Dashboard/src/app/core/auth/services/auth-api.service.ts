import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  DashboardSignInRequest,
  DashboardSignInResponse
} from '../auth.models';
import { ApiClient } from '../../api/api-client';

@Injectable({
  providedIn: 'root'
})
export class AuthApiService {
  private readonly apiClient = inject(ApiClient);

  signIn(
    email: string,
    password: string
  ): Observable<DashboardSignInResponse> {
    const request: DashboardSignInRequest = { email, password };

    return this.apiClient.post<DashboardSignInResponse, DashboardSignInRequest>(
      '/dashboard/signIn',
      request,
      {
        skipAuth: true
      }
    );
  }
}
