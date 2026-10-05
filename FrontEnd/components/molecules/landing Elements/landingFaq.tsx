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
    <section className="bg-white px-4 sm:px-6 lg:px-8 py-16">
      <div className="mx-auto w-full max-w-3xl">
        <h2 className="mb-3 font-bold text-2xl sm:text-3xl text-center">
          {t('title')}
        </h2>
        <p className="mb-8 text-gray-600 text-sm sm:text-base text-center">
          {t('description')}
        </p>

        <FaqAccordion items={faqs} />

        <div className="flex justify-center mt-8">
          <Link href={`/${locale}/faq`} className="store-btn store-btn-primary">
            {t('viewAll')}
          </Link>
        </div>
      </div>
    </section>
  );
}
