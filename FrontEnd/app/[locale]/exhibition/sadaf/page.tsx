import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import ExhibitionPhoto from '@components/organisms/exhibition/ExhibitionPhoto';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export const dynamic = 'force-static';
export const revalidate = false;

type SadafPhoto = {
  src: string;
  width: number;
  height: number;
};

/**
 * Sadaf product photos exactly as they sit on disk under
 * /public/exhibition/sadaf (width/height match the files).
 */
const SADAF_PHOTOS: SadafPhoto[] = [
  { src: '/exhibition/sadaf/1000046087.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046088.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046089.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046090.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046091.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046092.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046093.webp', width: 960, height: 1280 },
  { src: '/exhibition/sadaf/1000046094.webp', width: 960, height: 1280 },
];

/**
 * Price-list flyer closing the roll. Set to the image under
 * /public/exhibition/sadaf once it is added (null = not shown yet).
 */
const SADAF_PRICE_LIST: SadafPhoto | null = null;

/**
 * Full Sadaf roll: every product photo in order, then the
 * price-list flyer at the very end.
 */
function sadafExhibitionPhotos() {
  const productPhotos = SADAF_PHOTOS.map((photo, index) => ({
    ...photo,
    alt: `صدف — عکس ${index + 1}`,
  }));

  return SADAF_PRICE_LIST
    ? [...productPhotos, { ...SADAF_PRICE_LIST, alt: 'لیست قیمت صدف' }]
    : productPhotos;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const photos = sadafExhibitionPhotos();

  return buildPageMetadata({
    locale,
    path: 'exhibition/sadaf',
    title: locale === 'fa' ? 'لیست قیمت صدف' : 'Sadaf price lists',
    description: t('indexDescription'),
    images: photos.slice(0, 3).map((p) => p.src),
  });
}

/**
 * One continuous sheet: every Sadaf product photo in order,
 * then the price-list image. Layout is fluid via .exhibit-sheet /
 * .exhibit-photo (full width on phones, capped at 1200px on desktop).
 */
export default async function Page({ params }: Props) {
  const { locale } = await params;
  const photos = sadafExhibitionPhotos();
  const title = locale === 'fa' ? 'لیست قیمت صدف' : 'Sadaf price lists';

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
