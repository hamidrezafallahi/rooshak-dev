import 'server-only';

import { serverApiBaseUrl } from './api';
import { type CustomFontDto, type FontOption, toCustomFontOptions } from './fontCatalog';
import { logger } from './logger';

/**
 * Fonts the admin uploaded (CustomFonts table + files in uploads/fonts).
 *
 * Fetched once at build time and kept in Next's data cache (revalidated hourly and
 * right after an admin change via the `customFonts` tag), so a page render never
 * waits on this call. Because the files and rows live with the rest of the site's
 * content, a backup restored on another server brings the same fonts with it.
 */
export async function getCustomFonts(): Promise<FontOption[]> {
  const url = `${serverApiBaseUrl}/CustomFonts/public`;
  try {
    const res = await fetch(url, {
      next: { revalidate: 3600, tags: ['customFonts', 'themeSettings'] },
      signal: AbortSignal.timeout(2500),
    });
    if (!res.ok) return [];
    const json = await res.json();
    return toCustomFontOptions(json?.isSuccess ? (json.data as CustomFontDto[]) : null);
  } catch (err) {
    logger.error('getCustomFonts failed', { scope: 'theme', source: 'server', url }, err);
    return [];
  }
}
