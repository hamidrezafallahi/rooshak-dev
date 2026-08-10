import { serverApiBaseUrl, siteBaseUrl } from '@lib/api';
import { absoluteUrl } from '@lib/seo';
import { toMediaUrl } from '@utils/toMediaUrl';

export const revalidate = 3600;

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

type MerchantProduct = {
  id?: number;
  slug?: string;
  name?: string;
  description?: string;
  brandName?: string;
  finalPrice?: number | null;
  price?: number | null;
  inStock?: boolean;
  currency?: string;
  imageUrls?: string[];
  mainImage?: string;
};

async function fetchProducts(): Promise<MerchantProduct[]> {
  try {
    const res = await fetch(`${serverApiBaseUrl}/Products?page=1&pageSize=500&OnlyActives=true`, {
      next: { revalidate },
      signal: AbortSignal.timeout(4000),
    });
    if (!res.ok) return [];
    const json = await res.json();
    const records = json?.data?.records ?? json?.data ?? [];
    return Array.isArray(records) ? records : [];
  } catch {
    return [];
  }
}

export async function GET() {
  const products = await fetchProducts();
  const locale = 'fa';

  const items = products
    .map((product) => {
      const slug = product.slug || String(product.id || '');
      if (!slug || !product.name) return '';
      const link = absoluteUrl(locale, `products/${slug}`);
      const image =
        toAbsoluteImage(product.imageUrls?.[0] || product.mainImage) ||
        `${siteBaseUrl}/og-image.jpg`;
      const price = Number(product.finalPrice ?? product.price ?? 0);
      const availability = product.inStock === false ? 'out_of_stock' : 'in_stock';
      const description = (product.description || product.name).slice(0, 5000);

      return `<item>
  <g:id>${xmlEscape(String(product.id || slug))}</g:id>
  <g:title>${xmlEscape(product.name)}</g:title>
  <g:description>${xmlEscape(description)}</g:description>
  <g:link>${xmlEscape(link)}</g:link>
  <g:image_link>${xmlEscape(image)}</g:image_link>
  <g:availability>${availability}</g:availability>
  <g:price>${price.toFixed(0)} IRR</g:price>
  <g:brand>${xmlEscape(product.brandName || 'Rooshak')}</g:brand>
  <g:condition>new</g:condition>
</item>`;
    })
    .filter(Boolean)
    .join('\n');

  const xml = `<?xml version="1.0" encoding="UTF-8"?>
<rss version="2.0" xmlns:g="http://base.google.com/ns/1.0">
  <channel>
    <title>Rooshak Merchant Feed</title>
    <link>${xmlEscape(siteBaseUrl)}</link>
    <description>Google Merchant product feed</description>
${items}
  </channel>
</rss>`;

  return new Response(xml, {
    status: 200,
    headers: {
      'Content-Type': 'application/xml; charset=utf-8',
      'Cache-Control': 'public, s-maxage=3600, stale-while-revalidate=86400',
    },
  });
}
