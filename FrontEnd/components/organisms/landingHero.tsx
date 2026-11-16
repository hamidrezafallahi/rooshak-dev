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

/** Internal paths get the locale prefix; full http(s) URLs are used as they are. */
const toHref = (locale: string, url?: string) => {
  const raw = url?.trim();
  if (!raw) return null;
  if (/^https?:\/\//i.test(raw)) return raw;
  const clean = raw.replace(/^\/+/, '');
  return clean ? `/${locale}/${clean}` : null;
};

export default async function LandingHero({ slide }: Props) {
  const locale = await getLocale();
  const t = await getTranslations('homePage');

  const hasSlide = Boolean(slide?.bannerUrl);
  const desktop = {
    image: hasSlide ? toMediaUrl(slide?.bannerUrl) : DEFAULT_IMAGE,
    video: slide?.videoUrl ? toMediaUrl(slide.videoUrl) : undefined,
  };
  // Phone assets are optional; HeroMedia falls back to the desktop ones per part.
  const mobile = {
    image: slide?.mobileBannerUrl ? toMediaUrl(slide.mobileBannerUrl) : undefined,
    video: slide?.mobileVideoUrl ? toMediaUrl(slide.mobileVideoUrl) : undefined,
  };

  // Title and description are optional in the admin: empty means not shown at all.
  const title = slide?.bannerTitle?.trim() ?? '';
  const description = slide?.bannerDescription?.trim() ?? '';

  // The banner itself (image or video) links to the first URL.
  const bannerHref = toHref(locale, slide?.firstUrl) ?? (hasSlide ? null : `/${locale}/products`);
  // The optional second URL shows as a text link, in the visitor's language.
  const secondHref = toHref(locale, slide?.secondUrl);
  const secondText = (
    locale === 'en'
      ? slide?.secondUrlTextEn || slide?.secondUrlTextFa
      : slide?.secondUrlTextFa || slide?.secondUrlTextEn
  )?.trim();
  const showSecond = Boolean(secondHref && secondText);
  const hasText = Boolean(title || description || showSecond);
  // Admin «شفافیت عنوان» (0..100). At 0 the title and description stay in the HTML
  // (H1 and text for search engines and screen readers) but are not visible.
  const opacity = Math.min(100, Math.max(0, slide?.titleOpacity ?? 100)) / 100;

  return (
    <section
      className="relative bg-black min-h-[34rem] h-svh max-h-[58rem] overflow-hidden text-white"
      aria-labelledby="home-hero-title"
    >
      <HeroMedia
        desktop={desktop}
        mobile={mobile}
        alt={title || t('heroImageAlt')}
        playLabel={t('videoPlay')}
        pauseLabel={t('videoPause')}
      />

      {bannerHref ? (
        // Covers the whole banner; the play/pause button and the second link sit above it.
        <Link
          href={bannerHref}
          aria-label={title || t('heroLinkAria')}
          className="z-[5] absolute inset-0"
        />
      ) : null}

      {/* The page keeps one H1 for search engines even when the banner has no title. */}
      {!title ? (
        <h1 id="home-hero-title" className="sr-only">
          {t('title')}
        </h1>
      ) : null}

      {hasText ? (
        <div className="z-10 relative flex flex-col justify-end items-start gap-5 mx-auto px-4 sm:px-8 lg:px-16 pt-[calc(var(--store-header-h)+2rem)] pb-16 md:pb-24 max-w-[1600px] h-full pointer-events-none">
          {title || description ? (
            <div className="flex flex-col items-start gap-5" style={opacity < 1 ? { opacity } : undefined}>
              {title ? (
                <h1
                  id="home-hero-title"
                  className="max-w-3xl font-normal text-3xl sm:text-5xl lg:text-6xl leading-tight"
                >
                  {title}
                </h1>
              ) : null}
              {description ? (
                <p className="max-w-xl text-white/90 text-base sm:text-lg leading-relaxed">{description}</p>
              ) : null}
            </div>
          ) : null}
          {showSecond ? (
            <Link
              href={secondHref!}
              className="pb-0.5 border-white border-b font-medium text-sm sm:text-base pointer-events-auto"
            >
              {secondText}
            </Link>
          ) : null}
        </div>
      ) : null}
    </section>
  );
}
