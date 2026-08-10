import React from 'react';

import { getTranslations } from 'next-intl/server';

import { SimpleProductCard } from '@components/molecules/productCard';
import { ISimpleProduct } from '@components/molecules/productCard/type';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';

export async function RelatedProductByTag({ tagId }: { tagId: number }) {
  const result = await safeFetchJson<SimpleResponse<ISimpleProduct[]>>(
    `${serverApiBaseUrl}/ProductOfferTags/tag/${tagId}`,
    { next: { revalidate: 36 } },
  );
  const products: ISimpleProduct[] =
    result.ok && result.data?.isSuccess !== false ? result.data?.data || [] : [];
  const t = await getTranslations();
  return (
    <div className="pb-10">
      <h2 className="mb-4 font-bold text-xl">{t('product.tagProducts')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5">
        {products.map((p, idx) => (
          <SimpleProductCard key={idx} product={p} />
        ))}
      </div>
    </div>
  );
}
