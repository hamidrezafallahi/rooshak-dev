/**
 * Hard-coded exhibition price-list sheets (SSG).
 * Product photos first, price-list flyer last.
 * Images are pre-compressed WebP; width/height match the files on disk.
 */

export type ExhibitionPhoto = {
  src: string;
  alt: string;
  width: number;
  height: number;
};

export type ExhibitionCatalog = {
  slug: string;
  /** Older URLs that should land on this sheet. */
  aliases: string[];
  nameFa: string;
  nameEn: string;
  coverImage: string;
  coverWidth: number;
  coverHeight: number;
  photos: ExhibitionPhoto[];
};

export const EXHIBITION_CATALOGS: ExhibitionCatalog[] = [
  {
    slug: 'lab-tala',
    aliases: ['sunshine-11', 'sunshine', 'aftab', '11', 'leb-tala'],
    nameFa: 'لب طلا',
    nameEn: 'Lab Tala',
    coverImage: '/exhibition/lab-tala/01-fruit-bowl.webp',
    coverWidth: 819,
    coverHeight: 1024,
    photos: [
      { src: '/exhibition/lab-tala/01-fruit-bowl.webp', alt: 'کاسه میوه لب طلا', width: 819, height: 1024 },
      { src: '/exhibition/lab-tala/02-chocolate.webp', alt: 'شکلات‌خوری لب طلا', width: 819, height: 1024 },
      { src: '/exhibition/lab-tala/03-nut-bowl.webp', alt: 'ظرف آجیل لب طلا', width: 819, height: 1024 },
      { src: '/exhibition/lab-tala/04-single-tier.webp', alt: 'شیرینی تک‌طبقه لب طلا', width: 960, height: 738 },
      { src: '/exhibition/lab-tala/05-two-tier.webp', alt: 'شیرینی دوطبقه لب طلا', width: 680, height: 1024 },
      { src: '/exhibition/lab-tala/06-small-bowl.webp', alt: 'پیاله لب طلا', width: 841, height: 1024 },
      { src: '/exhibition/lab-tala/99-price-list.webp', alt: 'لیست قیمت لب طلا', width: 732, height: 1024 },
    ],
  },
  {
    slug: 'carameli',
    aliases: ['caramel', 'caramely', 'caramelli'],
    nameFa: 'کاراملی',
    nameEn: 'Carameli',
    coverImage: '/exhibition/carameli/01-fruit-bowl.webp',
    coverWidth: 771,
    coverHeight: 1024,
    photos: [
      { src: '/exhibition/carameli/01-fruit-bowl.webp', alt: 'کاسه میوه کاراملی', width: 771, height: 1024 },
      { src: '/exhibition/carameli/02-chocolate.webp', alt: 'شکلات‌خوری کاراملی', width: 771, height: 1024 },
      { src: '/exhibition/carameli/03-nut-bowl.webp', alt: 'ظرف آجیل کاراملی', width: 771, height: 1024 },
      { src: '/exhibition/carameli/04-single-tier.webp', alt: 'شیرینی تک‌طبقه کاراملی', width: 771, height: 1024 },
      { src: '/exhibition/carameli/05-two-tier.webp', alt: 'شیرینی دوطبقه کاراملی', width: 771, height: 1024 },
      { src: '/exhibition/carameli/06-small-bowl.webp', alt: 'پیاله کاراملی', width: 768, height: 1024 },
      { src: '/exhibition/carameli/99-price-list.webp', alt: 'لیست قیمت کاراملی', width: 567, height: 850 },
    ],
  },
  {
    slug: 'icy',
    aliases: ['ice', 'yakh', 'yakhī', 'yakhi'],
    nameFa: 'یخی',
    nameEn: 'Icy',
    coverImage: '/exhibition/icy/01-fruit-bowl.webp',
    coverWidth: 960,
    coverHeight: 872,
    photos: [
      { src: '/exhibition/icy/01-fruit-bowl.webp', alt: 'کاسه میوه یخی', width: 960, height: 872 },
      { src: '/exhibition/icy/02-chocolate.webp', alt: 'شکلات‌خوری یخی', width: 768, height: 1024 },
      { src: '/exhibition/icy/03-nut-bowl.webp', alt: 'ظرف آجیل یخی', width: 960, height: 960 },
      { src: '/exhibition/icy/04-single-tier.webp', alt: 'شیرینی تک‌طبقه یخی', width: 960, height: 760 },
      { src: '/exhibition/icy/05-two-tier.webp', alt: 'شیرینی دوطبقه یخی', width: 940, height: 1024 },
      { src: '/exhibition/icy/06-small-bowl.webp', alt: 'پیاله یخی', width: 768, height: 1024 },
      { src: '/exhibition/icy/99-price-list.webp', alt: 'لیست قیمت یخی', width: 567, height: 850 },
    ],
  },
  {
    slug: 'smoky',
    aliases: ['doodi', 'dudi', 'smoke'],
    nameFa: 'دودی',
    nameEn: 'Smoky',
    coverImage: '/exhibition/smoky/01-fruit-bowl.webp',
    coverWidth: 960,
    coverHeight: 710,
    photos: [
      { src: '/exhibition/smoky/01-fruit-bowl.webp', alt: 'کاسه میوه دودی', width: 960, height: 710 },
      { src: '/exhibition/smoky/02-chocolate.webp', alt: 'شکلات‌خوری دودی', width: 827, height: 1024 },
      { src: '/exhibition/smoky/03-nut-bowl.webp', alt: 'ظرف آجیل دودی', width: 960, height: 756 },
      { src: '/exhibition/smoky/04-single-tier.webp', alt: 'شیرینی تک‌طبقه دودی', width: 960, height: 624 },
      { src: '/exhibition/smoky/05-two-tier.webp', alt: 'شیرینی دوطبقه دودی', width: 891, height: 1024 },
      { src: '/exhibition/smoky/06-small-bowl.webp', alt: 'پیاله دودی', width: 960, height: 738 },
      { src: '/exhibition/smoky/99-price-list.webp', alt: 'لیست قیمت دودی', width: 567, height: 850 },
    ],
  },
];

export function findExhibitionCatalog(slug: string): ExhibitionCatalog | null {
  const key = decodeURIComponent(slug).trim().toLowerCase();
  return (
    EXHIBITION_CATALOGS.find(
      (c) => c.slug === key || c.aliases.includes(key),
    ) ?? null
  );
}

export function exhibitionCatalogName(catalog: ExhibitionCatalog, locale: string) {
  return locale === 'fa' ? catalog.nameFa : catalog.nameEn;
}

export function exhibitionStaticSlugs() {
  return EXHIBITION_CATALOGS.map((c) => c.slug);
}
