import React from 'react';

import TagCard from '@components/molecules/tagCard';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IProductTags } from '@models/product';

export default async function ProductTags({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IProductTags[]>>(
    `${serverApiBaseUrl}/ProductOfferTags/productId/${id}`,
    { next: { revalidate: 36 } },
  );
  const tags =
    result.ok && result.data?.isSuccess !== false ? result.data?.data || [] : [];

  return (
    <div className="flex justify-start items-center gap-2 p-2 w-full">
      {tags.map((tag, index) => (
        <TagCard key={index} tag={{ id: tag.tagId, name: tag.tagName }} />
      ))}
    </div>
  );
}
