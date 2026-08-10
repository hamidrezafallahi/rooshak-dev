import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { ICategory } from '@models/category';

import CategoryCard from '../categoryCart';

interface IProps {
  categories: ICategory[];
}
export default async  function LandingCategory(props: IProps) {
  const { categories } = props;
  const locale = await getLocale();
  const t = await getTranslations('landing');
  return (
    <section id="categories" className="flex flex-col gap-4 mx-auto px-4 py-10 w-full max-w-7xl">
       <div className="flex justify-between items-center gap-3">
         <h2 className="font-semibold text-2xl sm:text-3xl">{t('categoriesTitle')}</h2>
         <Link href={`/${locale}/categories`} className="block text-sm text-end underline">
                  {t('viewAllCategories')}
         </Link>
       </div>
        <div className="hidden-show-scrollbar sm:hidden flex gap-4 pb-2 overflow-x-auto">
          {categories?.map((cat) => (
            <Link
              key={cat.id}
              href={`/${locale}/categories/${cat.slug || cat.id}`}
              aria-label={cat.persianName || cat.englishName}
              className="group relative flex-shrink-0 bg-white shadow-sm hover:shadow-lg rounded-2xl min-w-[70%] sm:min-w-0 overflow-hidden transition-shadow"
            >
              <div className="relative w-full h-44 overflow-hidden">
                <MediaImage
                  src={cat.categoryCover}
                  alt={cat.persianName || cat.englishName}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform transform"
                  sizes="70vw"
                  loading="lazy"
                />
              </div>
              <div className="p-4">
                <h3 className="font-medium">{cat.persianName}</h3>
                <p className="text-gray-600 text-xs">{cat.categoryPersianDesc}</p>
              </div>
            </Link>
          ))}
        </div>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-3 py-10">
        {categories?.map((cat,index) => (
            <CategoryCard
            key={index}
            category={cat}
        />
          ))}
      </div>
    </section>
  );
}
