import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';

import { AuthStore } from './auth-store';

/** Builds a JWT-shaped token whose payload carries the given claims (signature is irrelevant here). */
function makeToken(claims: Record<string, unknown>): string {
  return `header.${btoa(JSON.stringify(claims))}.signature`;
}

/** Builds a JWT-shaped token with a UTF-8 encoded payload (handles accents, ñ, etc.). */
function makeUtf8Token(claims: Record<string, unknown>): string {
  const json = JSON.stringify(claims);
  const bytes = new TextEncoder().encode(json);
  const binary = Array.from(bytes, (b) => String.fromCharCode(b)).join('');
  return `header.${btoa(binary)}.signature`;
}

describe('AuthStore', () => {
  let store: AuthStore;
  let controller: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: Router, useValue: { navigate: () => undefined } },
      ],
    });
    store = TestBed.inject(AuthStore);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    controller.verify();
    localStorage.clear();
  });

  it('starts unauthenticated', () => {
    expect(store.isAuthenticated()).toBe(false);
    expect(store.identityToken()).toBeNull();
  });

  it('persists the tokens and decodes the user on login', () => {
    const permissionsToken = makeToken({
      role: 'Admin',
      name: 'Administrador',
      perm: ['events.create', 'events.read'],
    });

    store.login('admin@eventosvivos.dev', 'Admin123*').subscribe();
    controller
      .expectOne((request) => request.url.includes('/auth/login'))
      .flush({ identityToken: 'identity-token', permissionsToken });

    expect(store.isAuthenticated()).toBe(true);
    expect(store.identityToken()).toBe('identity-token');
    expect(store.user()?.name).toBe('Administrador');
    expect(store.user()?.role).toBe('Admin');
    expect(store.hasPermission('events.create')).toBe(true);
    expect(store.hasPermission('reports.read')).toBe(false);
  });

  it('normalizes a single permission claim into a list', () => {
    const permissionsToken = makeToken({ role: 'User', name: 'Usuario', perm: 'events.read' });

    store.login('usuario@eventosvivos.dev', 'Usuario123*').subscribe();
    controller
      .expectOne((request) => request.url.includes('/auth/login'))
      .flush({ identityToken: 'identity-token', permissionsToken });

    expect(store.hasPermission('events.read')).toBe(true);
    expect(store.hasPermission('events.create')).toBe(false);
  });

  it('decodes names with accented characters correctly', () => {
    const permissionsToken = makeUtf8Token({
      role: 'User',
      name: 'María José Ñoño',
      email: 'maria@example.com',
      perm: 'reservations.read.own',
    });

    store.login('maria@example.com', 'Password1*').subscribe();
    controller
      .expectOne((request) => request.url.includes('/auth/login'))
      .flush({ identityToken: 'identity-token', permissionsToken });

    expect(store.user()?.name).toBe('María José Ñoño');
  });

  it('clears the session on logout', () => {
    const permissionsToken = makeToken({ role: 'User', name: 'Usuario', perm: 'events.read' });
    store.login('usuario@eventosvivos.dev', 'Usuario123*').subscribe();
    controller
      .expectOne((request) => request.url.includes('/auth/login'))
      .flush({ identityToken: 'identity-token', permissionsToken });

    store.logout();
    controller.expectOne((request) => request.url.includes('/auth/logout')).flush({});

    expect(store.isAuthenticated()).toBe(false);
    expect(store.identityToken()).toBeNull();
  });
});
