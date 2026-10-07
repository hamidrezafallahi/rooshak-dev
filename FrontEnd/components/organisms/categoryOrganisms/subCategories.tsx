import React from 'react';

import { getTranslations } from 'next-intl/server';

import CategoryCard from '@components/molecules/categoryCart';
import { ICategory } from '@models/category';

export async function SubCategories({ sub }: { sub?: ICategory[] }) {
  const t = await getTranslations();
  return (
    <div className="p-4">
      <div className="text-store-text text-center">
        {t("category.subCategories")}
      </div>
      <div className="gap-6 grid grid-cols-1 sm:grid-cols-3 py-10">
        {sub && sub.length > 0 ? (
          sub.map((c, idx) => <CategoryCard category={c} key={idx} />)
        ) : (
          <div>{t("category.noSubCategory")}</div>
        )}
      </div>
    </div>
  );
}
