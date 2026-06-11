import { render, screen, fireEvent, RenderResult } from '@testing-library/angular';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { RegisterComponent } from './register.component';
import { AuthStore } from '@core/auth/auth-store';

const esCO = {
  labels: {
    'auth.register.title': 'Crear cuenta',
    'auth.register.subtitle': 'Regístrate para reservar eventos.',
    'auth.register.submit': 'Registrarme',
    'auth.login.link': '¿Ya tienes cuenta? Inicia sesión',
    'auth.name': 'Nombre',
    'auth.email': 'Correo electrónico',
    'auth.password': 'Contraseña',
  },
  errors: {
    AUTH_NAME_REQUIRED: 'El nombre es obligatorio.',
    AUTH_EMAIL_INVALID: 'Ingresa un correo electrónico válido.',
    AUTH_PASSWORD_TOO_SHORT: 'La contraseña debe tener al menos 8 caracteres.',
  },
};

interface RegisterApi {
  form: { setValue: (value: Record<string, unknown>) => void };
}

async function setup(register = vi.fn().mockReturnValue(of(undefined))) {
  const view: RenderResult<RegisterComponent> = await render(RegisterComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: AuthStore, useValue: { register } },
      provideRouter([{ path: '**', children: [] }]),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const component = view.fixture.componentInstance as unknown as RegisterApi;
  return { view, component, register };
}

describe('RegisterComponent', () => {
  it('renders the registration form', async () => {
    await setup();

    expect(screen.getByText('Crear cuenta')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Registrarme' })).toBeTruthy();
  });

  it('does not register when the form is empty', async () => {
    const { register } = await setup();

    fireEvent.click(screen.getByRole('button', { name: 'Registrarme' }));

    expect(register).not.toHaveBeenCalled();
  });

  it('registers with the entered details', async () => {
    const { component, register } = await setup();

    component.form.setValue({
      name: 'Persona Nueva',
      email: 'persona@example.com',
      password: 'Password1',
    });
    fireEvent.click(screen.getByRole('button', { name: 'Registrarme' }));

    expect(register).toHaveBeenCalledWith('Persona Nueva', 'persona@example.com', 'Password1');
  });
});
