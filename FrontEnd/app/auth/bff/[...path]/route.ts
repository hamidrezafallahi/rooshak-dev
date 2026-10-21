import { cookies } from 'next/headers';
import { NextRequest, NextResponse } from 'next/server';

import { serverApiBaseUrl } from '@lib/api';
import {
  ACCESS_COOKIE,
  REFRESH_COOKIE,
} from '@lib/auth-cookies';
import { clientIpHeaders } from '@lib/client-ip';
import {
  clearSessionCookies,
  refreshTokens,
  writeSessionCookies,
} from '@lib/session-refresh';

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

  const headers = new Headers(clientIpHeaders(req.headers));
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
  // is no refresh-and-retry here (the admin page refreshes the session first and
  // re-sends the upload itself after a 401).
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
    const refreshed = await refreshTokens(accessToken, refreshToken, req.headers);
    if (refreshed.kind === 'refreshed') {
      accessToken = refreshed.accessToken;
      headers.set('Authorization', `Bearer ${accessToken}`);
      upstream = await fetch(targetUrl, {
        method,
        headers,
        body,
        cache: 'no-store',
      });

      const res = toClientResponse(upstream);
      writeSessionCookies(res.cookies, refreshed);
      return res;
    }
    if (refreshed.kind === 'invalid') {
      // Dead session: drop its cookies. The client then gets 401 from /auth/refresh
      // too and goes to the login page.
      const res = toClientResponse(upstream);
      clearSessionCookies(res.cookies);
      return res;
    }
  }

  return toClientResponse(upstream);
}

function toClientResponse(upstream: Response) {
  const responseHeaders = new Headers();
  const upstreamType = upstream.headers.get('content-type');
  if (upstreamType) {
    responseHeaders.set('content-type', upstreamType);
  }
  copyDownloadHeaders(upstream, responseHeaders);

  return new NextResponse(upstream.body, {
    status: upstream.status,
    headers: responseHeaders,
  });
}

/** Keeps the file name of downloads (e.g. Excel exports) when proxying. */
function copyDownloadHeaders(upstream: Response, target: Headers) {
  const disposition = upstream.headers.get('content-disposition');
  if (disposition) {
    target.set('content-disposition', disposition);
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
