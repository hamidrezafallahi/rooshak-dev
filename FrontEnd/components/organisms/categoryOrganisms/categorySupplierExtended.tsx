import React from 'react';

import { getTranslations } from 'next-intl/server';

import {
  SimpleSupplierCard,
} from '@components/molecules/supplierCard/simpleSupplierCard';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IUser } from '@models/user';

export async function CategorySupplierExtended({ id }: { id: number }) {
  const t = await getTranslations();

  const result = await safeFetchJson<
    SimpleResponse<IUser[] | { records?: IUser[] }>
  >(
    `${serverApiBaseUrl}/productOffers/getSuppliersByCategoryId?CategoryId=${id}`,
    { next: { revalidate: 36 } },
  );

  if (!result.ok || !result.data) {
    return <div>{t('category.noCategorySupplier')}</div>;
  }

  const payload = result.data.data;
  const suppliers: IUser[] = Array.isArray(payload)
    ? payload
    : Array.isArray(payload?.records)
      ? payload.records
      : [];

  return (
    <div className="gap-5 grid grid-cols-1 lg:grid-cols-4">
      {suppliers.length > 0 ? (
        suppliers.map((supplier: IUser, index: number) => (
          <SimpleSupplierCard key={supplier.id ?? index} supplier={supplier} />
        ))
      ) : (
        <div>{t('category.noCategorySupplier')}</div>
      )}
    </div>
  );
}
