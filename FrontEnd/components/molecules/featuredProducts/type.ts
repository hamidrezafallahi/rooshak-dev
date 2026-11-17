export interface IFeaturedProduct {
  id: number | string;
  /** Product name shown as the card title. */
  name: string;
  /** Short plain-text description (clamped to 2 lines on the card). */
  description: string;
  /** Backend media path or absolute URL; resolved through MediaImage. */
  image: string | null;
  /** Descriptive alt text; falls back to the product name. */
  imageAlt?: string;
  /** Price already formatted for display, e.g. "۱٬۲۵۰٬۰۰۰ تومان". */
  priceLabel?: string | null;
  /** Up to two small badges (extra items are ignored). */
  badges?: string[];
  /** Product detail page URL. */
  href: string;
}

export interface IProductCardProps {
  product: IFeaturedProduct;
  /** Button label, e.g. "مشاهده محصول". */
  ctaLabel: string;
  /** Mark the image as high priority (above-the-fold usage only). */
  priority?: boolean;
}
