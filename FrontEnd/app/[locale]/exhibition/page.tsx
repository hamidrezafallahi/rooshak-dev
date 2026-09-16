import type { Metadata } from 'next';
import Link from 'next/link';

import { getTranslations } from 'next-intl/server';

import ExhibitionCover from '@components/organisms/exhibition/ExhibitionCover';
import {
  EXHIBITION_CATALOGS,
  exhibitionCatalogName,
} from '@lib/exhibitionCatalogs';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export const dynamic = 'force-static';
export const revalidate = false;

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

export default async function Page({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });

  return (
    <main className="exhibit-index" aria-label={t('indexTitle')}>
      <h1 className="sr-only">{t('indexTitle')}</h1>
      <ul className="exhibit-index-list">
        {EXHIBITION_CATALOGS.map((catalog) => {
          const name = exhibitionCatalogName(catalog, locale);
          return (
            <li key={catalog.slug}>
              <Link
                href={`/${locale}/exhibition/${catalog.slug}`}
                className="exhibit-index-card"
              >
                <ExhibitionCover
                  src={catalog.coverImage}
                  alt={name}
                  width={catalog.coverWidth}
                  height={catalog.coverHeight}
                />
                <span className="exhibit-index-name">{name}</span>
              </Link>
            </li>
          );
        })}
      </ul>
    </main>
  );
}
