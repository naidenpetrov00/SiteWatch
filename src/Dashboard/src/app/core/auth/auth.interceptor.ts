import {
  HttpErrorResponse,
  HttpInterceptorFn
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { AuthSessionService } from './services/auth-session.service';
import { SKIP_AUTH_INTERCEPTOR } from './auth-context';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.context.get(SKIP_AUTH_INTERCEPTOR)) {
    return next(req);
  }

  const authSession = inject(AuthSessionService);
  const router = inject(Router, { optional: true });
  const token = authSession.accessToken();
  const request = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  return next(request).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        authSession.clearSession();
        void router?.navigateByUrl('/sign-in');
      }

      return throwError(() => error);
    })
  );
};
