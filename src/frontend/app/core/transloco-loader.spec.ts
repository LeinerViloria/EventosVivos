import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { TranslocoHttpLoader } from './transloco-loader';

describe('TranslocoHttpLoader', () => {
  let loader: TranslocoHttpLoader;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), TranslocoHttpLoader],
    });
    loader = TestBed.inject(TranslocoHttpLoader);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => controller.verify());

  it('requests the translation file for the given language and returns its content', () => {
    const translation = { greeting: 'Hola' };
    let result: unknown;

    loader.getTranslation('es-CO').subscribe((value) => (result = value));

    const request = controller.expectOne('/i18n/es-CO.json');
    expect(request.request.method).toBe('GET');
    request.flush(translation);

    expect(result).toEqual(translation);
  });
});
