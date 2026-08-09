import React from 'react';

import {
  getLocale,
  getTranslations,
} from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { IBrand } from '@models/brand';

export async function SupplierBrands(props: { brands: IBrand[] }) {
  const { brands } = props;
  const locale = await getLocale();
  const t = await getTranslations();

  // حذف برندهای تکراری
  const uniqueBrands = Array.from(new Map(brands.map((b) => [b.id, b])).values());

  return (
<div className="mb-10">
      <div className="gap-6 sm:grid grid-cols-6">
        {uniqueBrands?.map((b, i) => (
          <Link
          href={`/${locale}/brands/${b.id}`}
            key={i}
            className="group relative shadow-sm rounded-2xl h-24 overflow-hidden"
          >
            <MediaImage
               src={b.logoFile}
              alt={b.name}
              className="w-full h-full object-cover group-hover:scale-105 transition-transform transform"
              loading="lazy"
              width={100}
              height={100}
            />
            <div className="absolute inset-0 flex items-end bg-gradient-to-t from-black/40 to-transparent p-4">
              <div>
                <h3 className="font-semibold text-white">
                  {b.name}
                </h3>
                {b.description && (
              <p className="mt-1 text-gray-100 text-sm text-center line-clamp-3">
                {b.description}
              </p>
            )}
            <span className="mt-3 text-gray-100 text-xs uppercase tracking-wider">
              {t('common.viewBrand')}
            </span>
            </div>
            </div>
          </Link>
        ))}
      </div>
    </div>



















 
  );
}
