import React from 'react';

import { getTranslations } from 'next-intl/server';

import CategoryCard from '@components/molecules/categoryCart';
import { serverApiBaseUrl } from '@lib/api';
import { SimpleResponse } from '@models/base';
import { ICategory } from '@models/category';

export async function BrandCategories({ id }: { id: number }) {
  const response = await fetch(
    `${serverApiBaseUrl}/Brands/getProductsCategoriesByBrandId/${id}`,
    {
      cache: "no-store",
    },
  );
  const categoriesResponse: SimpleResponse<ICategory[]> = await response.json();
  const categories: ICategory[] = categoriesResponse.data;
  const t = await getTranslations();
  return (
    <div className="my-10">
      <h2 className="mb-4 font-bold text-xl">{t('product.brandCategories')}</h2>
      <div className="flex gap-4 pb-2 overflow-x-auto">
        {categories.map((cat, idx) => (
          <CategoryCard key={idx} category={cat}/>
        ))}
      </div>
    </div>
  );
}
