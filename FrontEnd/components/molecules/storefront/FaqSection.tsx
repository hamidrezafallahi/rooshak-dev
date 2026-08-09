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
    <section className="mt-8 rounded-2xl border border-white/10 bg-white/5 p-5">
      <JsonLd data={faqLd} />
      <h2 className="mb-4 text-xl font-semibold text-white">
        {title || t('faqTitle')}
      </h2>
      <div className="space-y-3">
        {items.map((item) => (
          <details
            key={item.question}
            className="group rounded-xl border border-white/10 bg-black/10 px-4 py-3 open:border-amber-300/30"
          >
            <summary className="cursor-pointer list-none font-medium text-white marker:content-none">
              {item.question}
            </summary>
            <p className="mt-2 text-sm leading-relaxed text-white/75">{item.answer}</p>
          </details>
        ))}
      </div>
    </section>
  );
}
