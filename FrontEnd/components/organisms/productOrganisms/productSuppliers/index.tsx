import {
  getLocale,
  getTranslations,
} from 'next-intl/server';

import EntityGrid from '@components/molecules/storefront/EntityGrid';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';

import {
  ISupplier,
  SupplierCardGrid,
} from './supplierCard';

export async function ProductSupplierExtended({
  productId,
}: {
  productId: string | number;
}) {
  const result = await safeFetchJson<SimpleResponse<ISupplier[]>>(
    `${serverApiBaseUrl}/productOffers/by-product/${productId}`,
    { next: { revalidate: 36 } },
  );
  const suppliers =
    result.ok && result.data?.isSuccess !== false
      ? result.data?.data || []
      : [];
  const locale = await getLocale();
  const t = await getTranslations();
  if (suppliers.length === 0) return null;

  return (
    <section className="mt-16">
      <div className="flex justify-between items-center gap-3 mb-6">
        <div className="flex items-center gap-2 min-w-0">
          <div className="bg-primary rounded-full w-1 h-7 shrink-0"></div>
          <h3 className="font-bold text-gray-800 text-xl truncate">
            {t('product.suppliersTitle')}
          </h3>
        </div>
        <span className="bg-gray-100 px-3 py-1 rounded-full text-gray-600 text-sm shrink-0">
          {t('product.suppliersCount', { count: suppliers.length })}
        </span>
      </div>

      <EntityGrid cols="cards">
        {suppliers.map((supplier: ISupplier, index: number) => (
          <SupplierCardGrid
            key={supplier.id ?? index}
            supplier={supplier}
            productId={Number(productId)}
            locale={locale}
          />
        ))}
      </EntityGrid>
    </section>
  );
}
