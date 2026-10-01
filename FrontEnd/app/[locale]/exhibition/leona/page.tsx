import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import ExhibitionPhoto from '@components/organisms/exhibition/ExhibitionPhoto';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export const dynamic = 'force-static';
export const revalidate = false;

type LeonaPhoto = {
  src: string;
  width: number;
  height: number;
};

type LeonaFamily = {
  nameFa: string;
  nameEn: string;
  photos: LeonaPhoto[];
};

/**
 * Leona families — product photos exactly as they sit on disk under
 * /public/exhibition/leona/<folder> (width/height match the files).
 * Their display order is set by FAMILY_ORDER below.
 */
const LEONA_FAMILIES: LeonaFamily[] = [
  {
    nameFa: 'ساده',
    nameEn: 'Simple',
    photos: [
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-24.webp', width: 986, height: 986 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-26.webp', width: 994, height: 994 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-27.webp', width: 986, height: 986 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-28.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-29%20(2).webp', width: 992, height: 992 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-29.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-30.webp', width: 978, height: 978 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-31.webp', width: 976, height: 976 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-32.webp', width: 998, height: 998 },
      { src: '/exhibition/leona/simple/photo_2026-10-01_10-23-33.webp', width: 984, height: 984 },
    ],
  },
  {
    nameFa: 'فیروزه‌ای',
    nameEn: 'Turquoise',
    photos: [
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-25.webp', width: 986, height: 986 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-27.webp', width: 992, height: 992 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-28.webp', width: 984, height: 984 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-29.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-30.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-31.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-32.webp', width: 984, height: 984 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-33.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-34.webp', width: 980, height: 980 },
      { src: '/exhibition/leona/firuzei/photo_2026-10-01_10-22-35.webp', width: 990, height: 990 },
    ],
  },
  {
    nameFa: 'قرمز',
    nameEn: 'Red',
    photos: [
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-49.webp', width: 988, height: 988 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-50.webp', width: 998, height: 998 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-51.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-52%20(2).webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-52.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-53.webp', width: 984, height: 984 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-54%20(2).webp', width: 988, height: 988 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-54.webp', width: 984, height: 984 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-55.webp', width: 992, height: 992 },
      { src: '/exhibition/leona/red/photo_2026-10-01_10-24-56.webp', width: 994, height: 994 },
    ],
  },
  {
    nameFa: 'کاراملی',
    nameEn: 'Caramel',
    photos: [
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-13.webp', width: 986, height: 986 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-14.webp', width: 998, height: 998 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-15.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-16.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-17.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-18.webp', width: 988, height: 988 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-19.webp', width: 992, height: 992 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-20.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-21%20(2).webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/karamely/photo_2026-10-01_10-24-21.webp', width: 992, height: 992 },
    ],
  },
  {
    nameFa: 'کرم',
    nameEn: 'Cream',
    photos: [
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-20-57.webp', width: 990, height: 990 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-01.webp', width: 990, height: 990 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-02.webp', width: 982, height: 982 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-04.webp', width: 988, height: 988 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-05.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-06.webp', width: 976, height: 976 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-08.webp', width: 980, height: 980 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-09.webp', width: 998, height: 998 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-10.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/cream/photo_2026-10-01_10-21-11.webp', width: 996, height: 996 },
    ],
  },
  {
    nameFa: 'سرمه‌ای',
    nameEn: 'Navy',
    photos: [
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-17%20(2).webp', width: 994, height: 994 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-17.webp', width: 994, height: 994 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-18.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-19.webp', width: 986, height: 990 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-20.webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-21.webp', width: 992, height: 998 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-22%20(2).webp', width: 1000, height: 1000 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-22.webp', width: 990, height: 990 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-23.webp', width: 992, height: 992 },
      { src: '/exhibition/leona/sormei/photo_2026-10-01_10-25-24.webp', width: 988, height: 988 },
    ],
  },
];

/**
 * Display order on the sheet: سرمه‌ای and کاراملی on top (above the rest),
 * then فیروزه‌ای، قرمز، کرم و ساده — the price-list flyer still closes the roll.
 */
const FAMILY_ORDER = ['سرمه‌ای', 'کاراملی', 'فیروزه‌ای', 'قرمز', 'کرم', 'ساده'];

const ORDERED_LEONA_FAMILIES = FAMILY_ORDER.flatMap((name) =>
  LEONA_FAMILIES.filter((family) => family.nameFa === name),
);

/** A4-ratio flyer sitting at the root of /public/exhibition/leona. */
const LEONA_PRICE_LIST = {
  src: '/exhibition/leona/photo_2026-10-01_10-25-54.webp',
  width: 707,
  height: 1000,
};

/**
 * Full Leona roll: every family's product photos in order, then the
 * price-list flyer at the very end.
 */
function leonaExhibitionPhotos() {
  const familyPhotos = ORDERED_LEONA_FAMILIES.flatMap((family) =>
    family.photos.map((photo, index) => ({
      ...photo,
      alt: `${family.nameFa} لیونا — عکس ${index + 1}`,
    })),
  );

  return [
    ...familyPhotos,
    { ...LEONA_PRICE_LIST, alt: 'لیست قیمت لیونا' },
  ];
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'exhibition' });
  const photos = leonaExhibitionPhotos();

  return buildPageMetadata({
    locale,
    path: 'exhibition/leona',
    title: locale === 'fa' ? 'لیست قیمت لیونا' : 'Leona price lists',
    description: t('indexDescription'),
    images: photos.slice(0, 3).map((p) => p.src),
  });
}

/**
 * One continuous sheet: every family's product photos, in FAMILY_ORDER
 * (سرمه‌ای، کاراملی، فیروزه‌ای، قرمز، کرم، ساده), then the price-list image.
 */
export default async function Page({ params }: Props) {
  const { locale } = await params;
  const photos = leonaExhibitionPhotos();
  const title = locale === 'fa' ? 'لیست قیمت لیونا' : 'Leona price lists';

  return (
    <main className="exhibit-sheet" aria-label={title}>
      <h1 className="sr-only">
        {title}
        {' — '}
        {ORDERED_LEONA_FAMILIES.map((family) =>
          locale === 'fa' ? family.nameFa : family.nameEn,
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
