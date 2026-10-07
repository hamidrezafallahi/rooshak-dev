import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { getAll } from '@lib/getAll';
import { IBlog } from '@models/Blog';

import SectionHeading from '../storefront/SectionHeading';

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
    <section className="mx-auto px-4 sm:px-6 lg:px-10 py-14 md:py-20 max-w-[1440px]">
      <SectionHeading
        title={t('landingTitle')}
        subtitle={t('landingSubtitle')}
        href={`/${locale}/blog`}
        linkLabel={t('viewAll')}
      />

      <div className="gap-x-6 gap-y-10 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
        {posts.map((post) => {
          const title = locale === 'fa' ? post.titleFa : post.titleEn;
          const excerpt = locale === 'fa' ? post.excerptFa : post.excerptEn;
          const dateLabel = formatBlogDate(post.updatedAt || post.createdAt, locale);

          return (
            <Link
              key={post.slug}
              href={`/${locale}/blog/${post.slug}`}
              className="group flex flex-col"
            >
              <div className="relative bg-store-muted aspect-[4/3] overflow-hidden">
                <MediaImage
                  src={post.thumbnailFile}
                  alt={title || post.slug}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform duration-700"
                  sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
                  loading="lazy"
                />
              </div>

              <div className="flex flex-col flex-grow gap-2 pt-4">
                {dateLabel ? (
                  <time className="text-store-subtle text-xs">{dateLabel}</time>
                ) : null}
                <h3 className="font-normal text-lg sm:text-xl line-clamp-2 group-hover:underline underline-offset-4">
                  {title}
                </h3>
                {excerpt ? (
                  <p className="text-store-subtle text-sm line-clamp-3">{excerpt}</p>
                ) : null}
              </div>
            </Link>
          );
        })}
      </div>
    </section>
  );
}
