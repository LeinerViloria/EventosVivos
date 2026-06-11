import { HttpInterceptorFn } from '@angular/common/http';

/**
 * Adds the client's time zone to every request, so the backend can evaluate the rules that
 * depend on local time. The backend itself operates in UTC.
 */
export const timezoneInterceptor: HttpInterceptorFn = (req, next) => {
  const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;
  return next(req.clone({ setHeaders: { 'X-Timezone': timeZone } }));
};
