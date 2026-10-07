import React from 'react';

import { useTranslations } from 'next-intl';

import {
  BadgeCheckIcon,
  CashIcon,
  PhoneIcon,
  ShieldCheckIcon,
  TruckIcon,
} from '@components/atoms/iconComponents';

const USP_KEYS = [
  { id: 'shipping', icon: <TruckIcon /> },
  { id: 'support', icon: <PhoneIcon /> },
  { id: 'cod', icon: <CashIcon /> },
  { id: 'warranty', icon: <ShieldCheckIcon /> },
  { id: 'authenticity', icon: <BadgeCheckIcon /> },
] as const;

const USPSection: React.FC = () => {
  const t = useTranslations('usp');

  return (
    <section
      aria-labelledby="usp-title"
      className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 max-w-[1440px]"
    >
      <h2
        id="usp-title"
        className="mb-10 md:mb-14 font-normal text-2xl sm:text-3xl md:text-4xl text-center tracking-tight"
      >
        {t('sectionTitle')}
      </h2>

      <div className="gap-px grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 bg-store-border border border-store-border">
        {USP_KEYS.map((usp) => (
          <article
            key={usp.id}
            className="flex flex-col items-center gap-3 bg-store-surface p-6 sm:p-8 text-center"
          >
            <div aria-hidden className="text-store-text">
              {usp.icon}
            </div>
            <h3 className="font-medium text-base">{t(`items.${usp.id}.title`)}</h3>
            <p className="text-store-subtle text-sm leading-relaxed">
              {t(`items.${usp.id}.description`)}
            </p>
          </article>
        ))}
      </div>
    </section>
  );
};

export default USPSection;
