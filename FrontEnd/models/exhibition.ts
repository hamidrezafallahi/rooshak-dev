import { TDimensions } from './product';

/** One row of an exhibition price list. Mirrors BackEnd TagPriceListItemDto. */
export interface IPriceListItem {
  productId: number;
  slug: string;
  name: string;
  description?: string | null;
  mainImage?: string | null;
  categoryName?: string | null;
  categorySlug?: string | null;
  brandName?: string | null;
  brandSlug?: string | null;
  code: string;
  price?: number | null;
  finalPrice?: number | null;
  discountAmount?: number | null;
  discountIsPercent?: boolean | null;
  hasDiscount: boolean;
  inventory: number;
  inStock: boolean;
  diameter?: string | null;
  height?: string | null;
  pieceCount?: string | null;
  dimensions?: Partial<TDimensions> | null;
}

/** Mirrors BackEnd TagPriceListDto. */
export interface IPriceList {
  tagId: number;
  tagName: string;
  tagSlug: string;
  currency: string;
  itemCount: number;
  updatedAt: string;
  items: IPriceListItem[];
}

/** Mirrors BackEnd TagFamilyDto. */
export interface ITagFamily {
  id: number;
  name: string;
  slug: string;
  productCount: number;
  coverImage?: string | null;
}
