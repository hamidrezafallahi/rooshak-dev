import {
  getLocale,
  getTranslations,
} from 'next-intl/server';

import { IDetailedProduct } from '@models/product';

import ProductPrice from './productPrice';
import ProductRate from './productRate';

export default async function ProductInfo({ product }: { product: IDetailedProduct }) {
  const locale = await getLocale();
  const t = await getTranslations();

  const formatLength = (value: number) => {
    const unit = locale === 'en' ? 'inch' : 'centimeter';
    const converted = locale === 'en' ? value / 2.54 : value;
    return new Intl.NumberFormat(locale, { style: 'unit', unit, unitDisplay: 'short' }).format(converted);
  };

  const formatWeight = (value: number) => {
    const unit = locale === 'en' ? 'ounce' : 'gram';
    const converted = locale === 'en' ? value / 28.3495 : value;
    return new Intl.NumberFormat(locale, { style: 'unit', unit, unitDisplay: 'short' }).format(converted);
  };

  return (
    <div className="flex flex-col gap-5 lg:sticky lg:top-[calc(var(--store-header-h)+1.5rem)] max-w-xl">
      {product.brandName && (
        <p className="text-store-subtle text-xs uppercase ltr:tracking-[0.14em]">{product.brandName}</p>
      )}
      <h1 className="font-normal text-2xl md:text-4xl leading-tight">{product.name}</h1>

      {product.categoryName && (
        <p className="text-store-subtle text-sm">{product.categoryName}</p>
      )}

      {product.description && (
        <p className="text-store-subtle leading-relaxed line-clamp-4">{product.description}</p>
      )}

      <ProductPrice
        price={product.price}
        finalPrice={product.finalPrice}
        currency={product.currency}
        inStock={product.inStock}
        inventory={product.inventory}
        locale={locale}
      />

      <ProductRate
        id={product.id}
        average={product.averageRate}
        count={product.rateCount}
      />

      {product.dimensions && (
        <div className="flex flex-wrap gap-x-6 gap-y-2 pt-5 border-t border-store-border text-store-subtle text-sm">
          {product.dimensions.width ? (
            <p>{t('product.width')}: {formatLength(product.dimensions.width)}</p>
          ) : null}
          {product.dimensions.height ? (
            <p>{t('product.height')}: {formatLength(product.dimensions.height)}</p>
          ) : null}
          {product.dimensions.depth ? (
            <p>{t('product.depth')}: {formatLength(product.dimensions.depth)}</p>
          ) : null}
          {product.dimensions.weight ? (
            <p>{t('product.weight')}: {formatWeight(product.dimensions.weight)}</p>
          ) : null}
        </div>
      )}
    </div>
  );
}
