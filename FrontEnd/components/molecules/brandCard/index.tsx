"use client";
import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { IBrand } from '@models/brand';

export default function BrandCard({brand}:{brand:IBrand}) {
    const locale = useLocale();
    const t = useTranslations('common');
  return (
              <Link
            href={`/${locale}/brands/${brand.slug || brand.id}`}
              className="group relative bg-white shadow-sm hover:shadow-lg p-0 !rounded-2xl overflow-hidden text-left transition-shadow"
            >
          <div className="relative flex justify-center items-center bg-inherit w-full h-48 overflow-hidden">
                <MediaImage
                  src={brand.logoFile}
                  alt={brand.name}
                  width={400}
                  height={400}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform transform"
                  loading="lazy"
                />
              </div>
              <div className="bg-inherit p-4 sm:p-5">
                <h3 className="font-medium">{brand.name}</h3>
                <p className="text-gray-600 text-xs">{brand.description}</p>
                <span className="inline-block mt-3 font-medium text-primary text-sm">
                  {t('viewBrandArrow')}
                </span>
              </div>
            </Link>
  )
}
