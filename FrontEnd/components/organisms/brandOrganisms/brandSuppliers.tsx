import React from 'react';

import {
  getLocale,
  getTranslations,
} from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { serverApiBaseUrl } from '@lib/api';
import { SimpleResponse } from '@models/base';
import { IUser } from '@models/user';

export async function BrandSuppliers({ id }: { id: number }) {
  const response = await fetch(
    `${serverApiBaseUrl}/Brands/getProductsSuppliersByBrandId/${id}`,
    {
      cache: "no-store",
    },
  );
  const suppliersResponse: SimpleResponse<IUser[]> = await response.json();
  const suppliers: IUser[] = suppliersResponse.data;
  const locale = await getLocale();
  const t = await getTranslations();

  return (
    <div className="my-10">
      <h2 className="mb-4 font-bold text-xl">{t('product.brandSuppliers')}</h2>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
        {suppliers.map((s) => (
          <Link
            key={s.id}
            href={`/${locale}/suppliers/${s.slug || s.id}`}
            className="flex flex-col items-center bg-white shadow-sm hover:shadow-lg p-4 rounded-2xl transition"
          >
            <div className="relative flex justify-center items-center bg-gray-50 mb-3 rounded-full w-24 h-24 overflow-hidden">
              <MediaImage
                src={s.userImage}
                alt={s.fullName}
                fill
                className="object-cover"
                priority
              />
            </div>
            <h3 className="font-semibold text-gray-900 text-center">
              {s.fullName}
            </h3>
            <span className="mt-1 text-gray-400 text-xs">{s.email}</span>
            <span className="mt-1 text-gray-400 text-xs">{s.phoneNumber}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}
