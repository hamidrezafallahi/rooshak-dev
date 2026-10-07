import React from 'react';

import { getTranslations } from 'next-intl/server';

import { getAll } from '@lib/getAll';
import { SpecialOffer } from '@models/specialOffer';

import SpecialOfferCarouselClient from './SpecialOfferCarouselClient';

export default async function LandingSpecialOffer() {
  const t = await getTranslations('landing');
  const specialOffers = await getAll<SpecialOffer>('SpecialOffers/landing');
  const records = specialOffers?.data?.records ?? [];
  if (!records.length) return null;

  return (
    <section className="bg-black text-white">
      <div className="items-center gap-8 md:gap-14 grid md:grid-cols-[minmax(0,2fr)_minmax(0,3fr)] mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 max-w-[1440px]">
        <div>
          <p className="mb-3 font-medium text-white/70 text-xs uppercase ltr:tracking-[0.2em]">
            {t('specialOfferEyebrow')}
          </p>
          <h2 className="mb-4 font-normal text-3xl sm:text-4xl md:text-5xl leading-tight">
            {t('specialOfferTitle')}
          </h2>
          <p className="max-w-md text-white/80 text-sm sm:text-base leading-relaxed">
            {t('specialOfferDesc')}
          </p>
        </div>

        <div className="min-w-0 h-[22rem] sm:h-[26rem]">
          <SpecialOfferCarouselClient spacialOffers={records} />
        </div>
      </div>
    </section>
  );
}
