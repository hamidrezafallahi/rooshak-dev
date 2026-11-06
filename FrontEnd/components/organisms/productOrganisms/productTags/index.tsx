import React from 'react';

import TagCard from '@components/molecules/tagCard';
import { serverApiBaseUrl } from '@lib/api';
import { loadFailure, safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IProductTags } from '@models/product';
import ApiErrorNotice from '@components/molecules/apiErrorNotice';

export default async function ProductTags({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IProductTags[]>>(
    `${serverApiBaseUrl}/ProductOfferTags/productId/${id}`,
    { next: { revalidate: 36 } },
  );
  const failure = loadFailure(result);
  if (failure) return <ApiErrorNotice message={failure} />;
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
