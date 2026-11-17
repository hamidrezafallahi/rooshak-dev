import { getTranslations } from 'next-intl/server';

import { getLandingProductsByTabs } from '@lib/landing';
import { ILandingProduct } from '@models/product';

import FeaturedProducts from './FeaturedProducts';
import { IFeaturedProduct } from './type';

export { default as ProductCard } from './ProductCard';
export { default as FeaturedProducts } from './FeaturedProducts';
export type { IFeaturedProduct, IProductCardProps } from './type';

function toPlainText(html: string | null | undefined) {
  return (html ?? '')
    .replace(/<[^>]*>/g, ' ')
    .replace(/&nbsp;/g, ' ')
    .replace(/\s+/g, ' ')
    .trim();
}

function formatMoney(value: number, locale: string) {
  return new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(value);
}

function toFeatured(p: ILandingProduct, locale: string, currency: string): IFeaturedProduct {
  const final = p.finalPrice && p.finalPrice > 0 ? p.finalPrice : p.price;
  const category = locale === 'fa' ? p.persianCategoryName : p.englishCategoryName;
  return {
    id: p.id,
    name: p.name,
    description: toPlainText(p.description),
    image: p.mainImage,
    imageAlt: [p.name, category].filter(Boolean).join(' - '),
    priceLabel: final && final > 0 ? `${formatMoney(final, locale)} ${currency}` : null,
    badges: [category, p.brand].filter((b): b is string => Boolean(b && b.trim())),
    href: `/${locale}/products/${p.slug || p.id}`,
  };
}

/**
 * Landing section: picks three products from the landing API
 * (newest first, topped up from best sellers / discounted) and renders them.
 */
export default async function LandingFeaturedProducts({ locale }: { locale: string }) {
  const [t, tCommon, { theNewest, bestSeller, discounters }] = await Promise.all([
    getTranslations({ locale, namespace: 'landing' }),
    getTranslations({ locale, namespace: 'common' }),
    getLandingProductsByTabs(),
  ]);

  const picked: ILandingProduct[] = [];
  for (const p of [...theNewest, ...bestSeller, ...discounters]) {
    if (picked.length === 3) break;
    if (p?.mainImage && !picked.some((x) => x.id === p.id)) picked.push(p);
  }

  return (
    <FeaturedProducts
      heading={t('featuredProducts')}
      ctaLabel={t('viewProduct')}
      products={picked.map((p) => toFeatured(p, locale, tCommon('currency')))}
    />
  );
}
