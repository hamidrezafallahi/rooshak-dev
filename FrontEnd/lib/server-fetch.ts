import 'server-only';

import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

import { requireAbsoluteUrl, serverApiBaseUrl } from './api';
import {
  ACCESS_COOKIE,
  LOGIN_PATH,
  REFRESH_COOKIE,
} from './auth-cookies';
import { logger } from './logger';
import {
  clearSessionCookies,
  refreshTokens,
  writeSessionCookies,
} from './session-refresh';

type FetchOptions = {
  endpoint: string;
  method?: string;
  body?: FormData | Record<string, unknown> | null;
};

/**
 * Backend call for Server Actions and Route Handlers, which may write cookies
 * (Server Components may not: see lib/server-auth.ts). On 401 it gets new tokens,
 * stores them and retries once; if the refresh token is rejected it clears the
 * session and redirects to the login page. Throws on any other non-2xx answer.
 */
export async function authenticatedFetch<T>({
  endpoint,
  method = 'GET',
  body,
}: FetchOptions): Promise<T> {
  const cookieStore = await cookies();

  const accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;

  const url = requireAbsoluteUrl(
    `${serverApiBaseUrl}/${endpoint.replace(/^\/+/, '')}`,
    'authenticatedFetch URL',
  );

  async function execute(token?: string) {
    return fetch(url, {
      method,
      headers: getHeaders(body, token),
      body: body instanceof FormData
        ? body
        : body
          ? JSON.stringify(body)
          : undefined,
      cache: 'no-store',
    });
  }

  let response = await execute(accessToken);

  // access token expired
  if (response.status === 401) {
    const refreshed = refreshToken
      ? await refreshTokens(accessToken, refreshToken)
      : { kind: 'invalid' as const };

    if (refreshed.kind === 'invalid') {
      clearSessionCookies(cookieStore);
      redirect(LOGIN_PATH);
    }

    // On a temporary refresh failure the 401 below is thrown and the session kept.
    if (refreshed.kind === 'refreshed') {
      writeSessionCookies(cookieStore, refreshed);
      response = await execute(refreshed.accessToken);
    }
  }

  if (response.status === 403) {
    logger.warn('authenticatedFetch forbidden', {
      scope: 'server-fetch',
      source: 'server',
      url,
      status: 403,
    });
    throw new Error('FORBIDDEN');
  }

  if (!response.ok) {
    logger.error('authenticatedFetch HTTP error', {
      scope: 'server-fetch',
      source: 'server',
      url,
      status: response.status,
    });
    throw new Error(
      `HTTP ${response.status}: ${response.statusText}`
    );
  }

  return response.json();
}

function getHeaders(
  body: FetchOptions['body'],
  token?: string
) {
  const headers: Record<string, string> = {};

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  if (!(body instanceof FormData)) {
    headers['Content-Type'] = 'application/json';
  }

  return headers;
}
