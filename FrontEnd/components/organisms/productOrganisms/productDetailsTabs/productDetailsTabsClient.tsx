'use client';

import {
  ReactNode,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

type TabKey = 'desc' | 'specs' | 'comments';

export default function ProductDetailsTabsClient({
  children,
}: {
  children: ReactNode[];
}) {
  const [active, setActive] = useState<TabKey>('desc');
  const t = useTranslations();
 
  return (
    <>
      {/* Tabs Header */}
      <div className="flex gap-6 mb-6 border-b overflow-x-auto text-sm">
        <TabButton
          label={t('product.description')}
          active={active === 'desc'}
          onClick={() => setActive('desc')}
        />
        <TabButton
          label={t('product.specs')}
          active={active === 'specs'}
          onClick={() => setActive('specs')}
        />
        <TabButton
          label={t('product.userReviews')}
          active={active === 'comments'}
          onClick={() => setActive('comments')}
        />
      </div>

      {/* Content (SEO safe) */}
      <div>
        <div className={active === 'desc' ? 'block' : 'hidden'}>
          {children[0]}
        </div>
        <div className={active === 'specs' ? 'block' : 'hidden'}>
          {children[1]}
        </div>
        <div className={active === 'comments' ? 'block' : 'hidden'}>
          {children[2]}
        </div>
      </div>
    </>
  );
}

function TabButton({
  label,
  active,
  onClick,
}: {
  label: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={`pb-3 whitespace-nowrap transition ${
        active
          ? 'border-b-2 border-primary font-medium'
          : 'text-gray-500'
      }`}
      aria-selected={active}
    >
      {label}
    </button>
  );
}
