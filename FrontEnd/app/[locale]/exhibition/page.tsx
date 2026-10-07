import type { Metadata } from 'next';
import Link from 'next/link';

import { getTranslations } from 'next-intl/server';

import ExhibitionCover from '@components/organisms/exhibition/ExhibitionCover';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

const EXHIBITION_FAMILIES = [
  {
    slug: 'all',
    nameFa: 'رویال',
    nameEn: 'Royal',
    coverImage: '/exhibition/erico-intro.webp',
    coverWidth: 689,
    coverHeight: 1000,
  },
  {
    slug: 'delvin',
    nameFa: 'دلوین',
    nameEn: 'Delvin',
    coverImage: '/exhibition/delvin/product-1.webp',
    coverWidth: 819,
    coverHeight: 1024,
  },
  {
    slug: 'leona',
    nameFa: 'لیونا',
    nameEn: 'Leona',
    coverImage: '/exhibition/leona/cream/photo_2026-10-01_10-20-57.webp',
    coverWidth: 819,
    coverHeight: 1024,
  },
  {
    slug: 'sadaf',
    nameFa: 'صدف',
    nameEn: 'Sadaf',
    coverImage: '/exhibition/sadaf/1000046087.webp',
    coverWidth: 819,
    coverHeight: 1024,
  },
];

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
        {EXHIBITION_FAMILIES.map((family) => {
          const name = locale === 'fa' ? family.nameFa : family.nameEn;
          return (
            <li key={family.slug}>
              <Link
                href={`/${locale}/exhibition/${family.slug}`}
                className="exhibit-index-card"
              >
                <ExhibitionCover
                  src={family.coverImage}
                  alt={name}
                  width={family.coverWidth}
                  height={family.coverHeight}
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
