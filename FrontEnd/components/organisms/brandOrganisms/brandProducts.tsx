import React from 'react';

import { getTranslations } from 'next-intl/server';

import { SimpleProductCard } from '@components/molecules/productCard';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IProduct } from '@models/product';

export async function BrandProducts({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IProduct[]>>(
    `${serverApiBaseUrl}/Brands/getProductByBrandId/${id}`,
    { next: { revalidate: 36 } },
  );
  const products: IProduct[] =
    result.ok && result.data?.isSuccess !== false ? result.data?.data || [] : [];
  const t = await getTranslations();
  return (
    <div className="py-10">
      <h2 className="mb-4 font-bold text-xl">{t('product.brandProducts')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5">
        {products.map((product, idx) => (
          <SimpleProductCard key={idx} product={product} />
        ))}
      </div>
    </div>
  );
}
