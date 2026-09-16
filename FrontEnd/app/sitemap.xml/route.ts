import { serverApiBaseUrl, siteBaseUrl } from '@lib/api';
import { absoluteUrl, DEFAULT_LOCALE, LOCALES } from '@lib/seo';
import { toMediaUrl } from '@utils/toMediaUrl';

export const revalidate = 3600;

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
        lastmod: record.updatedAt || undefined,
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
  lastmod: string,
  images: string[] = [],
) {
  const links = LOCALES.map(
    (locale) =>
      `    <xhtml:link rel="alternate" hreflang="${locale}" href="${xmlEscape(absoluteUrl(locale, path))}" />`,
  ).join('\n');

  const imageXml = images
    .slice(0, 10)
    .map(
      (img) => `  <image:image>
    <image:loc>${xmlEscape(img)}</image:loc>
  </image:image>`,
    )
    .join('\n');

  return LOCALES.map((locale) => {
    const loc = absoluteUrl(locale, path);
    return `<url>
  <loc>${xmlEscape(loc)}</loc>
${links}
    <xhtml:link rel="alternate" hreflang="x-default" href="${xmlEscape(absoluteUrl(DEFAULT_LOCALE, path))}" />
  <lastmod>${xmlEscape(lastmod)}</lastmod>
  <changefreq>${changeFrequency}</changefreq>
  <priority>${priority.toFixed(1)}</priority>
${imageXml}
</url>`;
  }).join('\n');
}

export async function GET() {
  const fallbackLastmod = new Date().toISOString();

  const [products, blogs, brands, categories, suppliers, tags, discounts] =
    await Promise.all([
      fetchRecords('products/getslugs'),
      fetchRecords('blogs/getslugs'),
      fetchRecordsWithFallback('brands/getslugs', 'brands/getids'),
      fetchRecordsWithFallback('categories/getslugs', 'categories/getids'),
      fetchRecordsWithFallback('users/getslugs', 'productOffers/suppliersIds'),
      fetchRecordsWithFallback('tags/getslugs', 'tags/getids'),
      fetchRecordsWithFallback('discounts/getslugs', 'discounts/active'),
    ]);

  const staticPaths: SitemapEntry[] = [
    { path: '', priority: 1.0, changeFrequency: 'weekly' },
    { path: 'products', priority: 0.9, changeFrequency: 'daily' },
    { path: 'blog', priority: 0.9, changeFrequency: 'weekly' },
    { path: 'brands', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'categories', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'suppliers', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'tags', priority: 0.7, changeFrequency: 'weekly' },
    { path: 'exhibition', priority: 0.8, changeFrequency: 'weekly' },
    { path: 'discounts', priority: 0.8, changeFrequency: 'daily' },
  ];

  const chunks = [
    ...staticPaths.map((item) =>
      buildUrlXml(item.path, item.priority, item.changeFrequency, fallbackLastmod),
    ),
    ...products.map((item) =>
      buildUrlXml(
        `products/${item.key}`,
        0.7,
        'weekly',
        item.lastmod || fallbackLastmod,
        item.images,
      ),
    ),
    ...blogs.map((item) =>
      buildUrlXml(
        `blog/${item.key}`,
        0.6,
        'monthly',
        item.lastmod || fallbackLastmod,
        item.images,
      ),
    ),
    ...brands.map((item) =>
      buildUrlXml(
        `brands/${item.key}`,
        0.6,
        'monthly',
        item.lastmod || fallbackLastmod,
        item.images,
      ),
    ),
    ...categories.map((item) =>
      buildUrlXml(
        `categories/${item.key}`,
        0.6,
        'monthly',
        item.lastmod || fallbackLastmod,
        item.images,
      ),
    ),
    ...suppliers.map((item) =>
      buildUrlXml(
        `suppliers/${item.key}`,
        0.6,
        'monthly',
        item.lastmod || fallbackLastmod,
        item.images,
      ),
    ),
    ...tags.map((item) =>
      buildUrlXml(
        `tags/${item.key}`,
        0.5,
        'monthly',
        item.lastmod || fallbackLastmod,
      ),
    ),
    // Hard-coded exhibition photo sheets.
    ...['lab-tala', 'carameli', 'icy', 'smoky', 'saria'].map((slug) =>
      buildUrlXml(`exhibition/${slug}`, 0.7, 'weekly', fallbackLastmod),
    ),
    ...discounts.map((item) =>
      buildUrlXml(
        `discounts/${item.key}`,
        0.6,
        'weekly',
        item.lastmod || fallbackLastmod,
      ),
    ),
  ];

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9" xmlns:xhtml="http://www.w3.org/1999/xhtml" xmlns:image="http://www.google.com/schemas/sitemap-image/1.1">
${chunks.join('\n')}
</urlset>`;

  return new Response(xml, {
    status: 200,
    headers: {
      'Content-Type': 'application/xml; charset=utf-8',
      'Cache-Control': 'public, s-maxage=3600, stale-while-revalidate=86400',
    },
  });
}
