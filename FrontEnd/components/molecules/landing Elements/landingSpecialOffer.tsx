// app/components/landing/LandingSpecialOffer.tsx
import React from 'react';

import { getTranslations } from 'next-intl/server';

import { getAll } from '@lib/getAll';
import { SpecialOffer } from '@models/specialOffer';

import SpecialOfferCarouselClient from './SpecialOfferCarouselClient';

export default async  function LandingSpecialOffer() {
  const t = await getTranslations('landing');
  const spacialOffers = await getAll<SpecialOffer>("SpecialOffers/landing");
  return (
    <section className="mx-auto px-4 py-16 w-full max-w-7xl">
      <div
        className="relative flex md:flex-row flex-col items-center gap-8 p-8 sm:p-12 rounded-3xl overflow-hidden text-white"
        style={{
          background:
            "linear-gradient(90deg, var(--primary-color), color-mix(in srgb, var(--secondary-color) 75%, var(--primary-color)))",
        }}
      >
        <div className="flex-1 min-w-0">
          <h2 className="mb-3 font-extrabold text-3xl sm:text-4xl">
            {t('specialOfferTitle')}
          </h2>
          <p className="mb-6 max-w-md text-sm sm:text-base text-center text-white">
            {t('specialOfferDesc')}
          </p>


          
        </div>

        <div className="flex flex-1 justify-center w-2/3">
          {/* اندازه این قاب به گونه‌ای انتخاب شده که جای یک تصویر قبلی را بگیرد */}
          <div className="w-full h-80">
            <SpecialOfferCarouselClient spacialOffers={spacialOffers?.data.records||[]} />
          </div>
        </div>
      </div>
    </section>
  );
}
