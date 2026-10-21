import type { NextRequest, NextResponse } from 'next/server';

import { serverApiBaseUrl } from './api';
import {
  ACCESS_COOKIE,
  REFRESH_COOKIE,
  SESSION_FLAG_COOKIE,
  accessCookieOptions,
  refreshCookieOptions,
  sessionFlagCookieOptions,
} from './auth-cookies';
import { clientIpHeaders } from './client-ip';

/**
 * The only caller of /Identity/refresh-token. Used by middleware.ts (before a page
 * renders), the BFF proxy and /auth/refresh (after a 401) and server actions.
 *
 * Server Components (getById, getAll, …) read candyAccess but cannot write cookies,
 * so they have no way to refresh an expired session themselves. The middleware runs
 * this before rendering: the new tokens go into the request cookies (so this render
 * uses them) and into the response cookies (so the browser keeps them).
 */

// Refresh slightly early so a render that starts now does not reach the backend
// with a token that expires mid-request (the backend uses ClockSkew = 0).
const EXPIRY_MARGIN_MS = 60 * 1000;

/** Set by the middleware so a Server Component knows which page to come back to. */
export const CURRENT_PATH_HEADER = 'x-current-path';

export type TokenRefresh =
  | { kind: 'refreshed'; accessToken: string; refreshToken?: string }
  // The backend answered and rejected the refresh token: the session is over.
  | { kind: 'invalid' }
  // Network error, 5xx or rate limit: keep the session, a later request can retry.
  | { kind: 'error' };

export type SessionRefresh = { kind: 'none' } | TokenRefresh;

type CookieWriter = Pick<NextResponse['cookies'], 'set'>;

// Requests that hit 401 together share one backend call per refresh token, so the
// token is rotated once instead of once per request.
const inFlight = new Map<string, Promise<TokenRefresh>>();

/** `incoming` is the visitor's request, so the backend rate-limits the visitor (lib/client-ip.ts). */
export function refreshTokens(
  accessToken: string | undefined,
  refreshToken: string,
  incoming: Pick<Headers, 'get'>,
): Promise<TokenRefresh> {
  let pending = inFlight.get(refreshToken);
  if (!pending) {
    pending = requestTokenRefresh(accessToken, refreshToken, incoming).finally(() =>
      inFlight.delete(refreshToken),
    );
    inFlight.set(refreshToken, pending);
  }
  return pending;
}

async function requestTokenRefresh(
  accessToken: string | undefined,
  refreshToken: string,
  incoming: Pick<Headers, 'get'>,
): Promise<TokenRefresh> {
  try {
    const response = await fetch(`${serverApiBaseUrl}/Identity/refresh-token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', ...clientIpHeaders(incoming) },
      body: JSON.stringify({ accessToken, refreshToken }),
      cache: 'no-store',
    });
    // The backend answers 200 with isSuccess=false for a bad token; anything else
    // (429/5xx, an HTML error page) says nothing about the token.
    if (!response.ok) {
      return { kind: 'error' };
    }
    const data = await response.json();
    if (data?.isSuccess && data.data?.accessToken) {
      return {
        kind: 'refreshed',
        accessToken: data.data.accessToken as string,
        refreshToken: (data.data.refreshToken as string | undefined) ?? undefined,
      };
    }
    return { kind: 'invalid' };
  } catch {
    return { kind: 'error' };
  }
}

function accessTokenExpiresSoon(token: string | undefined) {
  if (!token) return true;
  try {
    const payload = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
    const { exp } = JSON.parse(atob(payload)) as { exp?: number };
    return typeof exp !== 'number' || exp * 1000 - Date.now() < EXPIRY_MARGIN_MS;
  } catch {
    return true;
  }
}

export async function refreshSessionIfNeeded(
  req: NextRequest,
): Promise<SessionRefresh> {
  const refreshToken = req.cookies.get(REFRESH_COOKIE)?.value;
  const accessToken = req.cookies.get(ACCESS_COOKIE)?.value;
  if (!refreshToken || !accessTokenExpiresSoon(accessToken)) {
    return { kind: 'none' };
  }
  return refreshTokens(accessToken, refreshToken, req.headers);
}

export function writeSessionCookies(
  target: CookieWriter,
  tokens: { accessToken: string; refreshToken?: string },
) {
  target.set(ACCESS_COOKIE, tokens.accessToken, accessCookieOptions);
  if (tokens.refreshToken) {
    target.set(REFRESH_COOKIE, tokens.refreshToken, refreshCookieOptions);
  }
  target.set(SESSION_FLAG_COOKIE, '1', sessionFlagCookieOptions);
}

export function clearSessionCookies(target: CookieWriter) {
  target.set(ACCESS_COOKIE, '', { httpOnly: true, path: '/', maxAge: 0 });
  target.set(REFRESH_COOKIE, '', { httpOnly: true, path: '/', maxAge: 0 });
  target.set(SESSION_FLAG_COOKIE, '', { path: '/', maxAge: 0 });
}

export function applySessionToRequest(req: NextRequest, session: SessionRefresh) {
  if (session.kind === 'refreshed') {
    req.cookies.set(ACCESS_COOKIE, session.accessToken);
    if (session.refreshToken) {
      req.cookies.set(REFRESH_COOKIE, session.refreshToken);
    }
  } else if (session.kind === 'invalid') {
    req.cookies.delete(ACCESS_COOKIE);
    req.cookies.delete(REFRESH_COOKIE);
  }
}

export function applySessionToResponse(res: NextResponse, session: SessionRefresh) {
  if (session.kind === 'refreshed') {
    writeSessionCookies(res.cookies, session);
  } else if (session.kind === 'invalid') {
    clearSessionCookies(res.cookies);
  }
}

/** Page path for CURRENT_PATH_HEADER, without the RSC cache-busting param. */
export function currentPath(req: NextRequest) {
  const search = new URLSearchParams(req.nextUrl.searchParams);
  search.delete('_rsc');
  const query = search.toString();
  return query ? `${req.nextUrl.pathname}?${query}` : req.nextUrl.pathname;
}
