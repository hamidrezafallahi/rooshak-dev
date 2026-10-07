import React from 'react';

import MediaImage from '@components/atoms/MediaImage';

import { IProps } from './type';

export async function HeroSection({ ...props }: IProps) {
  const { blog, locale } = props;
  const isRTL = locale == "fa";
  return (
    <section className="relative h-[60vh] max-h-[700px] overflow-hidden">
      {blog.thumbnailFile && (
        <div className="relative w-full h-full">
          <MediaImage
            src={blog.thumbnailFile}
            alt={isRTL? blog.titleFa : blog.titleEn}
            fill
            className="object-cover"
            priority
            sizes="100vw"
          />
          <div className="absolute inset-0 bg-gradient-to-t from-black/60 via-black/30 to-transparent" />
        </div>
      )}

      <div
        className={`absolute bottom-0 ${
          isRTL ? "right-0 text-right" : "left-0"
        } p-8 md:p-12 text-white max-w-4xl w-full`}
      >
        <div className="flex justify-end items-end gap-4 mb-4">
          <time className="text-white/90">
            {new Intl.DateTimeFormat(locale, {
              day: "numeric",
              month: "long",
              year: "numeric",
            }).format(new Date(blog.createdAt))}
          </time>
        </div>
        <h1 className="mb-4 font-normal text-3xl md:text-5xl leading-tight">
          {isRTL ? blog.titleFa : blog.titleEn}
        </h1>
        <p className="opacity-90 text-xl line-clamp-2">{isRTL ? blog.excerptFa : blog.excerptEn}</p>
      </div>
    </section>
  );
}
