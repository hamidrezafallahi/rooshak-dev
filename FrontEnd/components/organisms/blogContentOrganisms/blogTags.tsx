import React from 'react';

import Link from 'next/link';

import { IProps } from './type';

export async function BlogTags({ ...props }: IProps) {
  const { blog, locale } = props;
  const isRTL = locale == "fa";
  return (
    <div className={`flex flex-wrap gap-2 p-2  ${isRTL ? "justify-end" : ""}`}>
      {blog?.blogTags?.map((tag,idx) => (
        <Link
        href={`/${locale}/tags/${tag.name}`}
        key={idx}
          className="border border-store-border hover:border-store-strong px-4 py-2 text-sm transition-colors cursor-pointer"
        >
            #{tag.name}
        </Link>
      ))}
    </div>
  );
}
