import { serverApiBaseUrl, siteBaseUrl } from '@lib/api';
import { absoluteUrl, DEFAULT_LOCALE, INDEXED_LOCALES } from '@lib/seo';
import { toMediaUrl } from '@utils/toMediaUrl';

// Must be rendered per request: the backend is unreachable during `next build`,
// so a prerendered sitemap would be frozen with only the static paths.
export const dynamic = 'force-dynamic';

const revalidate = 3600;

type ChangeFreq =
  | 'always'
  | 'hourly'
  | 'daily'
  | 'weekly'
  | 'monthly'
  | 'yearly'
  | 'never';

type SitemapRecord = {
  key: string;
  lastmod?: string;
  images?: string[];
};

type SitemapEntry = {
  path: string;
  priority: number;
  changeFrequency: ChangeFreq;
};

const FETCH_TIMEOUT_MS = 2500;

function xmlEscape(value: string): string {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&apos;');
}

// The API serializes .NET DateTimes without a timezone (e.g.
// "2026-09-16T05:59:17.241546"), which W3C Datetime / Google reject.
// Backend timestamps are UTC, so treat zone-less values as UTC.
function toW3cDate(value?: string | null): string | undefined {
  if (!value) return undefined;
  const trimmed = value.trim();
  const hasZone = /(Z|[+-]\d{2}:?\d{2})$/i.test(trimmed);
  const date = new Date(hasZone || !trimmed.includes('T') ? trimmed : `${trimmed}Z`);
  if (Number.isNaN(date.getTime()) || date.getUTCFullYear() < 2000) return undefined;
  return date.toISOString().replace(/\.\d{3}Z$/, 'Z');
}

function toAbsoluteImage(url?: string | null) {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://')) return url;
  const mediaPath = toMediaUrl(url);
  return mediaPath ? `${siteBaseUrl}${mediaPath}` : '';
}

function extractRecords(payload: unknown): SitemapRecord[] {
  if (!payload) return [];

  const list = Array.isArray(payload)
    ? payload
    : Array.isArray((payload as { records?: unknown[] }).records)
      ? (payload as { records: unknown[] }).records
      : Array.isArray((payload as { data?: unknown[] }).data)
        ? (payload as { data: unknown[] }).data
        : [];

  return list
    .map((item) => {
      if (typeof item === 'string' || typeof item === 'number') {
        return { key: String(item) };
      }
      if (!item || typeof item !== 'object') return { key: '' };
      const record = item as {
        slug?: string | number | null;
        id?: string | number | null;
        name?: string | null;
        updatedAt?: string | null;
        imageUrls?: string[] | null;
        ImageUrls?: string[] | null;
      };
      const key = String(record.slug ?? record.id ?? record.name ?? '').trim();
      const images = (record.imageUrls || record.ImageUrls || [])
        .map((img) => toAbsoluteImage(img))
        .filter(Boolean);
      return {
        key,
        lastmod: toW3cDate(record.updatedAt),
        images,
      };
    })
    .filter((item) => Boolean(item.key));
}

async function fetchRecords(endpoint: string): Promise<SitemapRecord[]> {
  try {
    const res = await fetch(`${serverApiBaseUrl}/${endpoint}`, {
      next: { revalidate },
      signal: AbortSignal.timeout(FETCH_TIMEOUT_MS),
    });

    if (!res.ok) return [];

    const json = await res.json();
    return extractRecords(json?.data ?? json);
  } catch {
    return [];
  }
}

async function fetchRecordsWithFallback(primary: string, fallback: string) {
  const primaryKeys = await fetchRecords(primary);
  if (primaryKeys.length > 0) return primaryKeys;
  return fetchRecords(fallback);
}

