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
      <div className="flex sm:flex-row flex-col flex-wrap sm:justify-between sm:items-center gap-4 mb-8">
        <div className="flex items-center gap-3">
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
        <div className="flex items-center gap-3">
          <Link href={`/${locale}/products`} className="text-sm underline">
            {t('common.viewAllProducts')}
          </Link>
        </div>
      </div>

      <div>
        <ProductsCarousel items={items} Loading={false} />
      </div>
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
      onClick={onClick}
      className={`px-4 py-2 rounded-full text-sm font-medium transition-all ${
        active
          ? 'bg-primary text-white shadow'
          : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
      }`}
      aria-pressed={active}
    >
      {children}
    </button>
  );
}
