import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import { serverApiBaseUrl } from '@lib/api';
import {
  ACCESS_COOKIE,
  REFRESH_COOKIE,
  SESSION_FLAG_COOKIE,
  accessCookieOptions,
  refreshCookieOptions,
  sessionFlagCookieOptions,
} from '@lib/auth-cookies';

export const dynamic = 'force-dynamic';
/** Allow large banner/product image uploads through the BFF. */
export const maxDuration = 120;

type RouteContext = {
  params: Promise<{ path: string[] }>;
};

async function proxyToBackend(req: NextRequest, pathParts: string[]) {
  const path = pathParts.map(encodeURIComponent).join('/');
  const incomingUrl = new URL(req.url);
  const targetUrl = `${serverApiBaseUrl}/${path}${incomingUrl.search}`;

  const cookieStore = await cookies();
  let accessToken = cookieStore.get(ACCESS_COOKIE)?.value;
  const refreshToken = cookieStore.get(REFRESH_COOKIE)?.value;

  const headers = new Headers();
  const contentType = req.headers.get('content-type');
  if (contentType) {
    headers.set('content-type', contentType);
  }
  const accept = req.headers.get('accept');
  if (accept) {
    headers.set('accept', accept);
  }
  if (accessToken) {
    headers.set('Authorization', `Bearer ${accessToken}`);
  }

  const method = req.method.toUpperCase();

  // Backup restore uploads can be hundreds of MB: stream the body straight through
  // instead of buffering it in memory. A streamed body cannot be replayed, so there
  // is no refresh-and-retry here (the admin page refreshes the session first).
  if (method === 'POST' && path === 'Backup/restore') {
    const streamed = await fetch(targetUrl, {
      method,
      headers,
      body: req.body,
      // Node's fetch requires this when the request body is a stream.
      duplex: 'half',
      cache: 'no-store',
    } as RequestInit & { duplex: 'half' });
    const streamedHeaders = new Headers();
    const streamedType = streamed.headers.get('content-type');
    if (streamedType) {
      streamedHeaders.set('content-type', streamedType);
    }
    return new NextResponse(streamed.body, {
      status: streamed.status,
      headers: streamedHeaders,
    });
  }

  const body =
    method === 'GET' || method === 'HEAD' ? undefined : await req.arrayBuffer();

  let upstream = await fetch(targetUrl, {
    method,
    headers,
    body,
    cache: 'no-store',
  });

  if (upstream.status === 401 && refreshToken) {
    const refreshed = await refreshTokens(accessToken, refreshToken);
    if (refreshed.ok) {
      accessToken = refreshed.accessToken;
      headers.set('Authorization', `Bearer ${accessToken}`);
      upstream = await fetch(targetUrl, {
        method,
        headers,
        body,
        cache: 'no-store',
      });

      const responseHeaders = new Headers();
      const upstreamType = upstream.headers.get('content-type');
      if (upstreamType) {
        responseHeaders.set('content-type', upstreamType);
      }

      const res = new NextResponse(upstream.body, {
        status: upstream.status,
        headers: responseHeaders,
      });
      res.cookies.set(ACCESS_COOKIE, refreshed.accessToken, accessCookieOptions);
      if (refreshed.refreshToken) {
        res.cookies.set(
          REFRESH_COOKIE,
          refreshed.refreshToken,
          refreshCookieOptions
        );
      }
      res.cookies.set(SESSION_FLAG_COOKIE, '1', sessionFlagCookieOptions);
      return res;
    }
  }

  const responseHeaders = new Headers();
  const upstreamType = upstream.headers.get('content-type');
  if (upstreamType) {
    responseHeaders.set('content-type', upstreamType);
  }

  return new NextResponse(upstream.body, {
    status: upstream.status,
    headers: responseHeaders,
  });
}

async function refreshTokens(
  accessToken: string | undefined,
  refreshToken: string
) {
  try {
    const response = await fetch(`${serverApiBaseUrl}/Identity/refresh-token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ accessToken, refreshToken }),
      cache: 'no-store',
    });
    const data = await response.json();
    if (!response.ok || !data?.isSuccess || !data.data?.accessToken) {
      return { ok: false as const };
    }
    return {
      ok: true as const,
      accessToken: data.data.accessToken as string,
      refreshToken: (data.data.refreshToken as string | undefined) ?? null,
    };
  } catch {
    return { ok: false as const };
  }
}

async function handle(req: NextRequest, context: RouteContext) {
  const { path } = await context.params;
  if (!path?.length) {
    return NextResponse.json({ isSuccess: false, error: 'Missing path' }, { status: 400 });
  }
  try {
    return await proxyToBackend(req, path);
  } catch {
    return NextResponse.json(
      { isSuccess: false, error: 'BFF proxy failed' },
      { status: 502 }
    );
  }
}

export const GET = handle;
export const POST = handle;
export const PUT = handle;
export const PATCH = handle;
export const DELETE = handle;
