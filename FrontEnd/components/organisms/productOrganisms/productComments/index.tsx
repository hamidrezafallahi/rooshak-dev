"use client";

import {
  useLocale,
  useTranslations,
} from 'next-intl';

import { IComment } from '@models/comment';

export default function ProductComments({
  comments,
}: {
  comments: IComment[];
}) {
  const locale = useLocale();
  const t = useTranslations();
  return (
    <section className="bg-white shadow-lg p-4 rounded-2xl">
      <h3 className="mb-2 font-semibold text-lg">{t('product.userReviews')}</h3>

      {comments.map((c: IComment) => (
        <article key={c.id} className="mb-2 pb-2 border-b">
          <p className="font-medium text-sm">{c.userFullName}</p>
          <p className="text-gray-700 text-sm">{c.content}</p>
          <time className="text-gray-400 text-xs">
            {Intl.DateTimeFormat(locale === "fa" ? "fa-IR" : "en-US", {
              dateStyle: "full",
            }).format(new Date(c.createdAt))}
          </time>
        </article>
      ))}
    </section>
  );
}
