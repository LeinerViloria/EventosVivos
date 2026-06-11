/** Response of POST /api/v1/auth/login: the two tokens issued on sign-in. */
export interface LoginResponse {
  identityToken: string;
  permissionsToken: string;
}

/** The current user as decoded from the permissions token, used only to drive the UI. */
export interface AuthUser {
  name: string;
  email: string;
  role: string;
  permissions: string[];
}
