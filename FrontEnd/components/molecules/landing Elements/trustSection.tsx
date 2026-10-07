import React from 'react';

import { useTranslations } from 'next-intl';

import {
  CreditCardIcon,
  LookIcon,
  ReplaceIcon,
  TruckIcon,
} from '@components/atoms/iconComponents';

const BADGE_KEYS = [
  { id: 'secure', icon: <LookIcon /> },
  { id: 'cod', icon: <CreditCardIcon /> },
  { id: 'return', icon: <ReplaceIcon /> },
  { id: 'shipping', icon: <TruckIcon /> },
] as const;

/** Service promises row (secure payment / COD / returns / shipping). */
const TrustSection: React.FC = () => {
  const t = useTranslations('trust');

  return (
    <section
      aria-labelledby="trust-title"
      className="border-y border-store-border"
    >
      <div className="mx-auto px-4 sm:px-6 lg:px-10 py-12 md:py-16 max-w-[1440px]">
        <h2 id="trust-title" className="sr-only">
          {t('sectionTitle')}
        </h2>
        <ul className="gap-x-6 gap-y-10 grid grid-cols-2 lg:grid-cols-4 text-center">
          {BADGE_KEYS.map((badge) => (
            <li key={badge.id} className="flex flex-col items-center gap-3">
              <span aria-hidden className="text-store-text">
                {badge.icon}
              </span>
              <h3 className="font-medium text-sm sm:text-base">
                {t(`badges.${badge.id}.title`)}
              </h3>
              <p className="max-w-[16rem] text-store-subtle text-xs sm:text-sm leading-relaxed">
                {t(`badges.${badge.id}.description`)}
              </p>
            </li>
          ))}
        </ul>
      </div>
    </section>
  );
};

export default TrustSection;
