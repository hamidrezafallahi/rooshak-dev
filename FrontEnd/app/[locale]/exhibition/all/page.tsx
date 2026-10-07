import type { Metadata } from 'next';

import { getTranslations } from 'next-intl/server';

import ExhibitionPhoto from '@components/organisms/exhibition/ExhibitionPhoto';
import {
  allExhibitionPhotos,
  EXHIBITION_CATALOGS,
  EXHIBITION_INTRO_PHOTO,
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
  const photos = [EXHIBITION_INTRO_PHOTO, ...allExhibitionPhotos()];

  return buildPageMetadata({
    locale,
    path: 'exhibition/all',
    title: locale === 'fa' ? 'لیست قیمت همه خانواده‌ها' : 'All family price lists',
    description: t('indexDescription'),
    images: photos.slice(0, 3).map((p) => p.src),
  });
}

/**
 * One continuous sheet: every family's 6 product photos, then that family's
 * price-list image, then the next family — through the end of the catalog.
 */
export default async function Page({ params }: Props) {
  const { locale } = await params;
  const photos = [EXHIBITION_INTRO_PHOTO, ...allExhibitionPhotos()];
  const title =
    locale === 'fa' ? 'لیست قیمت همه خانواده‌ها' : 'All family price lists';

  return (
    <main className="exhibit-sheet" aria-label={title}>
      <h1 className="sr-only">
        {title}
        {' — '}
        {EXHIBITION_CATALOGS.map((c) =>
          locale === 'fa' ? c.nameFa : c.nameEn,
        ).join(locale === 'fa' ? '، ' : ', ')}
      </h1>
      {photos.map((photo, index) => (
        <ExhibitionPhoto
          key={`${photo.src}-${index}`}
          src={photo.src}
          alt={photo.alt}
          width={photo.width}
          height={photo.height}
          priority={index === 0}
        />
      ))}
    </main>
  );
}
