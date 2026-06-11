import { inject, Pipe, PipeTransform } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';

/**
 * Translates a numeric enum value into its label using i18n, e.g.
 * `{{ event.type | enumLabel: 'eventType' }}`. The contract travels as the number.
 */
@Pipe({ name: 'enumLabel' })
export class EnumLabelPipe implements PipeTransform {
  private readonly transloco = inject(TranslocoService);

  transform(value: number | null | undefined, enumName: string): string {
    if (value === null || value === undefined) {
      return '';
    }

    return this.transloco.translate(`enums.${enumName}.${value}`);
  }
}
