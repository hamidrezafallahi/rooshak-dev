import type { Metadata } from 'next';

import {
  serverApiBaseUrl,
  siteBaseUrl,
} from '@lib/api';
import { toMediaUrl } from '@utils/toMediaUrl';

export const DEFAULT_LOCALE = 'fa' as const;
export const LOCALES = ['fa', 'en'] as const;
export type AppLocale = (typeof LOCALES)[number];

export const SITE_NAME =
  process.env.NEXT_PUBLIC_SITE_NAME?.trim() || 'Rooshak';

/** Path without leading locale. Empty string = home. */
export function cleanPath(path = ''): string {
  return path.replace(/^\/+|\/+$/g, '');
}

/**
 * Locale-aware path matching next-intl `localePrefix: 'always'`.
 * Every page URL is /fa/... or /en/...
 */
export function localizedPath(locale: string, path = ''): string {
  const cleaned = cleanPath(path);
  return cleaned ? `/${locale}/${cleaned}` : `/${locale}`;
}

export function absoluteUrl(locale: string, path = ''): string {
  return `${siteBaseUrl}${localizedPath(locale, path)}`;
}

export function buildAlternates(locale: string, path = '') {
  return {
    canonical: absoluteUrl(locale, path),
    languages: {
      fa: absoluteUrl('fa', path),
      en: absoluteUrl('en', path),
      'x-default': absoluteUrl(DEFAULT_LOCALE, path),
    },
  };
}

/** Used whenever a page has no image of its own, so every share card has a preview. */
export const DEFAULT_OG_IMAGE = '/og-image.jpg';

export function ogLocale(locale: string): string {
  return locale === 'fa' ? 'fa_IR' : 'en_US';
}

type BuildPageMetadataInput = {
  locale: string;
  path?: string;
  title: string;
  description: string;
  images?: (string | undefined | null)[];
  type?: 'website' | 'article';
  noIndex?: boolean;
  keywords?: string[];
  /** Skip slow SEO API so tags stay in the initial <head> (Lighthouse/SEO). */
  skipSeoOverride?: boolean;
  /**
   * For listing pages with ?page= / ?q= etc.
   * Canonical always points to clean `path` (no query).
   * Filtered/search views are noindex by default.
   */
  listingSearchParams?: Record<string, string | string[] | undefined>;
};

export function hasListingFilters(
  searchParams?: Record<string, string | string[] | undefined>,
) {
  if (!searchParams) return false;
  return Object.entries(searchParams).some(([key, value]) => {
    if (key === 'page') return false;
    if (value == null) return false;
    const raw = Array.isArray(value) ? value[0] : value;
    return Boolean(raw && String(raw).trim());
  });
}

export function getListingPage(
  searchParams?: Record<string, string | string[] | undefined>,
) {
  if (!searchParams?.page) return 1;
  const raw = Array.isArray(searchParams.page)
    ? searchParams.page[0]
    : searchParams.page;
  const page = parseInt(raw ?? '1', 10);
  return Number.isFinite(page) && page > 0 ? page : 1;
}

type SeoOverride = {
  title?: string | null;
  description?: string | null;
  keywords?: string | null;
  canonicalPath?: string | null;
  ogImageUrl?: string | null;
  robotsIndex?: boolean;
  robotsFollow?: boolean;
};

async function getSeoOverride(locale: string, path = ''): Promise<SeoOverride | null> {
  const normalizedPath = cleanPath(path);

  try {
    const url = new URL(`${serverApiBaseUrl}/seoSettings/resolve`);
    url.searchParams.set('path', normalizedPath);
    url.searchParams.set('locale', locale);

    // Keep metadata in the initial <head> for crawlers/Lighthouse.
    // A slow backend must not stream SEO tags after </head>.
    const res = await fetch(url.toString(), {
      // force-dynamic pages still need a warm cache so metadata stays in <head>
      cache: 'force-cache',
      next: { revalidate: 300, tags: ['seoSettings'] },
      signal: AbortSignal.timeout(150),
    });

    if (!res.ok) {
      return null;
    }

    const json = await res.json();
    return json?.data ?? null;
  } catch {
    return null;
  }
}

function splitKeywords(keywords?: string[] | string | null): string[] | undefined {
  if (Array.isArray(keywords)) {
    return keywords.length ? keywords : undefined;
  }

  if (!keywords) {
    return undefined;
  }

  const parsed = keywords
    .split(',')
    .map((item) => item.trim())
    .filter(Boolean);

  return parsed.length ? parsed : undefined;
}

function toAbsoluteAssetUrl(url: string): string {
  if (url.startsWith('http://') || url.startsWith('https://')) {
    return url;
  }

  const mediaPath = toMediaUrl(url);
  if (!mediaPath) {
    return siteBaseUrl;
  }

  return `${siteBaseUrl}${mediaPath}`;
}

export async function buildPageMetadata({
  locale,
  path = '',
  title,
  description,
  images = [],
  type = 'website',
  noIndex = false,
  keywords,
  skipSeoOverride = false,
  listingSearchParams,
}: BuildPageMetadataInput): Promise<Metadata> {
  const filtered = hasListingFilters(listingSearchParams);
  const resolvedNoIndexInput = noIndex || filtered;
  const seoOverride = skipSeoOverride
    ? null
    : await getSeoOverride(locale, path);
  const resolvedTitle = seoOverride?.title?.trim() || title;
  const resolvedDescription =
    seoOverride?.description?.trim() ||
    description?.trim() ||
    `${SITE_NAME} — online crystal store`;
  const resolvedCanonicalPath = seoOverride?.canonicalPath?.trim() || path;
  const resolvedNoIndex = seoOverride?.robotsIndex === false ? true : resolvedNoIndexInput;
  const resolvedFollow = seoOverride?.robotsFollow ?? true;
  const resolvedKeywords = splitKeywords(seoOverride?.keywords ?? keywords);
  const resolvedImages = seoOverride?.ogImageUrl
    ? [seoOverride.ogImageUrl, ...images]
    : images;
  const url = absoluteUrl(locale, resolvedCanonicalPath);
  const ogImages = resolvedImages
    .filter((img): img is string => Boolean(img))
    .map((img) => toAbsoluteAssetUrl(img));
  if (!ogImages.length) ogImages.push(`${siteBaseUrl}${DEFAULT_OG_IMAGE}`);

  return {
    metadataBase: new URL(siteBaseUrl),
    title: resolvedTitle,
    description: resolvedDescription,
    keywords: resolvedKeywords,
    alternates: buildAlternates(locale, resolvedCanonicalPath),
    robots: resolvedNoIndex
      ? { index: false, follow: false }
      : { index: true, follow: resolvedFollow },
    openGraph: {
      title: resolvedTitle,
      description: resolvedDescription,
      url,
      siteName: SITE_NAME,
      locale: ogLocale(locale),
      type,
      images: ogImages.length
        ? ogImages.map((url) => ({ url }))
        : undefined,
    },
    twitter: {
      card: ogImages.length ? 'summary_large_image' : 'summary',
      title: resolvedTitle,
      description: resolvedDescription,
      images: ogImages.length ? ogImages : undefined,
    },
  };
}

export function jsonLdScript(data: Record<string, unknown> | Record<string, unknown>[]) {
  return {
    __html: JSON.stringify(data).replace(/</g, '\\u003c'),
  };
}
