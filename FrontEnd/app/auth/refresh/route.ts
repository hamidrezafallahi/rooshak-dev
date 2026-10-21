import {
  cookies,
  headers,
} from 'next/headers';
import {
  NextRequest,
  NextResponse,
} from 'next/server';

import {
  ACCESS_COOKIE,
  AUTH_RETRY_COOKIE,
  LOGIN_PATH,
  REFRESH_COOKIE,
  authRetryCookieOptions,
} from '@lib/auth-cookies';
import {
  TokenRefresh,
  clearSessionCookies,
  refreshTokens,
  writeSessionCookies,
} from '@lib/session-refresh';

async function refreshFromCookies(): Promise<TokenRefresh> {
  const cookieStore = await cookies();
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;
  if (!refreshToken) {
    return { kind: 'invalid' };
  }
  return refreshTokens(cookieStore.get(ACCESS_COOKIE)?.value, refreshToken, await headers());
}

/**
 * Called by the browser after a 401 (lib/client-auth.ts). 401 here means the session
 * is over and the client goes to the login page; 502 is a temporary failure and the
 * cookies are kept.
 */
export async function POST() {
  const result = await refreshFromCookies();

  if (result.kind === 'error') {
    return NextResponse.json(
      { isSuccess: false, message: 'Refresh unavailable' },
      { status: 502 }
    );
  }

  if (result.kind === 'invalid') {
    const res = NextResponse.json(
      { isSuccess: false, message: 'Refresh failed' },
      { status: 401 }
    );
    clearSessionCookies(res.cookies);
    return res;
  }

  const res = NextResponse.json({
    isSuccess: true,
    data: {
      refreshed: true,
    },
  });
  writeSessionCookies(res.cookies, result);
  return res;
}

/**
 * Server Components cannot write cookies, so after a 401 they redirect here
 * (lib/server-auth.ts): refresh, then send the browser back to the page with the
 * new cookies, or to the login page when the refresh token is rejected.
 */
export async function GET(req: NextRequest) {
  const result = await refreshFromCookies();

  if (result.kind === 'invalid') {
    const res = redirectTo(LOGIN_PATH);
    clearSessionCookies(res.cookies);
    return res;
  }

  const returnTo = safeReturnPath(req.nextUrl.searchParams.get('returnTo'));
  const res = redirectTo(returnTo);
  // That page renders once more; if it still gets 401 it shows the error instead of looping.
  res.cookies.set(AUTH_RETRY_COOKIE, returnTo, authRetryCookieOptions);
  if (result.kind === 'refreshed') {
    writeSessionCookies(res.cookies, result);
  }
  return res;
}

// Relative Location: req.url behind nginx can carry the internal host.
function redirectTo(path: string) {
  return new NextResponse(null, { status: 307, headers: { Location: path } });
}

// Only paths on this site ("/fa/admin"), never "//host" or "https://host".
function safeReturnPath(value: string | null) {
  return value && /^\/(?![/\\])/.test(value) ? value : '/fa';
}
