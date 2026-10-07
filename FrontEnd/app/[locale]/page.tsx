import { existsSync } from 'fs';
import { join } from 'path';

import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import BlogSection from '@components/molecules/landing Elements/blogSection';
import LandingFaq from '@components/molecules/landing Elements/landingFaq';
import LandingBrands
  from '@components/molecules/landing Elements/landingBrands';
import LandingCategory
  from '@components/molecules/landing Elements/landingCategory';
import LandingSlider
  from '@components/molecules/landing Elements/landingSlider';
import LandingSpecialOffer
  from '@components/molecules/landing Elements/landingSpecialOffer';
import TestimonialsSection
  from '@components/molecules/landing Elements/testimonialsSection';
import TheMostProducts
  from '@components/molecules/landing Elements/theMostProducts';
import TrustSection from '@components/molecules/landing Elements/trustSection';
import USPSection from '@components/molecules/landing Elements/uspSection';
import JsonLd from '@components/molecules/storefront/JsonLd';
import AdminDock from '@components/organisms/adminDock';
import LandingHero from '@components/organisms/landingHero';
import Footer from '@layout/footer';
import Header from '@layout/header';
import { getCategories } from '@lib/category';
import { getSlides, splitHeroSlide } from '@lib/landing';
import { siteBaseUrl } from '@lib/api';
import {
  absoluteUrl,
  buildPageMetadata,
} from '@lib/seo';

/** ISR: homepage cacheable; regenerate every 60s. */
export const revalidate = 60;

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'seo' });

  return buildPageMetadata({
    locale,
    path: '',
    title: t('homeTitle'),
    description: t('homeDescription'),
    keywords: t('homeKeywords').split(',').map((k) => k.trim()),
    // Avoid streaming metadata after </head> (Lighthouse SEO fails otherwise).
    skipSeoOverride: true,
  });
}

export default async function Home({ params }: Props) {
  const { locale } = await params;
  const tBrand = await getTranslations({ locale, namespace: 'brand' });
  const tSeo = await getTranslations({ locale, namespace: 'seo' });

  const { hero, rest } = splitHeroSlide(await getSlides());
  const categories = await getCategories({
    queries: { IsShowInLanding: true },
  });

  const organizationLd = {
    '@context': 'https://schema.org',
    '@type': 'OnlineStore',
    '@id': `${absoluteUrl(locale, '')}#organization`,
    name: 'روشاک',
    url: absoluteUrl(locale, ''),
    // Root-absolute assets (never locale-prefixed). The logo is only advertised when
    // the file exists, so structured data never points at a 404.
    ...(existsSync(join(process.cwd(), 'public', 'brand', 'rooshak-logo.png'))
      ? { logo: `${siteBaseUrl}/brand/rooshak-logo.png` }
      : {}),
    image: `${siteBaseUrl}/og-image.jpg`,
    email: 'info@rooshak.ir',
    telephone: '+98-935-4042013',
  
    sameAs: [
      'https://instagram.com/roshak_kitchenware',
      'https://t.me/Arash71tj',
      // 'https://x.com/rooshak',
    ],
  
    address: {
      '@type': 'PostalAddress',
      addressCountry: 'IR',
    },
  };

  const websiteLd = {
    '@context': 'https://schema.org',
    '@type': 'WebSite',
    '@id': `${absoluteUrl(locale, '')}#website`,
    name: tBrand('name'),
    url: absoluteUrl(locale, ''),
    description: tSeo('homeDescription'),
    inLanguage: locale,
    publisher: {
      '@id': `${absoluteUrl(locale, '')}#organization`,
    },
    potentialAction: {
      '@type': 'SearchAction',
      target: `${absoluteUrl(locale, 'products')}?q={search_term_string}`,
      'query-input': 'required name=search_term_string',
    },
  };

  return (
    <>
      <JsonLd data={[organizationLd, websiteLd]} />
      <Header overlay />
      <main className="flex flex-col min-h-screen">
        <LandingHero slide={hero} />
        <LandingSlider slides={rest} />
        <LandingBrands />
        <TheMostProducts />
        <LandingCategory categories={categories?.data.records ?? []} />
        <LandingSpecialOffer />
        <TrustSection />
        <USPSection />
        <BlogSection />
        <TestimonialsSection />
        <LandingFaq locale={locale} />
      </main>
      <AdminDock />
      <Footer />
    </>
  );
}
