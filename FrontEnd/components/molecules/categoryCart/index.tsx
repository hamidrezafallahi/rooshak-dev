import React from 'react';

import { getLocale } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { ICategory } from '@models/category';

export default async function CategoryCard({ category }: { category: ICategory}) {
  const {
    id,
    categoryCover,
    persianName,
    englishName,
    categoryPersianDesc,
    categoryEnglishDesc,
  } = category;
  const locale = await getLocale()
   return (
    <Link
      href={`/${locale}/categories/${category.slug || id}`}
      className="group relative shadow-sm rounded-2xl h-44 md:h-52 overflow-hidden"
    >
      <MediaImage
        src={categoryCover}
        alt={locale == "fa" ? persianName : englishName}
        fill
        className="w-full h-full object-cover group-hover:scale-105 transition-transform transform"
        sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 380px"
        loading="lazy"
      />
      <div className="absolute inset-0 flex items-end bg-gradient-to-t from-black/40 to-transparent p-4">
        <div>
          <h3 className="font-semibold text-white">
            {locale == "fa" ? persianName : englishName}
          </h3>
          <p className="text-white text-xs">
            {locale == "fa" ? categoryPersianDesc : categoryEnglishDesc}
          </p>
        </div>
      </div>
    </Link>
  );
}
