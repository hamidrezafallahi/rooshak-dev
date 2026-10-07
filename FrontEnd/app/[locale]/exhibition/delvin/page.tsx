import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import ExhibitionPhoto from '@components/organisms/exhibition/ExhibitionPhoto';
import { EXHIBITION_INTRO_PHOTO } from '@lib/exhibitionCatalogs';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export const dynamic = 'force-static';
export const revalidate = false;

type DelvinPhoto = {
  src: string;
  width: number;
  height: number;
};

/**
 * Delvin product photos exactly as they sit on disk under
 * /public/exhibition/delvin (width/height match the files).
 */
const DELVIN_PHOTOS: DelvinPhoto[] = [
  { src: '/exhibition/delvin/IMG_20261005_120111_101.webp', width: 960, height: 1280 },
  { src: '/exhibition/delvin/IMG_20261005_120111_293.webp', width: 1195, height: 896 },
  { src: '/exhibition/delvin/IMG_20261005_120111_359.webp', width: 1195, height: 896 },
];

/** A4-ratio price-list flyer closing the roll. */
const DELVIN_PRICE_LIST = {
  src: '/exhibition/delvin/price%20list.webp',
  width: 707,
  height: 1000,
};

/**
 * Full Delvin roll: every product photo in order, then the
 * price-list flyer at the very end.
 */
function delvinExhibitionPhotos() {
  return [
    EXHIBITION_INTRO_PHOTO,
    ...DELVIN_PHOTOS.map((photo, index) => ({
      ...photo,
      alt: `دلوین — عکس ${index + 1}`,
    })),
    { ...DELVIN_PRICE_LIST, alt: 'لیست قیمت دلوین' },
  ];
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const photos = delvinExhibitionPhotos();

  return buildPageMetadata({
    locale,
    path: 'exhibition/delvin',
    title: locale === 'fa' ? 'لیست قیمت دلوین' : 'Delvin price lists',
    description: t('indexDescription'),
    images: photos.slice(0, 3).map((p) => p.src),
  });
}

/**
 * One continuous sheet: the product photos, then the price-list image last.
 */
export default async function Page({ params }: Props) {
  const { locale } = await params;
  const photos = delvinExhibitionPhotos();
  const title = locale === 'fa' ? 'لیست قیمت دلوین' : 'Delvin price lists';

  return (
    <main className="exhibit-sheet" aria-label={title}>
      <h1 className="sr-only">{title}</h1>
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
