'use client';

import React from 'react';

import { useTranslations } from 'next-intl';

import {
  CreditCardIcon,
  LookIcon,
  ReplaceIcon,
  StarIcon,
  TruckIcon,
} from '@components/atoms/iconComponents';

const BADGE_KEYS = [
  { id: 'secure', icon: <LookIcon /> },
  { id: 'cod', icon: <CreditCardIcon /> },
  { id: 'return', icon: <ReplaceIcon /> },
  { id: 'shipping', icon: <TruckIcon /> },
] as const;

const REVIEW_KEYS = ['1', '2', '3'] as const;

const TrustSection: React.FC = () => {
  const t = useTranslations('trust');
  const tCommon = useTranslations('common');

  return (
    <section className="bg-white py-16 text-center">
      <h2 className="mb-10 font-bold text-2xl sm:text-3xl">
        {t('sectionTitle')}
      </h2>

      <div className="gap-6 grid grid-cols-2 sm:grid-cols-4 mb-16">
        {BADGE_KEYS.map((badge) => (
          <div
            key={badge.id}
            className="flex flex-col items-center bg-gray-50 hover:shadow-md p-6 rounded-2xl text-center transition"
          >
            {badge.icon}
            <h3 className="mt-3 font-semibold">{t(`badges.${badge.id}.title`)}</h3>
            <p className="mt-1 text-gray-600 text-sm">
              {t(`badges.${badge.id}.description`)}
            </p>
          </div>
        ))}
      </div>

      <div className="gap-6 grid sm:grid-cols-3 mb-16">
        {REVIEW_KEYS.map((id) => (
          <div
            key={id}
            className="bg-gray-50 hover:shadow-md p-6 rounded-2xl text-right transition"
          >
            <div className="flex items-center mb-3">
              <div
                className="flex justify-center items-center bg-[color-mix(in_srgb,var(--primary-color)_20%,white)] rounded-full w-[50px] h-[50px] font-semibold text-[var(--primary-color)] text-sm"
                aria-hidden
              >
                {t(`reviews.${id}.name`).slice(0, 1)}
              </div>
              <div className="ms-3">
                <h3 className="font-semibold">{t(`reviews.${id}.name`)}</h3>
                <div
                  className="flex text-yellow-500"
                  role="img"
                  aria-label={tCommon('fiveOfFive')}
                >
                  {Array.from({ length: 5 }).map((_, i) => (
                    <span key={i} aria-hidden>
                      <StarIcon />
                    </span>
                  ))}
                </div>
              </div>
            </div>
            <p className="text-gray-600 text-sm leading-relaxed">
              {t(`reviews.${id}.comment`)}
            </p>
          </div>
        ))}
      </div>
    </section>
  );
};

export default TrustSection;
