import React from 'react';

import { useTranslations } from 'next-intl';

import { StarIcon } from '@components/atoms/iconComponents';

const ITEM_KEYS = ['1', '2', '3'] as const;

const TestimonialsSection: React.FC = () => {
  const t = useTranslations('testimonials');
  const tCommon = useTranslations('common');

  return (
    <section className="bg-store-muted">
      <div className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 max-w-[1440px]">
        <h2 className="mb-10 md:mb-14 font-normal text-2xl sm:text-3xl md:text-4xl text-center tracking-tight">
          {t('sectionTitle')}
        </h2>

        <div className="gap-6 md:gap-10 grid sm:grid-cols-2 lg:grid-cols-3">
          {ITEM_KEYS.map((id) => (
            <figure key={id} className="flex flex-col items-center gap-4 text-center">
              <div
                className="flex items-center gap-0.5 text-store-text"
                role="img"
                aria-label={tCommon('fiveOfFive')}
              >
                {Array.from({ length: 5 }).map((_, i) => (
                  <span key={i} aria-hidden>
                    <StarIcon />
                  </span>
                ))}
              </div>
              <blockquote className="max-w-sm text-sm sm:text-base leading-relaxed">
                {t(`items.${id}.comment`)}
              </blockquote>
              <figcaption className="text-xs">
                <span className="block font-medium text-sm">{t(`items.${id}.name`)}</span>
                <span className="text-store-subtle">{t(`items.${id}.product`)}</span>
              </figcaption>
            </figure>
          ))}
        </div>
      </div>
    </section>
  );
};

export default TestimonialsSection;
