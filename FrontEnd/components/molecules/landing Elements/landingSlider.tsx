import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import type { HomeSlide } from '@lib/landing';

interface IProps {
  slides: HomeSlide[];
}

const toHref = (locale: string, url?: string) => {
  const raw = url?.trim();
  if (!raw) return null;
  if (/^https?:\/\//i.test(raw)) return raw;
  return `/${locale}/${raw.replace(/^\/+/, '')}`;
};

/**
 * Editorial banner tiles fed by the admin «اسلایدر صفحه اصلی» (HomeSlides table).
 * Fully separate from the hero banner: its own table, API and images.
 * Pure server markup — no carousel JS, no layout shift.
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
        const href = toHref(locale, item.linkUrl);
        const label = (locale === 'en' ? item.titleEn || item.titleFa : item.titleFa || item.titleEn)?.trim();
        const tileClass = `group relative block overflow-hidden bg-store-muted ${
          single ? 'aspect-[4/5] sm:aspect-[21/9]' : 'aspect-[4/5] sm:aspect-[4/3]'
        }`;
        const sizes = single ? '100vw' : '(max-width: 768px) 100vw, 50vw';

        const body = (
          <>
            {item.mobileImageUrl ? (
              <>
                <MediaImage src={item.mobileImageUrl} alt={label || ''} fill sizes={sizes} loading="lazy"
                  className="sm:hidden object-cover group-hover:scale-105 transition-transform duration-[1200ms]" />
                <MediaImage src={item.imageUrl} alt={label || ''} fill sizes={sizes} loading="lazy"
                  className="hidden sm:block object-cover group-hover:scale-105 transition-transform duration-[1200ms]" />
              </>
            ) : (
              <MediaImage src={item.imageUrl} alt={label || ''} fill sizes={sizes} loading="lazy"
                className="object-cover group-hover:scale-105 transition-transform duration-[1200ms]" />
            )}
            {label || href ? (
              <>
                <div className="absolute inset-0 bg-gradient-to-t from-black/55 via-transparent to-transparent" />
                <div className="absolute inset-x-0 bottom-0 flex flex-col items-start gap-3 p-6 sm:p-10 text-white">
                  {label ? (
                    <h2 className="font-normal text-2xl sm:text-4xl leading-tight">{label}</h2>
                  ) : null}
                  {href ? (
                    <span className="pb-0.5 border-white border-b font-medium text-sm">{t('discover')}</span>
                  ) : null}
                </div>
              </>
            ) : null}
          </>
        );

        return href ? (
          <Link
            key={item.id}
            href={href}
            aria-label={label || t('slideDetailsAria', { index: index + 1 })}
            className={tileClass}
          >
            {body}
          </Link>
        ) : (
          <div key={item.id} className={tileClass}>
            {body}
          </div>
        );
      })}
    </section>
  );
}
