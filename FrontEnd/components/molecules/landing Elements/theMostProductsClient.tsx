'use client';

import React, { useMemo, useState } from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import { ILandingProduct } from '@models/product';

import ProductsCarousel from '../productsCarousel';

type TabKey = 'BestSeller' | 'TheNewest' | 'Discounters';

type Props = {
  bestSeller: ILandingProduct[];
  theNewest: ILandingProduct[];
  discounters: ILandingProduct[];
};

export default function TheMostProductsClient({
  bestSeller,
  theNewest,
  discounters,
}: Props) {
  const [activeTab, setActiveTab] = useState<TabKey>('BestSeller');
  const locale = useLocale();
  const t = useTranslations();

  const tabs: { key: TabKey; label: string }[] = [
    { key: 'BestSeller', label: t('landing.bestSellers') },
    { key: 'TheNewest', label: t('landing.newest') },
    { key: 'Discounters', label: t('landing.discounters') },
  ];

  const items = useMemo(() => {
    switch (activeTab) {
      case 'TheNewest':
        return theNewest;
      case 'Discounters':
        return discounters;
      case 'BestSeller':
      default:
        return bestSeller;
    }
  }, [activeTab, bestSeller, theNewest, discounters]);

  return (
    <>
      <div className="flex sm:flex-row flex-col sm:justify-between sm:items-end gap-5 mb-8 md:mb-12">
        <div>
          <h2 className="mb-5 font-normal text-2xl sm:text-3xl md:text-4xl tracking-tight">
            {t('landing.selectedProducts')}
          </h2>
          <div role="tablist" className="flex gap-6 border-b border-store-border">
            {tabs.map((tab) => (
              <TabButton
                key={tab.key}
                active={activeTab === tab.key}
                onClick={() => setActiveTab(tab.key)}
              >
                {tab.label}
              </TabButton>
            ))}
          </div>
        </div>
        <Link
          href={`/${locale}/products`}
          className="self-start sm:self-auto pb-0.5 border-current border-b font-medium text-sm hover:opacity-60 whitespace-nowrap transition-opacity"
        >
          {t('common.viewAllProducts')}
        </Link>
      </div>

      <ProductsCarousel items={items} Loading={false} />
    </>
  );
}

function TabButton({
  children,
  active,
  onClick,
}: {
  children: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      role="tab"
      onClick={onClick}
      aria-selected={active}
      className={`-mb-px pb-3 text-sm font-medium border-b-2 transition-colors ${
        active
          ? 'border-store-strong text-store-text'
          : 'border-transparent text-store-subtle hover:text-store-text'
      }`}
    >
      {children}
    </button>
  );
}
