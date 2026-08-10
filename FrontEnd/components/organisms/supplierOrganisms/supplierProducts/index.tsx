import React from 'react';

import { getTranslations } from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import { IDetailedProductOffer } from '@models/product';

import SupplierProductsCarousel from './supplierProductsCarousel';

export async function SupplierProducts(props: { id: number }) {
  const { id } = props;
  const t = await getTranslations();

  const result = await safeFetchJson<
    SimpleResponse<
      | IDetailedProductOffer[]
      | { records?: IDetailedProductOffer[]; productOffers?: IDetailedProductOffer[] }
    >
  >(`${serverApiBaseUrl}/productOffers/by-seller/${id}`, {
    next: { revalidate: 36 },
  });

  if (!result.ok || !result.data) {
    return <div>{t('common.noProductsFound')}</div>;
  }

  const payload = result.data.data;
  const items: IDetailedProductOffer[] = Array.isArray(payload)
    ? payload
    : Array.isArray(payload?.records)
      ? payload.records
      : Array.isArray(payload?.productOffers)
        ? payload.productOffers
        : [];

  if (items.length === 0) {
    return <div>{t('common.noProductsFound')}</div>;
  }

  return (
    <div className="mx-auto px-4 max-w-7xl">
      <SupplierProductsCarousel items={items} Loading={false} />
    </div>
  );
}
