const DEFAULT_SITE_URL = 'https://rooshakshop.ir';
/** Docker Compose service name — used by Next.js RSC / Route Handlers. */
const DEFAULT_INTERNAL_API_ORIGIN = 'http://backend:8080';

function trimTrailingSlash(value: string) {
  return value.replace(/\/+$/, '');
}

function ensureApiSuffix(value: string) {
  const normalized = trimTrailingSlash(value);

  if (!normalized) {
    return '';
  }

  return normalized.endsWith('/api')
    ? normalized
    : `${normalized}/api`;
}

/**
 * Browser calls go through Next BFF so httpOnly candyAccess can be attached
 * as Authorization. Nginx still routes raw /api/* straight to the backend.
 */
export const browserApiBaseUrl = '/auth/bff';
export const browserAuthBaseUrl = '/auth';

// Server components / Route Handlers must use an absolute internal URL.
// Relative "/api" breaks Node fetch (ERR_INVALID_URL).
const internalServerOrigin =
  process.env.INTERNAL_SERVER_SIDE_API_URL?.trim() ||
  process.env.NEXT_PUBLIC_INTERNAL_API_URL?.trim() ||
  DEFAULT_INTERNAL_API_ORIGIN;

export const serverApiBaseUrl = ensureApiSuffix(internalServerOrigin);

function firstEnvUrl(...values: (string | undefined)[]) {
  for (const value of values) {
    const trimmed = value?.trim();
    if (trimmed) return trimTrailingSlash(trimmed);
  }
  return DEFAULT_SITE_URL;
}

export const siteBaseUrl = firstEnvUrl(
  process.env.NEXT_PUBLIC_SITE_URL,
  process.env.SITE_URL,
);

/** Guard for server-side fetch callers. */
export function requireAbsoluteUrl(url: string, label = 'API URL') {
  if (!/^https?:\/\//i.test(url)) {
    throw new Error(
      `${label} must be absolute (got "${url}"). Set INTERNAL_SERVER_SIDE_API_URL=http://backend:8080`
    );
  }
  return url;
}

// Backward-compatible aliases while callers are migrated.
export const apiBaseUrl = browserApiBaseUrl;
export const apiBaseServerSideUrl = serverApiBaseUrl;
