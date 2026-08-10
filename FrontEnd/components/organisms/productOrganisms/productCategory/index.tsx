import React from 'react';

import CategoryCard from '@components/molecules/categoryCart';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { ICategory } from '@models/category';

export default async function ProductCategory({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<ICategory>>(
    `${serverApiBaseUrl}/Categories/${id}`,
    { next: { revalidate: 36 } },
  );
  const cat =
    result.ok && result.data?.isSuccess !== false ? result.data?.data : null;
  if (!cat) return null;

  return (
    <div className="gap-6 grid grid-cols-1 sm:grid-cols-3 py-10">
      <CategoryCard category={cat} />
    </div>
  );
}
