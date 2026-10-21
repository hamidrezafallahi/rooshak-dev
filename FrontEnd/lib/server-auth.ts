import 'server-only';

import {
  cookies,
  headers,
} from 'next/headers';
import { redirect } from 'next/navigation';

import {
  ACCESS_COOKIE,
  AUTH_RETRY_COOKIE,
  LOGIN_PATH,
  REFRESH_COOKIE,
} from './auth-cookies';
import { clientIpHeaders } from './client-ip';
import { CURRENT_PATH_HEADER } from './session-refresh';

/**
 * For backend calls made while rendering for the logged-in user: Authorization from
 * candyAccess and the visitor's IP (lib/client-ip.ts).
 */
export async function authHeaders(): Promise<Record<string, string>> {
  const token = (await cookies()).get(ACCESS_COOKIE)?.value;
  return {
    ...clientIpHeaders(await headers()),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

/**
 * A Server Component got 401 from the backend. It cannot store rotated cookies, so
 * /auth/refresh does the refresh and sends the browser back to this page; without a
 * refresh token there is nothing to refresh and the user goes to the login page.
 * Returns (no redirect) when this render already is that retry, so a backend that
 * keeps answering 401 shows an error instead of looping.
 */
export async function handleServerUnauthorized(): Promise<void> {
  const cookieStore = await cookies();
  if (!cookieStore.get(REFRESH_COOKIE)?.value) {
    redirect(LOGIN_PATH);
  }
  const returnTo = (await headers()).get(CURRENT_PATH_HEADER) ?? '/fa';
  if (cookieStore.get(AUTH_RETRY_COOKIE)?.value === returnTo) {
    return;
  }
  redirect(`/auth/refresh?returnTo=${encodeURIComponent(returnTo)}`);
}
