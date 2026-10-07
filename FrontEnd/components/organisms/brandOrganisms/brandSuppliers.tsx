import React from 'react';

import {
  getLocale,
  getTranslations,
} from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IUser } from '@models/user';

export async function BrandSuppliers({ id }: { id: number }) {
  const result = await safeFetchJson<SimpleResponse<IUser[]>>(
    `${serverApiBaseUrl}/Brands/getProductsSuppliersByBrandId/${id}`,
    { next: { revalidate: 36 } },
  );
  const suppliers: IUser[] =
    result.ok && result.data?.isSuccess !== false ? result.data?.data || [] : [];
  const locale = await getLocale();
  const t = await getTranslations();

  return (
    <div className="my-10">
      <h2 className="mb-6 font-normal text-2xl">{t('product.brandSuppliers')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        {suppliers.map((s) => (
          <Link
            key={s.id}
            href={`/${locale}/suppliers/${s.slug || s.id}`}
            className="flex flex-col items-center bg-store-surface border border-store-border hover:border-store-strong p-4 transition-colors"
          >
            <div className="relative flex justify-center items-center bg-store-muted mb-3 rounded-full w-24 h-24 overflow-hidden">
              <MediaImage
                src={s.userImage}
                alt={s.fullName}
                fill
                className="object-cover"
                priority
              />
            </div>
            <h3 className="font-normal text-center">
              {s.fullName}
            </h3>
            <span className="mt-1 text-store-subtle text-xs">{s.email}</span>
            <span className="mt-1 text-store-subtle text-xs">{s.phoneNumber}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}
