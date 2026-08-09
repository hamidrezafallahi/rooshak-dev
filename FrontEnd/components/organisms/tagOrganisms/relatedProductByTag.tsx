import React from 'react';

import { getTranslations } from 'next-intl/server';

import { SimpleProductCard } from '@components/molecules/productCard';
import { ISimpleProduct } from '@components/molecules/productCard/type';
import { serverApiBaseUrl } from '@lib/api';
import { SimpleResponse } from '@models/base';

export async function RelatedProductByTag({ tagId }: { tagId: number }) {
  const response = await fetch(
    `${serverApiBaseUrl}/ProductOfferTags/tag/${tagId}`,{next: { revalidate: 36 }});
  const productsResponse: SimpleResponse<ISimpleProduct[]> = await response.json();
  const t = await getTranslations();
  console.log(productsResponse)
  return (
    <div className="pb-10">
      <h2 className="mb-4 font-bold text-xl">{t('product.tagProducts')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5">
        {productsResponse.data.map((p, idx) => (
          <SimpleProductCard key={idx} product={p} />
        ))}
      </div>
    </div>
  );
}
