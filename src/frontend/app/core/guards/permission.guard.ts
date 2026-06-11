import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthStore } from '@core/auth/auth-store';

/** Allows the route only if the user holds the given permission; otherwise sends them home. */
export function permissionGuard(permission: string): CanActivateFn {
  return () => {
    const authStore = inject(AuthStore);
    const router = inject(Router);

    return authStore.hasPermission(permission) ? true : router.createUrlTree(['/']);
  };
}
