"use client";
import React, {
  useEffect,
  useState,
} from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import {
  ChevronLeftIcon,
  ChevronRightIcon,
  LinkIcon,
} from '@components/atoms/iconComponents';

export type LandingSlideImage = {
  bannerUrl: string;
  firstUrl: string;
};

interface IProps {
  images: LandingSlideImage[];
}

/**
 * Only mount the active slide image. Rendering every slide (even with lazy)
 * still downloads off-screen banners and tanks mobile "image delivery".
 * No Next image optimizer — nginx still serves /uploads WebP directly.
 */
function LandingSlider({ ...props }: IProps) {
  const { images } = props;
  const locale = useLocale();
  const t = useTranslations('landing');
  const [current, setCurrent] = useState(0);
  const length = images.length;

  useEffect(() => {
    if (length <= 1) return;
    const interval = setInterval(() => {
      setCurrent((prev) => (prev + 1) % length);
    }, 5000);
    return () => clearInterval(interval);
  }, [length]);

  if (length === 0) {
    return null;
  }

  const goToSlide = (index: number) => setCurrent(index);
  const prevSlide = () =>
    setCurrent((prev) => (prev === 0 ? length - 1 : prev - 1));
  const nextSlide = () => setCurrent((prev) => (prev + 1) % length);

  const item = images[current];
  const href = item.firstUrl?.trim()
    ? `/${locale}/${item.firstUrl.replace(/^\/+/, '')}`
    : `/${locale}`;

  return (
    <div className="relative w-full overflow-hidden">
      <div className="relative shadow-lg h-56 md:h-96 overflow-hidden">
        <div className="absolute inset-0 z-10">
          <MediaImage
            key={item.bannerUrl}
            src={item.bannerUrl}
            alt={t('slideDetailsAria', { index: current + 1 })}
            fill
            className="object-cover"
            priority={current === 0}
            sizes="100vw"
          />

          <div className="absolute inset-0 bg-black/40" />

          <div className="rtl:md:right-16 rtl:right-8 bottom-8 left-8 md:left-16 absolute bg-white hover:bg-gray-200 rounded-full w-10 h-10">
            <Link
              href={href}
              aria-label={t('slideDetailsAria', { index: current + 1 })}
              className="flex justify-center items-center shadow-md w-full h-full transition-all duration-200"
            >
              <LinkIcon />
            </Link>
          </div>
        </div>
      </div>

      <div className="bottom-5 left-1/2 z-40 absolute flex rtl:flex-row-reverse gap-3 -translate-x-1/2 transform">
        {images.map((_, index) => (
          <button
            key={index}
            type="button"
            className={`w-3 h-3 rounded-full transition-colors ${
              index === current ? "bg-white" : "bg-gray-500"
            }`}
            onClick={() => goToSlide(index)}
            aria-label={t('goToSlideAria', { index: index + 1 })}
            aria-current={index === current ? 'true' : undefined}
          />
        ))}
      </div>

      <button
        type="button"
        onClick={prevSlide}
        aria-label={t('prevSlide')}
        className="top-0 left-0 z-40 absolute flex justify-center items-center px-4 h-full cursor-pointer"
      >
        <span className="inline-flex justify-center items-center bg-white/30 hover:bg-white/50 rounded-full w-10 h-10">
          <ChevronLeftIcon />
        </span>
      </button>

      <button
        type="button"
        onClick={nextSlide}
        aria-label={t('nextSlide')}
        className="top-0 right-0 z-40 absolute flex justify-center items-center px-4 h-full cursor-pointer"
      >
        <span className="inline-flex justify-center items-center bg-white/30 hover:bg-white/50 rounded-full w-10 h-10">
          <ChevronRightIcon />
        </span>
      </button>
    </div>
  );
}

export default LandingSlider;
