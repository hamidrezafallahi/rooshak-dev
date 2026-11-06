import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';

import SectionHeading from '@components/molecules/storefront/SectionHeading';
import { getAll } from '@lib/getAll';
import { IBrand } from '@models/brand';

import BrandCard from '../brandCard';
import ApiErrorNotice from '@components/molecules/apiErrorNotice';

export default async function LandingBrands() {
  const locale = await getLocale();
  const t = await getTranslations('landing');
  const response = await getAll<IBrand>('brands', {
    page: 1,
    pageSize: 4,
    byConfig: false,
  });
  const brands = response?.data?.records ?? [];
  if (!brands.length) {
    return response.isSuccess ? null : (
      <section className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 w-full max-w-[1440px]">
        <ApiErrorNotice message={response.error} />
      </section>
    );
  }

  return (
    <section className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 w-full max-w-[1440px]">
      <SectionHeading
        title={t('brandsTitle')}
        subtitle={t('brandsSubtitle')}
        href={`/${locale}/brands`}
        linkLabel={t('viewAllBrands')}
      />
      <div className="gap-4 md:gap-6 grid grid-cols-2 lg:grid-cols-4">
        {brands.map((brand) => (
          <BrandCard brand={brand} key={brand.id ?? brand.slug} />
        ))}
      </div>
    </section>
  );
}
