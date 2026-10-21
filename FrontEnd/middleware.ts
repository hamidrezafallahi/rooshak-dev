// middleware.ts
import createMiddleware from 'next-intl/middleware';
import type { NextRequest } from 'next/server';

import {
  CURRENT_PATH_HEADER,
  applySessionToRequest,
  applySessionToResponse,
  currentPath,
  refreshSessionIfNeeded,
} from '@lib/session-refresh';

const intlMiddleware = createMiddleware({
  locales: ['fa', 'en'],
  defaultLocale: 'fa',
  // Always require /fa/... or /en/... — no unprefixed store routes
  localePrefix: 'always',
  // Locale is in the URL; skip Accept-Language negotiation + NEXT_LOCALE cookie.
  // Set-Cookie on HTML responses forces Cache-Control: no-store and blocks bfcache.
  localeDetection: false,
  localeCookie: false,
});

export default async function middleware(req: NextRequest) {
  // Only does work when candyAccess is missing/expiring and candyRefresh exists,
  // so normal page loads send no extra request and set no cookies.
  const session = await refreshSessionIfNeeded(req);
  // next-intl forwards req.headers (incl. the updated cookie) to the render.
  applySessionToRequest(req, session);
  // Lets a Server Component that still gets 401 come back here after /auth/refresh.
  req.headers.set(CURRENT_PATH_HEADER, currentPath(req));
  const res = intlMiddleware(req);
  applySessionToResponse(res, session);
  return res;
}

export const config = {
  // Exclude SEO files, static assets, and Next/API proxies from locale middleware.
  // Without this, /robots.txt and /sitemap.xml get rewritten to /fa/... and 404.
  matcher: [
    '/((?!_next/static|_next/image|favicon.ico|robots\\.txt|sitemap\\.xml|feeds/.*|llms\\.txt|manifest\\.json|uploads/.*|.*\\.(?:svg|png|jpg|jpeg|gif|webp|avif|ico|txt|xml|json|mp4|webm|glb|gltf|usdz|woff2?)$|api|auth|health).*)',
  ],
};
