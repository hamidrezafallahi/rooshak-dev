import type { Metadata } from 'next';
import Link from 'next/link';

import { getTranslations } from 'next-intl/server';

import MediaImage from '@components/atoms/MediaImage';
import EmptyState from '@components/molecules/storefront/EmptyState';
import EntityGrid from '@components/molecules/storefront/EntityGrid';
import JsonLd from '@components/molecules/storefront/JsonLd';
import PageHeader from '@components/molecules/storefront/PageHeader';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { absoluteUrl, buildPageMetadata } from '@lib/seo';
import { SimpleResponse } from '@models/base';
import { ITagFamily } from '@models/exhibition';

type Props = {
  params: Promise<{ locale: string }>;
};

export const revalidate = 300;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });

  return buildPageMetadata({
    locale,
    path: 'exhibition',
    title: t('indexTitle'),
    description: t('indexDescription'),
  });
}

async function fetchFamilies(): Promise<ITagFamily[] | null> {
  const result = await safeFetchJson<SimpleResponse<ITagFamily[]>>(
    `${serverApiBaseUrl}/Tags/families`,
    { next: { revalidate: 300 } },
  );

  if (!result.ok || !result.data) return null;
  if (result.data.isSuccess === false) return null;
  return result.data.data ?? [];
}

export default async function Page({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const tStore = await getTranslations({ locale, namespace: 'store' });

  const families = await fetchFamilies();
  const loadFailed = families === null;
  const list = families ?? [];

  const collectionLd = {
    '@context': 'https://schema.org',
    '@type': 'CollectionPage',
    name: t('indexTitle'),
    description: t('indexDescription'),
    url: absoluteUrl(locale, 'exhibition'),
    inLanguage: locale,
  };

  return (
    <article className="store-page !pt-6">
      <JsonLd data={collectionLd} />
      <PageHeader
        title={t('indexTitle')}
        description={t('indexDescription')}
        eyebrow={t('eyebrow')}
      />

      {loadFailed && (
        <div
          className="store-panel px-4 py-3 border-[color-mix(in_srgb,var(--error-color)_35%,transparent)] text-[var(--error-color)]"
          role="alert"
        >
          <p className="font-medium">{tStore('error')}</p>
          <p>{tStore('fetchError')}</p>
        </div>
      )}

      {list.length === 0 ? (
        <EmptyState
          title={loadFailed ? tStore('loadError') : t('indexEmpty')}
          description={loadFailed ? tStore('serverError') : t('indexEmptyHint')}
          action={
            <Link href={`/${locale}/products`} className="store-btn store-btn-primary">
              {t('browseProducts')}
            </Link>
          }
        />
      ) : (
        <EntityGrid cols="cards">
          {list.map((family) => (
            <Link
              key={family.id}
              href={`/${locale}/exhibition/${family.slug || family.id}`}
              className="group exhibit-family-card"
            >
              <div className="exhibit-family-media">
                {family.coverImage ? (
                  <MediaImage
                    src={family.coverImage}
                    alt={family.name}
                    fill
                    className="object-cover transition-transform duration-500 group-hover:scale-105"
                    sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 300px"
                  />
                ) : (
                  <span className="exhibit-card-placeholder" aria-hidden>
                    {family.name}
                  </span>
                )}
              </div>
              <div className="exhibit-family-body">
                <h2 className="exhibit-family-name">{family.name}</h2>
                <p className="exhibit-family-count">
                  {t('itemCount', { count: family.productCount })}
                </p>
                <span className="exhibit-family-cta">{t('viewPriceList')}</span>
              </div>
            </Link>
          ))}
        </EntityGrid>
      )}
    </article>
  );
}
