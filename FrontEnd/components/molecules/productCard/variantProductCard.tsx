'use client';

import { useMemo, useState } from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import VariantPicker, { pickDefaultVariant } from '@components/molecules/variantPicker';
import { useAddToCart } from '@/hooks/useAddToCart';
import type { ILandingProduct } from '@models/product';

type Props = {
  product: ILandingProduct;
  /** `sizes` for the image; the default fits the landing carousel. */
  imageSizes?: string;
};

/**
 * Storefront product card. When the product has options (e.g. size or colour)
 * the shopper picks one on the card and the price, discount badge, stock and
 * add-to-cart all follow that option's offer.
 */
export default function VariantProductCard({
  product,
  imageSizes = '(max-width: 640px) 62vw, (max-width: 1024px) 34vw, 24vw',
}: Props) {
  const t = useTranslations();
  const locale = useLocale();
  const { add, pending } = useAddToCart();
  const variants = useMemo(() => product.variants ?? [], [product.variants]);
  const [selectedOfferId, setSelectedOfferId] = useState<number | null>(
    () => pickDefaultVariant(variants, product.bestOfferId)?.offerId ?? null,
  );
  const variant = variants.find((v) => v.offerId === selectedOfferId) ?? null;

  // With options, everything shown comes from the chosen option; otherwise from the best offer.
  const offerId = variant ? variant.offerId : product.bestOfferId;
  const price = variant ? variant.price : product.price;
  const finalPrice = variant ? variant.finalPrice : product.finalPrice;
  const discountAmount = variant ? variant.discountAmount : product.discountAmount;
  const discountIsPercent = variant ? variant.discountIsPercent : product.discountIsPercent;
  const inStock = variant ? variant.inStock : (product.inventory ?? 1) > 0;

  const canBuy = inStock && offerId != null && price != null && finalPrice != null;
  const hasDiscount = canBuy && (discountAmount ?? 0) > 0 && finalPrice! < price!;
  const money = new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US');
  const href = `/${locale}/products/${product.slug || product.id}`;

  const handleAdd = () => {
    if (!canBuy) return;
    add({
      productId: product.id,
      productOfferId: offerId!,
      name: variant ? `${product.name} - ${variant.name ? `${variant.name} ` : ''}${variant.value}` : product.name,
      description: product.description,
      price: price!,
      finalPrice: finalPrice!,
      discountAmount,
      discountIsPercent,
      mainImage: product.mainImage,
    });
  };

  return (
    <article className="group flex flex-col h-full text-store-text">
      <Link href={href} className="block relative bg-store-muted aspect-[4/5] overflow-hidden">
        <MediaImage
          src={product.mainImage}
          alt={product.name}
          fill
          className="object-cover group-hover:scale-105 transition-transform duration-700"
          sizes={imageSizes}
          loading="lazy"
        />
        {hasDiscount && (
          <span className="top-3 absolute bg-primary px-2.5 py-1 font-medium text-[0.7rem] text-primary-foreground start-3">
            -{money.format(discountAmount!)}
            {discountIsPercent && '%'}
          </span>
        )}
      </Link>
      <div className="flex flex-col flex-1 gap-1 pt-4">
        {product.brand && <p className="text-store-subtle text-xs">{product.brand}</p>}
        <h3 className="font-normal text-sm sm:text-base line-clamp-2">
          <Link href={href} className="hover:underline underline-offset-4">
            {product.name}
          </Link>
        </h3>

        {variants.length > 0 && (
          <div className="py-1">
            <VariantPicker
              variants={variants}
              selectedOfferId={selectedOfferId}
              onSelect={(v) => setSelectedOfferId(v.offerId)}
              size="sm"
              showLabel={false}
            />
          </div>
        )}

        <div aria-live="polite" className="flex flex-wrap items-baseline gap-x-2 mt-1 min-h-[1.25rem] text-sm">
          {canBuy ? (
            <>
              <span className="font-medium">
                {money.format(finalPrice!)} {t('common.currency')}
              </span>
              {hasDiscount && (
                <span className="text-store-subtle text-xs line-through">{money.format(price!)}</span>
              )}
            </>
          ) : (
            <span className="text-store-subtle">{t('common.outOfStock')}</span>
          )}
        </div>
        <button
          type="button"
          disabled={!canBuy || pending}
          onClick={handleAdd}
          aria-label={t('common.addToCartAria', {
            name: variant ? `${product.name} ${variant.value}` : product.name,
          })}
          className="self-start disabled:opacity-40 mt-3 pb-0.5 border-current border-b font-medium text-sm hover:opacity-60 transition-opacity disabled:cursor-not-allowed"
        >
          {t('common.addToCart')}
        </button>
      </div>
    </article>
  );
}
