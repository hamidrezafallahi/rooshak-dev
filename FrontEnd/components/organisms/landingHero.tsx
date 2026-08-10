import { getLocale, getTranslations } from 'next-intl/server';
import Image from 'next/image';
import Link from 'next/link';

export default async function LandingHero() {
  const locale = await getLocale();
  const t = await getTranslations('homePage');

  return (
    <section className="store-hero" aria-labelledby="home-hero-title">
      <div className="store-hero-inner">
        <div className="z-10 flex flex-col items-center md:items-start gap-5 max-w-xl text-center md:text-start">
          <h1
            id="home-hero-title"
            className="font-extrabold text-3xl sm:text-4xl md:text-5xl lg:text-6xl leading-tight text-white"
          >
            {t('title')}
          </h1>
          <p className="max-w-lg text-white text-base sm:text-lg md:text-xl leading-relaxed">
            {t('subtitle')}
          </p>
          <div className="flex sm:flex-row flex-col gap-3 mt-2 w-full sm:w-auto">
            <Link
              href={`/${locale}/products`}
              className="store-btn store-btn-primary px-7 py-3 text-base"
            >
              {t('ctaProducts')}
            </Link>
            <Link
              href={`/${locale}/discounts`}
              className="store-btn store-btn-ghost px-7 py-3 text-base"
            >
              {t('ctaDiscounts')}
            </Link>
          </div>
        </div>

        <div className="relative shrink-0">
          <div className="relative shadow-2xl rounded-2xl w-[240px] sm:w-[280px] md:w-[380px] h-[320px] sm:h-[380px] md:h-[480px] overflow-hidden ring-1 ring-white/20">
            <Image
              src="/images/landingPage/11.jpg"
              alt={t('heroImageAlt')}
              fill
              className="object-cover hover:scale-105 transition-transform duration-700"
              priority
              fetchPriority="high"
              quality={70}
              sizes="(max-width: 640px) 240px, (max-width: 768px) 280px, 380px"
            />
          </div>
          <div
            className="-top-8 -end-8 absolute opacity-50 blur-3xl rounded-full w-36 h-36"
            style={{
              background:
                'color-mix(in srgb, var(--secondary-color) 70%, transparent)',
            }}
            aria-hidden
          />
        </div>
      </div>
    </section>
  );
}
