const isProd = process.env.NODE_ENV === 'production';

export const ACCESS_COOKIE = 'candyAccess';
export const REFRESH_COOKIE = 'candyRefresh';
/** Non-httpOnly flag for client UI only — never stores a token. */
export const SESSION_FLAG_COOKIE = 'candySession';
/** Set by GET /auth/refresh to the page it returns to, so that page does not bounce back on another 401. */
export const AUTH_RETRY_COOKIE = 'candyAuthRetry';

/** Where every layer sends the user once the refresh token is rejected. */
export const LOGIN_PATH = '/fa/register';

export const accessCookieOptions = {
  httpOnly: true,
  secure: isProd,
  sameSite: 'lax' as const,
  path: '/',
  maxAge: 60 * 60,
};

export const refreshCookieOptions = {
  httpOnly: true,
  secure: isProd,
  sameSite: 'lax' as const,
  path: '/',
  maxAge: 60 * 60 * 24 * 30,
};

export const sessionFlagCookieOptions = {
  httpOnly: false,
  secure: isProd,
  sameSite: 'lax' as const,
  path: '/',
  maxAge: 60 * 60 * 24 * 30,
};

export const authRetryCookieOptions = {
  httpOnly: true,
  secure: isProd,
  sameSite: 'lax' as const,
  path: '/',
  maxAge: 30,
};
