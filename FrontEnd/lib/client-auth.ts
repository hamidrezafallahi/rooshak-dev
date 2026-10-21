import { browserAuthBaseUrl } from '@lib/api';
import {
  LOGIN_PATH,
  SESSION_FLAG_COOKIE,
} from '@lib/auth-cookies';
import { deleteCookie } from '@utils/core';

/**
 * Browser side of the session. Tokens are httpOnly, so only /auth/* can read or
 * rotate them. Every client API call follows the same rule:
 *   401 → POST /auth/refresh (new access + refresh token) → send the request again;
 *   /auth/refresh answers 401 → the session is over → login page.
 */

type RefreshOutcome = 'refreshed' | 'expired' | 'failed';

let refreshing: Promise<RefreshOutcome> | null = null;
// When the last refresh succeeded: a request sent before that only needs a retry.
let lastRefreshAt = 0;
let redirecting = false;

// Runs outside React (no next-intl hooks), so pick the copy from the URL locale.
export function forbiddenMessage(): string {
  const isEn =
    typeof window !== 'undefined' &&
    /^\/en(\/|$)/.test(window.location.pathname);
  return isEn
    ? 'You do not have permission to do this.'
    : 'شما دسترسی لازم برای انجام این کار را ندارید';
}

/** One /auth/refresh call at a time; concurrent 401s wait for the same one. */
export function refreshSession(): Promise<RefreshOutcome> {
  refreshing ??= requestRefresh().finally(() => {
    refreshing = null;
  });
  return refreshing;
}

async function requestRefresh(): Promise<RefreshOutcome> {
  try {
    const response = await fetch(`${browserAuthBaseUrl}/refresh`, {
      method: 'POST',
      credentials: 'include',
      cache: 'no-store',
    });
    if (response.status === 401) {
      return 'expired';
    }
    const data = response.ok ? await response.json() : null;
    if (!data?.isSuccess) {
      return 'failed';
    }
    lastRefreshAt = Date.now();
    return 'refreshed';
  } catch (err) {
    console.error('Refresh token error:', err);
    return 'failed';
  }
}

export function redirectToLogin() {
  if (typeof window === 'undefined' || redirecting) return;
  deleteCookie(SESSION_FLAG_COOKIE);
  if (/^\/(fa|en)\/register(\/|$)/.test(window.location.pathname)) return;
  redirecting = true;
  window.location.assign(LOGIN_PATH);
}

/**
 * Sends a request and, on 401, refreshes the session and sends it once more.
 * `send` must be repeatable: FormData, string and Blob bodies are, streams are not.
 * A temporary refresh failure (5xx, offline) returns the 401 without logging out.
 */
export async function withAuthRetry<R>(
  send: () => Promise<R>,
  statusOf: (response: R) => unknown,
): Promise<R> {
  // A request sent during a refresh would carry the old cookie and 401 for nothing.
  if (refreshing) await refreshing;

  const sentAt = Date.now();
  const response = await send();
  if (statusOf(response) !== 401) {
    return response;
  }

  if (lastRefreshAt <= sentAt) {
    const outcome = await refreshSession();
    if (outcome === 'expired') {
      redirectToLogin();
      return response;
    }
    if (outcome === 'failed') {
      return response;
    }
  }

  return send();
}

/** fetch() with the session cookies and the 401 → refresh → retry rule. */
export function fetchWithAuth(input: string, init: RequestInit = {}) {
  return withAuthRetry(
    () => fetch(input, { credentials: 'include', ...init }),
    (response) => response.status,
  );
}
