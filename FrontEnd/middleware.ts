// middleware.ts
import createMiddleware from 'next-intl/middleware';

export default createMiddleware({
  locales: ['fa', 'en'],
  defaultLocale: 'fa',
  // Always require /fa/... or /en/... — no unprefixed store routes
  localePrefix: 'always',
  // Locale is in the URL; skip Accept-Language negotiation + NEXT_LOCALE cookie.
  // Set-Cookie on HTML responses forces Cache-Control: no-store and blocks bfcache.
  localeDetection: false,
  localeCookie: false,
});

export const config = {
  // Exclude SEO files, static assets, and Next/API proxies from locale middleware.
  // Without this, /robots.txt and /sitemap.xml get rewritten to /fa/... and 404.
  matcher: [
    '/((?!_next/static|_next/image|favicon.ico|robots\\.txt|sitemap\\.xml|feeds/.*|llms\\.txt|manifest\\.json|uploads/.*|.*\\.(?:svg|png|jpg|jpeg|gif|webp|ico|txt|xml|json)$|api|auth|health).*)',
  ],
};
