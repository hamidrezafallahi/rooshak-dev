import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { IBrand } from '@models/brand';

export default function BrandCard({ brand }: { brand: IBrand }) {
  const locale = useLocale();
  const t = useTranslations('common');

  return (
    <Link
      href={`/${locale}/brands/${brand.slug || brand.id}`}
      className="group flex flex-col text-store-text"
    >
      <div className="relative bg-store-muted aspect-square overflow-hidden">
        <MediaImage
          src={brand.logoFile}
          alt={brand.name}
          fill
          className="object-cover group-hover:scale-105 transition-transform duration-700"
          sizes="(max-width: 640px) 50vw, (max-width: 1024px) 50vw, 25vw"
          loading="lazy"
        />
      </div>
      <div className="flex flex-col gap-1 pt-4">
        <h3 className="font-normal text-base sm:text-lg">{brand.name}</h3>
        {brand.description ? (
          <p className="text-store-subtle text-xs sm:text-sm line-clamp-2">{brand.description}</p>
        ) : null}
        <span className="self-start mt-2 pb-0.5 border-current border-b font-medium text-sm group-hover:opacity-60 transition-opacity">
          {t('viewBrand')}
        </span>
      </div>
    </Link>
  );
}
