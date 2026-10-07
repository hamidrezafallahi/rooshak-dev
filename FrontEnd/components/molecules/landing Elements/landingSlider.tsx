import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import type { LandingSlide } from '@lib/landing';

interface IProps {
  slides: LandingSlide[];
}

/**
 * Editorial banner tiles fed by the admin "landing slides" (every active slide that is
 * not used as the hero). Pure server markup — no carousel JS, no layout shift.
 */
export default async function LandingSlider({ slides }: IProps) {
  if (!slides.length) return null;
  const locale = await getLocale();
  const t = await getTranslations('landing');
  const single = slides.length === 1;

  return (
    <section
      aria-label={t('bannersAria')}
      className={`grid gap-1 sm:gap-2 mx-auto px-0 sm:px-2 max-w-[1920px] ${
        single ? 'grid-cols-1' : 'grid-cols-1 md:grid-cols-2'
      }`}
    >
      {slides.map((item, index) => {
        const href = item.firstUrl?.trim()
          ? `/${locale}/${item.firstUrl.replace(/^\/+/, '')}`
          : `/${locale}`;
        const label = item.bannerTitle?.trim();

        return (
          <Link
            key={`${item.bannerUrl}-${index}`}
            href={href}
            aria-label={label || t('slideDetailsAria', { index: index + 1 })}
            className={`group relative block overflow-hidden bg-store-muted ${
              single ? 'aspect-[4/5] sm:aspect-[21/9]' : 'aspect-[4/5] sm:aspect-[4/3]'
            }`}
          >
            <MediaImage
              src={item.bannerUrl}
              alt={label || ''}
              fill
              className="object-cover group-hover:scale-105 transition-transform duration-[1200ms]"
              sizes={single ? '100vw' : '(max-width: 768px) 100vw, 50vw'}
              priority={false}
              loading="lazy"
            />
            <div className="absolute inset-0 bg-gradient-to-t from-black/55 via-transparent to-transparent" />
            <div className="absolute inset-x-0 bottom-0 flex flex-col items-start gap-3 p-6 sm:p-10 text-white">
              {label ? (
                <h2 className="font-normal text-2xl sm:text-4xl leading-tight">{label}</h2>
              ) : null}
              {item.bannerDescription?.trim() ? (
                <p className="max-w-md text-white/90 text-sm sm:text-base">
                  {item.bannerDescription}
                </p>
              ) : null}
              <span className="pb-0.5 border-white border-b font-medium text-sm">
                {t('discover')}
              </span>
            </div>
          </Link>
        );
      })}
    </section>
  );
}
