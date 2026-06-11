import { InjectionToken } from '@angular/core';

/**
 * Base path of the versioned API, centralizado en un único lugar para no repetir `/api/v1`
 * en cada petición. Se puede sobreescribir (por ejemplo, en pruebas) proveyendo otro valor.
 */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '/api/v1',
});
