import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';

import { ISimpleProduct } from './type';

export async function SimpleProductCard({
  product,
}: {
  product: ISimpleProduct;
}) {
  const locale = await getLocale();
  const t = await getTranslations('common');
  return (
    <article
      key={product.id}
      className="flex-shrink-0 bg-white shadow-sm hover:shadow-lg mx-auto rounded-2xl w-full overflow-hidden transition-shadow"
    >
      <div className="relative w-full h-56 overflow-hidden">
        <MediaImage
          src={product.mainImage}
          alt={product.name}
          fill
          className="object-cover"
          sizes="(max-width: 640px) 100vw, 280px"
        />
      </div>
      <div className="p-4">
        <h4 className="font-medium text-sm line-clamp-2">{product.name}</h4>
        <p className="text-gray-500 text-xs">{product.description}</p>
        <div className="flex justify-between items-end gap-2 mt-3">
          <Link
            href={`/${locale}/products/${product.slug || product.id}`}
            className="bg-gray-100 hover:bg-gray-200 px-3 py-2 rounded-lg text-xs"
          >
            {t('view')}
          </Link>
          {product.suppliers&&product.suppliers?.length > 0 && (
            <div className="flex flex-row-reverse flex-1 p-1 overflow-hidden">
              {product.suppliers.map((s, idx) => (
                <Link
                  className="hover:z-20 relative bg-white rounded-full w-10 h-10 overflow-hidden hover:scale-110 transition-all duration-300"
                  style={{ right: idx * 20 }}
                  key={idx}
                  href={`/${locale}/suppliers/${s.id}`}
                >
                  <MediaImage
                    alt={s.fullName}
                    src={s.image}
                    priority
                    fill
                    loading={"eager"}
                    className='p-[3px] rounded-full'
                  />
                </Link>
              ))}
            </div>
          )}
        </div>
      </div>
    </article>
  );
}
