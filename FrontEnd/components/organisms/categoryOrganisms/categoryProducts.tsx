import React from 'react';

import { getTranslations } from 'next-intl/server';

import { SimpleProductCard } from '@components/molecules/productCard';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { ILandingProduct } from '@models/product';

export async function CategoryProducts({ id }: { id: number }) {
  const t = await getTranslations('productsPage');

  const result = await safeFetchJson<SimpleResponse<ILandingProduct[] | { records?: ILandingProduct[] }>>(
    `${serverApiBaseUrl}/Products/getProductByCategoryId/${id}`,
    { next: { revalidate: 36 } },
  );

  if (!result.ok || !result.data) return null;

  const payload = result.data.data;
  const products: ILandingProduct[] = Array.isArray(payload)
    ? payload
    : Array.isArray(payload?.records)
      ? payload.records
      : [];

  if (products.length === 0) return null;

  return (
    <div className="pb-10">
      <h2 className="mb-6 font-normal text-2xl">{t('title')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5">
        {products.map((product) => (
          <SimpleProductCard
            key={product.id ?? product.slug}
            product={product}
          />
        ))}
      </div>
    </div>
  );
}
