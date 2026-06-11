/** A single field validation error returned by the backend (422). */
export interface FieldError {
  field: string;
  errorCode: string;
  params: Record<string, unknown> | null;
}

/**
 * Normalized error shape used across the frontend. The backend never sends user-facing text;
 * the UI translates `errorCode` (interpolating `params`) with i18n.
 */
export interface AppError {
  status: number;
  errorCode: string;
  errorKind: 'business' | 'validation' | 'general';
  params: Record<string, unknown> | null;
  validationErrors: FieldError[] | null;
}
