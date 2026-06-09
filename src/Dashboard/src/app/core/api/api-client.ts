import {
  HttpClient,
  HttpContext,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams
} from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, throwError } from 'rxjs';

import { SKIP_AUTH_INTERCEPTOR } from '../auth/auth-context';
import { API_CLIENT_CONFIG } from './api-client.config';
import { ApiError } from './api-error';

export interface ApiRequestOptions {
  headers?: HttpHeaders;
  params?: HttpParams;
  context?: HttpContext;
  skipAuth?: boolean;
  withCredentials?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class ApiClient {
  private readonly http = inject(HttpClient);
  private readonly config = inject(API_CLIENT_CONFIG);

  get<T>(path: string, options?: ApiRequestOptions): Observable<T> {
    return this.request<T>('GET', path, options);
  }

  post<TResponse, TBody = unknown>(
    path: string,
    body?: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.request<TResponse>('POST', path, options, body);
  }

  put<TResponse, TBody = unknown>(
    path: string,
    body?: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.request<TResponse>('PUT', path, options, body);
  }

  patch<TResponse, TBody = unknown>(
    path: string,
    body?: TBody,
    options?: ApiRequestOptions
  ): Observable<TResponse> {
    return this.request<TResponse>('PATCH', path, options, body);
  }

  delete<T>(path: string, options?: ApiRequestOptions): Observable<T> {
    return this.request<T>('DELETE', path, options);
  }

  private request<T>(
    method: string,
    path: string,
    options?: ApiRequestOptions,
    body?: unknown
  ): Observable<T> {
    const url = this.buildUrl(path);
    const context = this.buildContext(options);

    return this.http
      .request<T>(method, url, {
        body,
        context,
        headers: options?.headers,
        params: options?.params,
        withCredentials: options?.withCredentials
      })
      .pipe(
        catchError((error: unknown) =>
          throwError(() => this.normalizeError(method, url, error))
        )
      );
  }

  private buildContext(options?: ApiRequestOptions): HttpContext | undefined {
    if (!options?.skipAuth) {
      return options?.context;
    }

    const context = options.context ?? new HttpContext();
    return context.set(SKIP_AUTH_INTERCEPTOR, true);
  }

  private buildUrl(path: string): string {
    if (this.isAbsoluteUrl(path)) {
      return path;
    }

    const baseUrl = this.config.baseUrl.trim();
    if (baseUrl.length === 0) {
      return path;
    }

    const normalizedBaseUrl = baseUrl.replace(/\/$/, '');
    const normalizedPath = path.startsWith('/') ? path : `/${path}`;

    return `${normalizedBaseUrl}${normalizedPath}`;
  }

  private normalizeError(method: string, url: string, error: unknown): ApiError {
    if (error instanceof ApiError) {
      return error;
    }

    if (error instanceof HttpErrorResponse) {
      return new ApiError({
        method,
        url,
        status: error.status,
        statusText: error.statusText,
        body: error.error,
        cause: error
      });
    }

    return new ApiError({
      method,
      url,
      status: 0,
      statusText: 'Unknown Error',
      body: error,
      cause: error
    });
  }

  private isAbsoluteUrl(path: string): boolean {
    return /^https?:\/\//i.test(path);
  }
}
