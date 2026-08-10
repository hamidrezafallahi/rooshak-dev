import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import {
  CalendarIcon,
  UserIcon,
} from '@components/atoms/iconComponents';
import MediaImage from '@components/atoms/MediaImage';
import { getAll } from '@lib/getAll';
import { IBlog } from '@models/Blog';

function formatBlogDate(value: Date | string | null | undefined, locale: string) {
  if (!value) return null;

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return null;

  return new Intl.DateTimeFormat(locale === 'fa' ? 'fa-IR' : 'en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  }).format(date);
}

export default async function BlogSection() {
  const locale = await getLocale();
  const t = await getTranslations('blog');

  const response = await getAll<IBlog>('blogs', {
    page: 1,
    pageSize: 3,
    byConfig: false,
    onlyActives: true,
  });

  const posts = response?.data?.records ?? [];

  if (posts.length === 0) {
    return null;
  }

  return (
    <section className="bg-white py-16">
      <div className="mx-auto px-4 container">
        <div className="mb-10 text-center">
          <h2 className="mb-3 font-bold text-2xl sm:text-3xl">
            {t('landingTitle')}
          </h2>
          <p className="text-gray-600 text-sm sm:text-base">
            {t('landingSubtitle')}
          </p>
          <Link
            href={`/${locale}/blog`}
            className="inline-block mt-4 text-primary text-sm underline"
          >
            {t('viewAll')}
          </Link>
        </div>

        <div className="gap-8 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
          {posts.map((post) => {
            const title = locale === 'fa' ? post.titleFa : post.titleEn;
            const excerpt = locale === 'fa' ? post.excerptFa : post.excerptEn;
            const dateLabel = formatBlogDate(
              post.updatedAt || post.createdAt,
              locale,
            );

            return (
              <Link
                key={post.slug}
                href={`/${locale}/blog/${post.slug}`}
                className="group flex flex-col bg-gray-50 hover:shadow-lg rounded-2xl overflow-hidden transition"
              >
                <div className="relative w-full h-56 bg-gray-200">
                  <MediaImage
                    src={post.thumbnailFile}
                    alt={title || post.slug}
                    fill
                    className="object-cover group-hover:scale-105 transition-transform duration-300"
                    sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                  />
                </div>

                <div
                  className={`flex flex-col flex-grow p-5 ${
                    locale === 'fa' ? 'text-right' : 'text-left'
                  }`}
                >
                  <h3 className="mb-2 font-semibold group-hover:text-primary text-lg transition line-clamp-2">
                    {title}
                  </h3>
                  {excerpt ? (
                    <p className="flex-grow text-gray-600 text-sm line-clamp-3">
                      {excerpt}
                    </p>
                  ) : null}

                  <div className="flex justify-between items-center gap-3 mt-4 text-gray-600 text-xs">
                    {post.authorName ? (
                      <div className="flex items-center gap-1 min-w-0">
                        <UserIcon />
                        <span className="truncate">{post.authorName}</span>
                      </div>
                    ) : (
                      <span />
                    )}
                    {dateLabel ? (
                      <div className="flex items-center gap-1 shrink-0">
                        <CalendarIcon />
                        <span>{dateLabel}</span>
                      </div>
                    ) : null}
                  </div>
                </div>
              </Link>
            );
          })}
        </div>
      </div>
    </section>
  );
}
