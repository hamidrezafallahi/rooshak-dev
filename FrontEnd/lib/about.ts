import 'server-only';

import { serverApiBaseUrl } from './api';
import { logger } from './logger';

/** Mirrors AboutSectionDto (BackEnd): one bilingual block of the About / Story page. */
type AboutSectionDto = {
  sectionKey: string;
  displayOrder: number;
  eyebrowFa: string;
  eyebrowEn: string;
  titleFa: string;
  titleEn: string;
  bodyFa: string;
  bodyEn: string;
  linkTextFa: string;
  linkTextEn: string;
  linkUrl: string;
};

/** A section in the visitor's language; empty strings mean "not shown". */
export type AboutSection = {
  key: string;
  eyebrow: string;
  title: string;
  body: string;
  linkText: string;
  linkUrl: string;
};

/** Active sections by key, or null when the API could not be reached (callers fall back to messages). */
export type AboutContent = Map<string, AboutSection> | null;

const pick = (locale: string, fa?: string, en?: string) =>
  ((locale === 'en' ? en || fa : fa || en) ?? '').trim();

/**
 * Sections the admin edits under «داستان لوکا / درباره ما» (AboutSections table).
 * Cached like other storefront data and revalidated when the admin saves (tag
 * `aboutSections`). Only active sections come back, so a switched-off section is
 * simply missing from the map.
 */
export async function getAboutContent(locale: string): Promise<AboutContent> {
  const url = `${serverApiBaseUrl}/AboutSections/public`;
  try {
    const res = await fetch(url, {
      next: { revalidate: 300, tags: ['aboutSections'] },
      signal: AbortSignal.timeout(4000),
    });
    if (!res.ok) {
      logger.error('getAboutContent HTTP error', { scope: 'about', source: 'server', url, status: res.status });
      return null;
    }
    const json = await res.json();
    const rows = (json?.data ?? null) as AboutSectionDto[] | null;
    if (!Array.isArray(rows)) return null;

    return new Map(
      rows.map((r) => [
        r.sectionKey,
        {
          key: r.sectionKey,
          eyebrow: pick(locale, r.eyebrowFa, r.eyebrowEn),
          title: pick(locale, r.titleFa, r.titleEn),
          body: pick(locale, r.bodyFa, r.bodyEn),
          linkText: pick(locale, r.linkTextFa, r.linkTextEn),
          linkUrl: (r.linkUrl ?? '').trim(),
        },
      ]),
    );
  } catch (err) {
    logger.error('getAboutContent failed', { scope: 'about', source: 'server', url }, err);
    return null;
  }
}
