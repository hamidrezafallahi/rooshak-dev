import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import type { LandingSlide } from '@lib/landing';
import { toMediaUrl } from '@utils/toMediaUrl';

import HeroMedia from './heroMedia';

/** Used until an admin uploads a hero slide (image, optionally with a video). */
const DEFAULT_IMAGE = '/images/landingPage/11.jpg';

type Props = {
  /** Slide flagged as hero in the admin (Slides table). Falls back to the bundled media. */
  slide?: LandingSlide | null;
};

const internalHref = (locale: string, url?: string) => {
  const clean = url?.trim().replace(/^\/+/, '');
  return clean ? `/${locale}/${clean}` : null;
};

export default async function LandingHero({ slide }: Props) {
  const locale = await getLocale();
  const t = await getTranslations('homePage');

  const hasSlide = Boolean(slide?.bannerUrl);
  const imageSrc = hasSlide ? toMediaUrl(slide?.bannerUrl) : DEFAULT_IMAGE;
  const videoSrc = slide?.videoUrl ? toMediaUrl(slide.videoUrl) : undefined;

  const primaryHref = internalHref(locale, slide?.firstUrl) ?? `/${locale}/products`;
  const secondaryHref = internalHref(locale, slide?.secondUrl) ?? `/${locale}/discounts`;

  return (
    <section
      className="relative bg-black min-h-[34rem] h-svh max-h-[58rem] overflow-hidden text-white"
      aria-labelledby="home-hero-title"
    >
      <HeroMedia
        imageSrc={imageSrc}
        videoSrc={videoSrc}
        alt={t('heroImageAlt')}
        playLabel={t('videoPlay')}
        pauseLabel={t('videoPause')}
      />

      <div className="relative z-10 flex flex-col justify-end items-start gap-5 mx-auto px-4 sm:px-8 lg:px-16 pt-[calc(var(--store-header-h)+2rem)] pb-16 md:pb-24 max-w-[1600px] h-full">
        <p className="font-medium text-xs sm:text-sm uppercase ltr:tracking-[0.2em]">
          {slide?.bannerTitle?.trim() || t('eyebrow')}
        </p>
        <h1
          id="home-hero-title"
          className="max-w-3xl font-normal text-3xl sm:text-5xl lg:text-6xl leading-tight"
        >
          {t('title')}
        </h1>
        <p className="max-w-xl text-white/90 text-base sm:text-lg leading-relaxed">
          {slide?.bannerDescription?.trim() || t('subtitle')}
        </p>
        <div className="flex sm:flex-row flex-col gap-3 mt-2 w-full sm:w-auto">
          <Link
            href={primaryHref}
            className="inline-flex justify-center items-center bg-white hover:bg-transparent px-8 py-3.5 border border-white font-medium text-black hover:text-white text-sm transition-colors"
          >
            {t('ctaProducts')}
          </Link>
          <Link
            href={secondaryHref}
            className="inline-flex justify-center items-center hover:bg-white px-8 py-3.5 border border-white font-medium text-white hover:text-black text-sm transition-colors"
          >
            {t('ctaDiscounts')}
          </Link>
        </div>
      </div>
    </section>
  );
}
