import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthStore } from '@core/auth/auth-store';

/**
 * Attaches the identity token to every request and, on a 401, clears the local session and
 * sends the user to login (except for the login request itself, which surfaces its own error).
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authStore = inject(AuthStore);
  const router = inject(Router);

  const token = authStore.identityToken();
  const authorizedRequest = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authorizedRequest).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/login')) {
        authStore.clearSession();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    }),
  );
};
