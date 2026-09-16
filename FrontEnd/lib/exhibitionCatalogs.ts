/**
 * Hard-coded exhibition price-list sheets.
 * Each sheet is a vertical stack of large product photos ending with the price-list flyer.
 * Add more families here until the admin/API catalog is ready.
 */

export type ExhibitionPhoto = {
  src: string;
  alt: string;
};

export type ExhibitionCatalog = {
  slug: string;
  /** Older URLs that should land on this sheet. */
  aliases: string[];
  nameFa: string;
  nameEn: string;
  /** Cover for the exhibition index card. */
  coverImage: string;
  /** Product photos first, price-list flyer last. */
  photos: ExhibitionPhoto[];
};

export const EXHIBITION_CATALOGS: ExhibitionCatalog[] = [
  {
    slug: 'lab-tala',
    aliases: ['sunshine-11', 'sunshine', 'aftab', '11', 'leb-tala'],
    nameFa: 'لب طلا',
    nameEn: 'Lab Tala',
    coverImage: '/exhibition/lab-tala/01-fruit-bowl.jpg',
    photos: [
      { src: '/exhibition/lab-tala/01-fruit-bowl.jpg', alt: 'کاسه میوه لب طلا' },
      { src: '/exhibition/lab-tala/02-chocolate.jpg', alt: 'شکلات‌خوری لب طلا' },
      { src: '/exhibition/lab-tala/03-nut-bowl.jpg', alt: 'ظرف آجیل لب طلا' },
      { src: '/exhibition/lab-tala/04-single-tier.jpg', alt: 'شیرینی تک‌طبقه لب طلا' },
      { src: '/exhibition/lab-tala/05-two-tier.jpg', alt: 'شیرینی دوطبقه لب طلا' },
      { src: '/exhibition/lab-tala/06-small-bowl.jpg', alt: 'پیاله لب طلا' },
      { src: '/exhibition/lab-tala/99-price-list.jpg', alt: 'لیست قیمت لب طلا' },
    ],
  },
];

export function findExhibitionCatalog(slug: string): ExhibitionCatalog | null {
  const key = slug.trim().toLowerCase();
  return (
    EXHIBITION_CATALOGS.find(
      (c) => c.slug === key || c.aliases.includes(key),
    ) ?? null
  );
}

export function exhibitionCatalogName(catalog: ExhibitionCatalog, locale: string) {
  return locale === 'fa' ? catalog.nameFa : catalog.nameEn;
}
