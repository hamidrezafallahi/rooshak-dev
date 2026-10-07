import React from 'react';

import { getLocale, getTranslations } from 'next-intl/server';
import Link from 'next/link';

import {
  ArrowLongLeft,
  ArrowLongRight,
  CalendarIcon,
  UserIcon,
} from '@components/atoms/iconComponents';
import MediaImage from '@components/atoms/MediaImage';
import { IBlog } from '@models/Blog';

function formatBlogDate(value: Date | string | null | undefined, locale: string) {
  if (!value) return null;

  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return null;

  return new Intl.DateTimeFormat(locale === 'fa' ? 'fa-IR' : 'en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(date);
}

export async function BlogCard({ blog }: { blog: IBlog }) {
  const locale = await getLocale();
  const t = await getTranslations('blog');
  const isRtl = locale === 'fa';

  const title = (isRtl ? blog.titleFa : blog.titleEn) || blog.titleFa || blog.titleEn;
  const excerpt =
    (isRtl ? blog.excerptFa : blog.excerptEn) ||
    blog.excerptFa ||
    blog.excerptEn ||
    '';
  const dateLabel = formatBlogDate(blog.updatedAt || blog.createdAt, locale);
  const hasThumbnail = Boolean(blog.thumbnailFile?.trim());

  return (
    <Link
      href={`/${locale}/blog/${blog.slug}`}
      className="group flex flex-col h-full focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-store-strong focus-visible:ring-offset-2"
    >
      <div className="relative aspect-[4/3] w-full overflow-hidden bg-store-muted">
        {hasThumbnail ? (
          <MediaImage
            src={blog.thumbnailFile}
            alt={title}
            fill
            sizes="(max-width: 640px) 100vw, (max-width: 1024px) 50vw, 33vw"
            className="object-cover transition-transform duration-500 ease-out group-hover:scale-[1.04]"
          />
        ) : (
          <div className="absolute inset-0 flex items-center justify-center text-sm text-store-subtle">
            {t('title')}
          </div>
        )}
      </div>

      <div
        className={`flex flex-1 flex-col gap-2 pt-4 ${
          isRtl ? 'text-right' : 'text-left'
        }`}
      >
        {dateLabel ? (
          <div className="inline-flex items-center gap-1.5 text-xs text-store-subtle">
            <CalendarIcon />
            <time dateTime={String(blog.updatedAt || blog.createdAt)}>
              {dateLabel}
            </time>
          </div>
        ) : null}

        <h2 className="text-lg sm:text-xl font-normal leading-snug text-store-text underline-offset-4 group-hover:underline line-clamp-2">
          {title}
        </h2>

        {excerpt ? (
          <p className="text-sm leading-relaxed text-store-subtle line-clamp-3">
            {excerpt}
          </p>
        ) : null}

        <div className="mt-auto flex items-center justify-between gap-3 pt-3">
          {blog.authorName ? (
            <div className="inline-flex min-w-0 items-center gap-1.5 text-xs text-store-subtle">
              <UserIcon />
              <span className="truncate">{blog.authorName}</span>
            </div>
          ) : (
            <span />
          )}

          <span className="inline-flex shrink-0 items-center gap-1 text-sm font-medium text-store-text">
            {t('readMore')}
            {isRtl ? (
              <ArrowLongLeft
                config={{ className: 'w-4 h-4 transition-transform group-hover:-translate-x-0.5' }}
              />
            ) : (
              <ArrowLongRight
                config={{ className: 'w-4 h-4 transition-transform group-hover:translate-x-0.5' }}
              />
            )}
          </span>
        </div>
      </div>
    </Link>
  );
}
