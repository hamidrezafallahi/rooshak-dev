import { getTranslations } from 'next-intl/server';

import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';
import {
  EnumTargetType,
  IComment,
} from '@models/comment';

import ProductCommentForm from './productCommentForm';

interface ProductCommentsProps {
  id: number;
  locale?: string;
}

export default async function ProductComments({ id, locale }: ProductCommentsProps) {
  const t = await getTranslations();
  if (!id) {
    return (
      <p className="text-store-subtle text-sm">{t('product.noComments')}</p>
    );
  }

  const result = await safeFetchJson<SimpleResponse<IComment[]>>(
    `${serverApiBaseUrl}/Comments/${EnumTargetType.Product}/${id}`,
    { next: { revalidate: 36 } },
  );
  const comments =
    result.ok && result.data?.isSuccess !== false ? result.data?.data || [] : [];

  return (
    <section className="space-y-4">
      {comments.length === 0 && (
        <p className="text-store-subtle text-sm">{t('product.noComments')}</p>
      )}

      {comments.map((c: IComment) => (
        <article key={c.id} className="pb-3 border-b border-store-border text-sm">
          <p className="font-medium">{c.userFullName}</p>
          <p className="mt-1 text-store-text">{c.content}</p>
          <time className="text-store-subtle text-xs">
            {Intl.DateTimeFormat(locale === 'fa' ? 'fa-IR' : 'en-US', {
              dateStyle: 'medium',
            }).format(new Date(c.createdAt))}
          </time>
        </article>
      ))}
      <ProductCommentForm id={id} />
    </section>
  );
}
