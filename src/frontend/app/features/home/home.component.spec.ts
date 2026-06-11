import { render, screen } from '@testing-library/angular';
import { provideRouter } from '@angular/router';
import { TranslocoTestingModule } from '@jsverse/transloco';

import { HomeComponent } from './home.component';
import { AuthStore } from '@core/auth/auth-store';

const esCO = {
  labels: {
    'panel.title': 'Panel',
    'panel.subtitle': 'Gestiona los eventos de tu organización',
    'dashlet.createEvent.title': 'Crear evento',
    'dashlet.createEvent.desc': 'Registra un nuevo evento',
    'dashlet.createEvent.action': 'Ir al formulario',
    'dashlet.events.title': 'Eventos',
    'dashlet.events.desc': 'Consulta y administra los eventos',
    'dashlet.reports.title': 'Reportes',
    'dashlet.reports.desc': 'Reportes de ocupación',
    'badge.comingSoon': 'Próximamente',
  },
};

async function setup() {
  return render(HomeComponent, {
    imports: [
      TranslocoTestingModule.forRoot({
        langs: { 'es-CO': esCO },
        translocoConfig: { availableLangs: ['es-CO'], defaultLang: 'es-CO' },
        preloadLangs: true,
      }),
    ],
    providers: [
      provideRouter([]),
      // An admin sees the (gated) create-event dashlet.
      { provide: AuthStore, useValue: { hasPermission: () => true } },
    ],
  });
}

describe('HomeComponent', () => {
  it('renders the panel heading and the three dashlets', async () => {
    await setup();

    expect(screen.getByText('Panel')).toBeTruthy();
    expect(screen.getByText('Crear evento')).toBeTruthy();
    expect(screen.getByText('Eventos')).toBeTruthy();
    expect(screen.getByText('Reportes')).toBeTruthy();
    // Only the upcoming report section is flagged as coming soon.
    expect(screen.getAllByText('Próximamente')).toHaveLength(1);
  });

  it('links the active dashlets to their routes', async () => {
    await setup();

    expect(screen.getByText('Crear evento').closest('a')?.getAttribute('href')).toBe(
      '/events/create',
    );
    expect(screen.getByText('Eventos').closest('a')?.getAttribute('href')).toBe('/events');
  });
});
