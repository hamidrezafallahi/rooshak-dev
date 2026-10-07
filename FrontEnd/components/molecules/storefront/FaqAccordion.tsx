import React from 'react';

import JsonLd from '@components/molecules/storefront/JsonLd';

export type FaqAccordionItem = {
  id: number | string;
  question: string;
  answer: string;
};

type Props = {
  items: FaqAccordionItem[];
  /** Emit schema.org FAQPage JSON-LD (use on one page only to avoid duplicates). */
  withJsonLd?: boolean;
  className?: string;
};

/**
 * Theme-aware FAQ list (question/answer) built on native <details>,
 * so it works without client-side JavaScript and stays SSG/ISR friendly.
 */
export default function FaqAccordion({
  items,
  withJsonLd = false,
  className = '',
}: Props) {
  if (!items.length) return null;

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
    <div className={`grid grid-cols-1 lg:grid-cols-2 lg:gap-x-14 ${className}`}>
      {withJsonLd ? <JsonLd data={faqLd} /> : null}
      {items.map((item) => (
        <details
          key={item.id}
          className="group py-4 sm:py-5 text-start border-b border-store-border first:border-t lg:[&:nth-child(2)]:border-t self-start"
        >
          <summary className="flex justify-between items-center gap-3 font-medium text-sm sm:text-base list-none cursor-pointer marker:content-none [&::-webkit-details-marker]:hidden">
            <span>{item.question}</span>
            <span
              aria-hidden
              className="text-store-text text-xl leading-none transition-transform group-open:rotate-45 shrink-0"
            >
              +
            </span>
          </summary>
          <p className="mt-3 text-store-subtle text-sm leading-relaxed whitespace-pre-line">
            {item.answer}
          </p>
        </details>
      ))}
    </div>
  );
}
