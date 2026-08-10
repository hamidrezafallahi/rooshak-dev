import React from 'react';

import BrandCard from '@components/molecules/brandCard';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IBrand } from '@models/brand';

export default async function ProductBrand({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IBrand>>(
    `${serverApiBaseUrl}/Brands/${id}`,
    { next: { revalidate: 36 } },
  );
  const brand =
    result.ok && result.data?.isSuccess !== false ? result.data?.data : null;
  if (!brand) return null;

  return (
    <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 py-10">
      <BrandCard brand={brand} />
    </div>
  );
}
