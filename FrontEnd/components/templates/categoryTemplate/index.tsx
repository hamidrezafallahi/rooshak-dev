import React from 'react';

import {
  getLocale,
  getTranslations,
} from 'next-intl/server';
import Link from 'next/link';

import { Banner } from '@components/molecules/banner';
import {
  CategoryDescription,
  CategoryProducts,
  CategorySupplierExtended,
  SubCategories,
} from '@components/organisms/categoryOrganisms';
import { ICategory } from '@models/category';

export default async function CategoryTemplate({
  category,
}: {
  category: ICategory;
}) {
  const locale = await getLocale();
  const t = await getTranslations();

  if (!category) {
    return null;
  }

  return (
    <div className="max-w-7xl">
      <Banner
        src={category.categoryCover}
        name={locale == 'fa' ? category.persianName : category.englishName}
      />
      <CategoryDescription
        desc={
          locale == 'fa'
            ? category.categoryPersianDesc
            : category.categoryEnglishDesc
        }
      />
      {category.parentCategoryId ? (
        <div className="relative">
          <Link
            className="top-2 absolute bg-store-surface px-3 py-2 text-store-text text-sm"
            href={`/${locale}/categories/${category.parentCategoryId}`}
          >
            {t('category.goToParent')}
          </Link>
        </div>
      ) : null}
      <SubCategories sub={category.subCategories ?? []} />
      <CategoryProducts id={category.id} />
      <CategorySupplierExtended id={category.id} />
    </div>
  );
}
