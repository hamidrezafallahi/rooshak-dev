import { getTranslations } from 'next-intl/server';

import JsonLd from '@components/molecules/storefront/JsonLd';

export type FaqItem = {
  question: string;
  answer: string;
};

type Props = {
  items: FaqItem[];
  locale: string;
  title?: string;
};

export function parseFaqJson(raw?: string | null): FaqItem[] {
  if (!raw?.trim()) return [];
  try {
    const parsed = JSON.parse(raw);
    if (!Array.isArray(parsed)) return [];
    return parsed
      .map((item) => ({
        question: String(item?.question ?? item?.q ?? '').trim(),
        answer: String(item?.answer ?? item?.a ?? '').trim(),
      }))
      .filter((item) => item.question && item.answer);
  } catch {
    return [];
  }
}

export default async function FaqSection({ items, locale, title }: Props) {
  if (!items.length) return null;

  const t = await getTranslations('storefront');

  const faqLd = {
    '@context': 'https://schema.org',
    '@type': 'FAQPage',
    mainEntity: items.map((item) => ({
      '@type': 'Question',
      name: item.question,
      acceptedAnswer: {
        '@type': 'Answer',
        text: item.answer,
      },
    })),
  };

  return (
    <section className="mt-10 border-t border-store-border pt-8">
      <JsonLd data={faqLd} />
      <h2 className="mb-5 text-xl sm:text-2xl font-normal text-store-text">
        {title || t('faqTitle')}
      </h2>
      <div className="space-y-3">
        {items.map((item) => (
          <details
            key={item.question}
            className="group border-b border-store-border py-4"
          >
            <summary className="cursor-pointer list-none font-medium text-store-text marker:content-none [&::-webkit-details-marker]:hidden">
              {item.question}
            </summary>
            <p className="mt-3 text-sm leading-relaxed text-store-subtle">{item.answer}</p>
          </details>
        ))}
      </div>
    </section>
  );
}
