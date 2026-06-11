import { computed, inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { map, Observable, tap } from 'rxjs';
import { API_BASE_URL } from '@core/api-base-url';
import { AuthUser, LoginResponse } from '@shared/models/auth';

const IDENTITY_KEY = 'ev.identityToken';
const PERMISSIONS_KEY = 'ev.permissionsToken';

/**
 * Holds the signed-in session. Tokens live in localStorage; the identity token travels in every
 * request, and the permissions token is decoded into {@link AuthUser} only to drive the UI.
 */
@Injectable({ providedIn: 'root' })
export class AuthStore {
  private readonly http = inject(HttpClient);
  private readonly apiBase = inject(API_BASE_URL);
  private readonly router = inject(Router);

  private readonly currentUser = signal<AuthUser | null>(this.restore());

  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUser() !== null);

  identityToken(): string | null {
    return localStorage.getItem(IDENTITY_KEY);
  }

  hasPermission(permission: string): boolean {
    return this.currentUser()?.permissions.includes(permission) ?? false;
  }

  login(email: string, password: string): Observable<void> {
    return this.http.post<LoginResponse>(`${this.apiBase}/auth/login`, { email, password }).pipe(
      tap((response) => this.persist(response)),
      map(() => undefined),
    );
  }

  /** Public self-registration; the backend always assigns the regular user role. */
  register(name: string, email: string, password: string): Observable<void> {
    return this.http
      .post<LoginResponse>(`${this.apiBase}/auth/register`, { name, email, password })
      .pipe(
        tap((response) => this.persist(response)),
        map(() => undefined),
      );
  }

  logout(): void {
    if (this.identityToken()) {
      // Best-effort revocation in Redis; the local session is cleared regardless of the outcome.
      this.http.post(`${this.apiBase}/auth/logout`, {}).subscribe({ error: () => undefined });
    }
    this.clearSession();
    this.router.navigate(['/login']);
  }

  /** Clears the local session without calling the backend (used when a request returns 401). */
  clearSession(): void {
    localStorage.removeItem(IDENTITY_KEY);
    localStorage.removeItem(PERMISSIONS_KEY);
    this.currentUser.set(null);
  }

  private persist(response: LoginResponse): void {
    localStorage.setItem(IDENTITY_KEY, response.identityToken);
    localStorage.setItem(PERMISSIONS_KEY, response.permissionsToken);
    this.currentUser.set(decodeUser(response.permissionsToken));
  }

  private restore(): AuthUser | null {
    const token = localStorage.getItem(PERMISSIONS_KEY);
    return token ? decodeUser(token) : null;
  }
}

function decodeUser(token: string): AuthUser | null {
  try {
    const payload = token.split('.')[1];
    const claims = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/'))) as Record<
      string,
      unknown
    >;
    const perm = claims['perm'];
    const permissions = Array.isArray(perm) ? (perm as string[]) : perm ? [perm as string] : [];

    return {
      name: (claims['name'] as string) ?? '',
      role: (claims['role'] as string) ?? '',
      permissions,
    };
  } catch {
    return null;
  }
}