function buildUrlXml(
  path: string,
  priority: number,
  changeFrequency: ChangeFreq,
  lastmod?: string,
  images: string[] = [],
) {
  // hreflang only between two or more indexable locales (noindexed /en is excluded).
  const links =
    INDEXED_LOCALES.length > 1
      ? [
          ...INDEXED_LOCALES.map(
            (locale) =>
              `    <xhtml:link rel="alternate" hreflang="${locale}" href="${xmlEscape(absoluteUrl(locale, path))}" />`,
          ),
          `    <xhtml:link rel="alternate" hreflang="x-default" href="${xmlEscape(absoluteUrl(DEFAULT_LOCALE, path))}" />`,
        ].join('\n')
      : '';

  const imageXml = images
    .slice(0, 10)
    .map(
      (img) => `  <image:image>
    <image:loc>${xmlEscape(img)}</image:loc>
  </image:image>`,
    )
    .join('\n');

  return INDEXED_LOCALES.map((locale) => {
    const loc = absoluteUrl(locale, path);
    const lines = [
      `  <loc>${xmlEscape(loc)}</loc>`,
      links,
      // Omitted when unknown: a lastmod that is always "now" teaches Google to ignore it.
      lastmod ? `  <lastmod>${xmlEscape(lastmod)}</lastmod>` : '',
    ].filter(Boolean);
    return `<url>
${lines.join('\n')}
  <changefreq>${changeFrequency}</changefreq>
  <priority>${priority.toFixed(1)}</priority>
${imageXml}
</url>`;
  }).join('\n');
}

export async function GET() {
  // Supplier profiles are noindex (thin, include service accounts), so they are not listed.
  const [products, blogs, brands, categories, tags, discounts] =
    await Promise.all([
      fetchRecords('products/getslugs'),
      fetchRecords('blogs/getslugs'),
      fetchRecordsWithFallback('brands/getslugs', 'brands/getids'),
      fetchRecordsWithFallback('categories/getslugs', 'categories/getids'),
      fetchRecordsWithFallback('tags/getslugs', 'tags/getids'),
      fetchRecordsWithFallback('discounts/getslugs', 'discounts/active'),
    ]);

  const staticPaths: SitemapEntry[] = [
    { path: '', priority: 1.0, changeFrequency: 'weekly' },
    { path: 'products', priority: 0.9, changeFrequency: 'daily' },
    { path: 'blog', priority: 0.9, changeFrequency: 'weekly' },
    { path: 'brands', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'categories', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'tags', priority: 0.7, changeFrequency: 'weekly' },
    { path: 'exhibition', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'discounts', priority: 0.8, changeFrequency: 'daily' },
    { path: 'faq', priority: 0.6, changeFrequency: 'weekly' },
    { path: 'cooperation', priority: 0.5, changeFrequency: 'monthly' },
  ];

  const chunks = [
    ...staticPaths.map((item) =>
      buildUrlXml(item.path, item.priority, item.changeFrequency),
    ),
    ...products.map((item) =>
      buildUrlXml(
        `products/${item.key}`,
        0.7,
        'weekly',
        item.lastmod,
        item.images,
      ),
    ),
    ...blogs.map((item) =>
      buildUrlXml(
        `blog/${item.key}`,
        0.6,
        'monthly',
        item.lastmod,
        item.images,
      ),
    ),
    ...brands.map((item) =>
      buildUrlXml(
        `brands/${item.key}`,
        0.6,
        'monthly',
        item.lastmod,
        item.images,
      ),
    ),
    ...categories.map((item) =>
      buildUrlXml(
        `categories/${item.key}`,
        0.6,
        'monthly',
        item.lastmod,
        item.images,
      ),
    ),
    ...tags.map((item) =>
      buildUrlXml(
        `tags/${item.key}`,
        0.5,
        'monthly',
        item.lastmod,
      ),
    ),
    // Hard-coded exhibition photo sheets.
    ...['lab-tala', 'carameli', 'icy', 'smoky', 'saria', 'all'].map((slug) =>
      buildUrlXml(`exhibition/${slug}`, 0.7, 'weekly'),
    ),
    ...discounts.map((item) =>
      buildUrlXml(
        `discounts/${item.key}`,
        0.6,
        'weekly',
        item.lastmod,
      ),
    ),
  ];

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml" xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
${chunks.join('\n')}
</urlset>`;

  // Don't let a CDN/proxy cache a sitemap built while the backend was down.
  const backendOk = products.length > 0 || blogs.length > 0;

  return new Response(xml, {
    status: 200,
    headers: {
      'Content-Type': 'application/xml; charset=utf-8',
      'Cache-Control': backendOk
        ? 'public, s-maxage=3600, stale-while-revalidate=86400'
        : 'no-store',
    },
  });
}
