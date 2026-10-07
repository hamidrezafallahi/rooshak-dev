import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';

import { ICategory } from '@models/category';

import CategoryCard from '../categoryCart';
import SectionHeading from '../storefront/SectionHeading';

interface IProps {
  categories: ICategory[];
}

export default async function LandingCategory({ categories }: IProps) {
  const locale = await getLocale();
  const t = await getTranslations('landing');
  if (!categories?.length) return null;

  return (
    <section
      id="categories"
      className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 w-full max-w-[1440px]"
    >
      <SectionHeading
        title={t('categoriesTitle')}
        href={`/${locale}/categories`}
        linkLabel={t('viewAllCategories')}
      />
      <div className="gap-3 md:gap-5 grid grid-cols-2 lg:grid-cols-3">
        {categories.map((cat, index) => (
          <CategoryCard key={cat.id ?? index} category={cat} priority={index < 2} />
        ))}
      </div>
    </section>
  );
}
