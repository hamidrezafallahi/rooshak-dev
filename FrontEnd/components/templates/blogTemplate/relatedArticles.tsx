import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';

export async function RelatedArticles() {
  const locale = await getLocale();
  const t = await getTranslations({ locale, namespace: 'blog' });

  return (
    <section className="py-14 md:py-20" aria-labelledby="related-articles-title">
      <div className="mx-auto px-4 sm:px-6 pt-10 border-t border-store-border max-w-6xl">
        <h2
          id="related-articles-title"
          className="mb-8 font-normal text-2xl sm:text-3xl tracking-tight"
        >
          {t('title')}
        </h2>
        <p className="text-store-subtle text-sm">
          {t('emptyHint')}
        </p>
      </div>
    </section>
  );
}
