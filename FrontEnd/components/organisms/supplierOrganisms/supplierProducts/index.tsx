import React from 'react';

import { getTranslations } from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';
import { IDetailedProductOffer } from '@models/product';

import SupplierProductsCarousel from './supplierProductsCarousel';

export async function SupplierProducts(props: {
  id: number;
}) {
  const { id } = props;
  const t = await getTranslations();

  try {
    const response = await fetch(
      `${serverApiBaseUrl}/productOffers/by-seller/${id}`,
      {
        next: { revalidate: 36 },
      },
    );

    if (!response.ok) {
      return <div>{t('common.noProductsFound')}</div>;
    }

    const res = await response.json();
    const payload = res?.data;
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
  } catch {
    return <div>{t('common.noProductsFound')}</div>;
  }
}
