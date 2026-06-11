import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AppError } from '../../shared/models/app-error';

/**
 * Normalizes backend errors (RFC 7807 ProblemDetails) into a typed {@link AppError}, so stores
 * and components never handle raw HTTP responses and always translate by error code.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const problem = (error.error ?? {}) as Record<string, unknown>;

      const appError: AppError = {
        status: error.status,
        errorCode: (problem['errorCode'] as string) ?? 'UNKNOWN_ERROR',
        errorKind: (problem['errorKind'] as AppError['errorKind']) ?? 'general',
        params: (problem['params'] as Record<string, unknown> | null) ?? null,
        validationErrors: (problem['errors'] as AppError['validationErrors']) ?? null,
      };

      return throwError(() => appError);
    }),
  );
