import type { Metadata } from 'next';
import Link from 'next/link';
import { unstable_noStore as noStore } from 'next/cache';
import { notFound, permanentRedirect } from 'next/navigation';

import { getTranslations } from 'next-intl/server';

import PriceListCard from '@components/organisms/exhibition/PriceListCard';
import SharePanel from '@components/organisms/exhibition/SharePanel';
import EmptyState from '@components/molecules/storefront/EmptyState';
import JsonLd from '@components/molecules/storefront/JsonLd';
import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import { serverApiBaseUrl } from '@lib/api';
import { renderQrSvg } from '@lib/qr';
import { safeFetchJson } from '@lib/safeFetch';
import { absoluteUrl, buildPageMetadata } from '@lib/seo';
import { fetchStaticSlugParams } from '@lib/staticParams';
import { SimpleResponse } from '@models/base';
import { IPriceList } from '@models/exhibition';

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};

export const revalidate = 300;

export async function generateStaticParams() {
  return fetchStaticSlugParams('Tags/getslugs', 'Tags/getids');
}

async function fetchPriceList(slug: string): Promise<IPriceList | null> {
  const result = await safeFetchJson<SimpleResponse<IPriceList>>(
    `${serverApiBaseUrl}/Tags/${encodeURIComponent(slug)}/pricelist`,
    { next: { revalidate: 300 } },
  );

  if (!result.ok || !result.data?.data || result.data.isSuccess === false) {
    noStore();
    return null;
  }
  return result.data.data;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const priceList = await fetchPriceList(slug);

  if (!priceList) {
    return buildPageMetadata({
      locale,
      path: `exhibition/${slug}`,
      title: tStore('notFound'),
      description: tStore('notFoundHint'),
      noIndex: true,
    });
  }

  const canonical = priceList.tagSlug || String(priceList.tagId);

  return buildPageMetadata({
    locale,
    path: `exhibition/${canonical}`,
    title: t('metaTitle', { family: priceList.tagName }),
    description: t('metaDescription', {
      family: priceList.tagName,
      count: priceList.itemCount,
    }),
    images: priceList.items.slice(0, 3).map((item) => item.mainImage),
  });
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const priceList = await fetchPriceList(slug);

  if (!priceList) {
    notFound();
  }

  if (priceList.tagSlug && priceList.tagSlug !== slug && /^\d+$/.test(slug)) {
    permanentRedirect(`/${locale}/exhibition/${priceList.tagSlug}`);
  }

  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const canonical = priceList.tagSlug || String(priceList.tagId);
  const shareUrl = absoluteUrl(locale, `exhibition/${canonical}`);
  const qrSvg = await renderQrSvg(shareUrl);

  const updatedAt = new Intl.DateTimeFormat(
    locale === 'fa' ? 'fa-IR-u-ca-persian' : 'en-US',
    { year: 'numeric', month: 'long', day: 'numeric' },
  ).format(new Date(priceList.updatedAt));

  const collectionLd = {
    '@context': 'https://schema.org',
    '@type': 'OfferCatalog',
    name: t('metaTitle', { family: priceList.tagName }),
    url: shareUrl,
    inLanguage: locale,
    numberOfItems: priceList.itemCount,
    itemListElement: priceList.items.map((item, index) => ({
      '@type': 'Offer',
      position: index + 1,
      sku: item.code,
      price: item.finalPrice ?? item.price ?? undefined,
      priceCurrency: priceList.currency,
      availability: item.inStock
        ? 'https://schema.org/InStock'
        : 'https://schema.org/OutOfStock',
      itemOffered: {
        '@type': 'Product',
        name: item.name,
        url: absoluteUrl(locale, `products/${item.slug || item.productId}`),
      },
    })),
  };

  return (
    <article className="store-page exhibit-page !pt-6">
      <JsonLd data={collectionLd} />

      <div className="print:hidden">
        <StoreBreadcrumbs
          locale={locale}
          items={[
            { name: locale === 'fa' ? 'خانه' : 'Home', path: '' },
            { name: t('indexTitle'), path: 'exhibition' },
            { name: priceList.tagName },
          ]}
        />
      </div>

      <header className="exhibit-hero">
        <p className="exhibit-hero-eyebrow">{t('eyebrow')}</p>
        <h1 className="exhibit-hero-title">{priceList.tagName}</h1>
        <p className="exhibit-hero-desc">{t('heroDesc')}</p>
        <div className="exhibit-hero-meta">
          <span>{t('itemCount', { count: priceList.itemCount })}</span>
          <span aria-hidden>•</span>
          <span>{t('updatedAt', { date: updatedAt })}</span>
        </div>
      </header>

      {priceList.items.length === 0 ? (
        <EmptyState
          title={t('empty')}
          description={t('emptyHint')}
          action={
            <Link href={`/${locale}/exhibition`} className="store-btn store-btn-primary">
              {t('backToIndex')}
            </Link>
          }
        />
      ) : (
        <section className="exhibit-grid" aria-label={priceList.tagName}>
          {priceList.items.map((item) => (
            <PriceListCard
              key={item.productId}
              item={item}
              locale={locale}
              currency={priceList.currency}
            />
          ))}
        </section>
      )}

      <SharePanel
        url={shareUrl}
        title={priceList.tagName}
        qrSvg={qrSvg}
        locale={locale}
      />

      <p className="exhibit-disclaimer">{t('disclaimer')}</p>
    </article>
  );
}
