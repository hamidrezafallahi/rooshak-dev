import 'server-only';

import { IFaq } from '@models/faq';

import { getAll } from './getAll';

/**
 * Active FAQs ordered by displayOrder (ISR: 60s, tag `faqs`).
 * Returns an empty list when the API is unreachable (e.g. during image build).
 */
export async function getFaqs(limit = 100): Promise<IFaq[]> {
  const res = await getAll<IFaq>('faqs', {
    page: 1,
    pageSize: limit,
    byConfig: false,
    onlyActives: true,
  });

  if (!res?.isSuccess) return [];
  // Never render blank accordions: drop rows without a question or an answer.
  return (res.data?.records ?? []).filter(
    (faq) => faq.question?.trim() && faq.answer?.trim(),
  );
}
