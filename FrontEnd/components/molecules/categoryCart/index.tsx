import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { ICategory } from '@models/category';

export default async function CategoryCard({
  category,
  priority = false,
}: {
  category: ICategory;
  priority?: boolean;
}) {
  const {
    id,
    categoryCover,
    persianName,
    englishName,
    categoryPersianDesc,
    categoryEnglishDesc,
  } = category;
  const locale = await getLocale();
  const t = await getTranslations('landing');
  const name = locale === 'fa' ? persianName : englishName;
  const desc = locale === 'fa' ? categoryPersianDesc : categoryEnglishDesc;

  return (
    <Link
      href={`/${locale}/categories/${category.slug || id}`}
      className="group relative block bg-store-muted aspect-[4/5] sm:aspect-[4/3] overflow-hidden"
    >
      <MediaImage
        src={categoryCover}
        alt={name || ''}
        fill
        className="object-cover group-hover:scale-105 transition-transform duration-[1200ms]"
        sizes="(max-width: 1024px) 50vw, 33vw"
        loading={priority ? undefined : 'lazy'}
      />
      <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/5 to-transparent" />
      <div className="absolute inset-x-0 bottom-0 flex flex-col items-start gap-1 p-4 sm:p-6 text-white">
        <h3 className="font-normal text-lg sm:text-2xl leading-tight">{name}</h3>
        {desc ? (
          <p className="hidden sm:block max-w-xs text-white/85 text-sm line-clamp-2">{desc}</p>
        ) : null}
        <span className="mt-1 pb-0.5 border-white border-b font-medium text-xs sm:text-sm">
          {t('discover')}
        </span>
      </div>
    </Link>
  );
}
