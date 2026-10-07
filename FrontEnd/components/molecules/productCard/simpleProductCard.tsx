import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';

import { ISimpleProduct } from './type';

function formatMoney(value: number, locale: string) {
  return new Intl.NumberFormat(locale === 'fa' ? 'fa-IR' : 'en-US').format(value);
}

export async function SimpleProductCard({
  product,
}: {
  product: ISimpleProduct;
}) {
  const locale = await getLocale();
  const t = await getTranslations('common');
  const href = `/${locale}/products/${product.slug || product.id}`;
  const final = product.finalPrice && product.finalPrice > 0 ? product.finalPrice : product.price;
  const hasDiscount =
    product.price != null && final != null && final > 0 && final < product.price;

  return (
    <article className="group flex flex-col h-full text-store-text">
      <Link href={href} className="block relative bg-store-muted aspect-[4/5] overflow-hidden">
        <MediaImage
          src={product.mainImage}
          alt={product.name}
          fill
          className="object-cover group-hover:scale-105 transition-transform duration-700"
          sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 20vw"
        />
        {product.inventory != null && product.inventory <= 0 && (
          <span className="top-3 start-3 absolute bg-store-surface px-2.5 py-1 font-medium text-[0.7rem] text-store-text">
            {t('outOfStock')}
          </span>
        )}
      </Link>

      <div className="flex flex-col flex-1 gap-1 pt-3 sm:pt-4">
        <h3 className="font-normal text-sm sm:text-base line-clamp-2">
          <Link href={href} className="hover:underline underline-offset-4">
            {product.name}
          </Link>
        </h3>
        {final != null && final > 0 && (
          <p className="flex flex-wrap items-baseline gap-x-2 text-sm">
            <span className="font-medium">
              {formatMoney(final, locale)} {t('currency')}
            </span>
            {hasDiscount && (
              <span className="text-store-subtle text-xs line-through">
                {formatMoney(product.price as number, locale)}
              </span>
            )}
          </p>
        )}

        {product.suppliers && product.suppliers.length > 0 && (
          <div className="flex mt-2 -space-x-2 rtl:space-x-reverse">
            {product.suppliers.slice(0, 4).map((s, idx) => (
              <Link
                className="hover:z-20 relative bg-store-surface border border-store-border rounded-full w-8 h-8 overflow-hidden"
                key={idx}
                href={`/${locale}/suppliers/${s.id}`}
                aria-label={s.fullName}
              >
                <MediaImage
                  alt={s.fullName}
                  src={s.image}
                  fill
                  sizes="32px"
                  loading="lazy"
                  className="p-[2px] rounded-full object-cover"
                />
              </Link>
            ))}
          </div>
        )}
      </div>
    </article>
  );
}
