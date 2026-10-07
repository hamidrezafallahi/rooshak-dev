import 'server-only';

import { serverApiBaseUrl } from './api';
import { logger } from './logger';

/** Mirrors AnnouncementBarDto (BackEnd). */
export type AnnouncementBar = {
  id: number;
  messageFa: string;
  messageEn: string;
  linkUrl: string;
  backgroundImageUrl: string;
  backgroundColor: string;
  textColor: string;
  heightPx: number;
  startsAt?: string | null;
  endsAt?: string | null;
};

/**
 * The bar that is visible right now (active + inside its schedule), or null.
 * The backend evaluates the schedule; this is re-fetched at least once a minute.
 */
export async function getCurrentAnnouncement(): Promise<AnnouncementBar | null> {
  const url = `${serverApiBaseUrl}/AnnouncementBars/current`;
  try {
    const res = await fetch(url, {
      next: { revalidate: 60, tags: ['announcementBars'] },
      signal: AbortSignal.timeout(2500),
    });
    if (!res.ok) return null;
    const json = await res.json();
    return json?.isSuccess && json?.data ? (json.data as AnnouncementBar) : null;
  } catch (err) {
    logger.error('getCurrentAnnouncement failed', { scope: 'announcement', source: 'server', url }, err);
    return null;
  }
}

/**
 * Turns the admin-entered link into a safe href. Relative values are locale-prefixed;
 * only http(s), mailto and tel are allowed as absolute schemes (no `javascript:`).
 */
export function resolveAnnouncementHref(
  locale: string,
  raw: string,
): { href: string; external: boolean } | null {
  const value = raw?.trim();
  if (!value) return null;

  if (/^(https?:)?\/\//i.test(value)) {
    return { href: value.startsWith('//') ? `https:${value}` : value, external: true };
  }
  if (/^(mailto|tel):/i.test(value)) return { href: value, external: true };
  if (/^[a-z][a-z0-9+.-]*:/i.test(value)) return null;

  const clean = value.replace(/^\/+/, '');
  return { href: clean ? `/${locale}/${clean}` : `/${locale}`, external: false };
}
