import { MessageService } from 'primeng/api';
import { TranslocoService } from '@jsverse/transloco';
import { AppError } from '@shared/models/app-error';

/**
 * Shows a normalized backend error as toast(s): one per field for a validation failure (422),
 * otherwise a single message. The backend only sends codes; i18n turns each into the shown text.
 */
export function showAppError(
  error: AppError,
  messages: MessageService,
  transloco: TranslocoService,
): void {
  const codes = error.validationErrors?.length
    ? error.validationErrors.map((fieldError) => ({
        code: fieldError.errorCode,
        params: fieldError.params,
      }))
    : [{ code: error.errorCode, params: error.params }];

  for (const { code, params } of codes) {
    messages.add({
      severity: 'error',
      detail: transloco.translate(`errors.${code}`, params ?? undefined),
    });
  }
}
