import React from 'react';

import BrandCard from '@components/molecules/brandCard';
import { serverApiBaseUrl } from '@lib/api';
import { loadFailure, safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IBrand } from '@models/brand';
import ApiErrorNotice from '@components/molecules/apiErrorNotice';

export default async function ProductBrand({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IBrand>>(
    `${serverApiBaseUrl}/Brands/${id}`,
    { next: { revalidate: 36 } },
  );
  const failure = loadFailure(result);
  if (failure) return <ApiErrorNotice message={failure} />;
  const brand =
    result.ok && result.data?.isSuccess !== false ? result.data?.data : null;
  if (!brand) return null;

  return (
    <div className="gap-6 grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 py-10 border-t border-store-border">
      <BrandCard brand={brand} />
    </div>
  );
}
