import {
  getLocale,
  getTranslations,
} from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';

import {
  ISupplier,
  SupplierCardGrid,
} from './supplierCard';

export async function ProductSupplierExtended({
  productId,
}: {
  productId: string | number;
}) {
  const response = await fetch(
    `${serverApiBaseUrl}/productOffers/by-product/${productId}`,
    {
      next: { revalidate: 36 },
    },
  );
  const result = await response.json();
  const suppliers = result?.data || [];
  const locale = await getLocale();
  const t = await getTranslations();
  if (suppliers.length === 0) return null;
 
  return (
    <section className="mt-16">
      {/* هدر بخش با دیزاین بهتر */}
      <div className="flex justify-between items-center mb-6">
        <div className="flex items-center gap-2">
          <div className="bg-primary rounded-full w-1 h-7"></div>
          <h3 className="font-bold text-gray-800 text-xl">
            {t('product.suppliersTitle')}
          </h3>
        </div>
        <span className="bg-gray-100 px-3 py-1 rounded-full text-gray-600 text-sm">
          {t('product.suppliersCount', { count: suppliers.length })}
        </span>
      </div>

      {/* گرید کارت‌های تأمین‌کنندگان */}
      <div className="gap-5 grid grid-cols-1 lg:grid-cols-4">
        {suppliers.map((supplier: ISupplier, index: number) => (
          <SupplierCardGrid
            key={index}
            supplier={supplier}
            productId={Number(productId)}
            locale={locale}
          />
        ))}
      </div>
    </section>
  );
}
