import React from 'react';

import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

import FaqAccordion from '@components/molecules/storefront/FaqAccordion';
import { getFaqs } from '@lib/faq';

/** How many questions the landing page previews before linking to /faq. */
const LANDING_FAQ_LIMIT = 6;

/** Landing-page FAQ preview. Renders nothing until at least one FAQ exists. */
export default async function LandingFaq({ locale }: { locale: string }) {
  const faqs = await getFaqs(LANDING_FAQ_LIMIT);
  if (!faqs.length) return null;

  const t = await getTranslations({ locale, namespace: 'faqPage' });

  return (
    <section className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20">
      <div className="mx-auto w-full max-w-6xl">
        <h2 className="mb-3 font-normal text-2xl sm:text-3xl md:text-4xl text-center tracking-tight">
          {t('title')}
        </h2>
        <p className="mb-8 md:mb-12 text-store-subtle text-sm sm:text-base text-center">
          {t('description')}
        </p>

        <FaqAccordion items={faqs} />

        <div className="flex justify-center mt-8">
          <Link href={`/${locale}/faq`} className="inline-flex justify-center items-center bg-primary hover:bg-transparent px-8 py-3 border border-primary font-medium text-primary-foreground hover:text-store-text text-sm transition-colors">
            {t('viewAll')}
          </Link>
        </div>
      </div>
    </section>
  );
}
