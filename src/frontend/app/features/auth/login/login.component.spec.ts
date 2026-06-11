import { render, screen, fireEvent, RenderResult } from '@testing-library/angular';
import { provideRouter } from '@angular/router';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import { TranslocoTestingModule } from '@jsverse/transloco';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { LoginComponent } from './login.component';
import { AuthStore } from '@core/auth/auth-store';

const esCO = {
  labels: {
    'auth.login.title': 'Iniciar sesión',
    'auth.login.subtitle': 'Ingresa tus credenciales para continuar.',
    'auth.email': 'Correo electrónico',
    'auth.password': 'Contraseña',
    'auth.submit': 'Entrar',
  },
  errors: {
    AUTH_EMAIL_INVALID: 'Ingresa un correo electrónico válido.',
    AUTH_PASSWORD_REQUIRED: 'La contraseña es obligatoria.',
  },
};

interface LoginApi {
  form: { setValue: (value: Record<string, unknown>) => void };
}

async function setup(login = vi.fn().mockReturnValue(of(undefined))) {
  const view: RenderResult<LoginComponent> = await render(LoginComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      { provide: AuthStore, useValue: { login } },
      provideRouter([{ path: '**', children: [] }]),
      providePrimeNG({ theme: { preset: Aura } }),
    ],
  });
  const component = view.fixture.componentInstance as unknown as LoginApi;
  return { view, component, login };
}

describe('LoginComponent', () => {
  it('renders the sign-in form', async () => {
    await setup();

    expect(screen.getByText('Iniciar sesión')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Entrar' })).toBeTruthy();
  });

  it('does not call login when the form is empty', async () => {
    const { login } = await setup();

    fireEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    expect(login).not.toHaveBeenCalled();
  });

  it('calls login with the entered credentials', async () => {
    const { component, login } = await setup();

    component.form.setValue({ email: 'admin@eventosvivos.dev', password: 'Admin123*' });
    fireEvent.click(screen.getByRole('button', { name: 'Entrar' }));

    expect(login).toHaveBeenCalledWith('admin@eventosvivos.dev', 'Admin123*');
  });
});
