import React from 'react';

import Link from 'next/link';

type Props = {
  title: string;
  subtitle?: string;
  href?: string;
  linkLabel?: string;
  /** Heading level — pages that already own the <h1> use h2. */
  as?: 'h2' | 'h3';
  align?: 'start' | 'center';
  id?: string;
  children?: React.ReactNode;
};

export default function SectionHeading({
  title,
  subtitle,
  href,
  linkLabel,
  as: Tag = 'h2',
  align = 'start',
  id,
  children,
}: Props) {
  const centered = align === 'center';
  return (
    <div
      className={`flex flex-col gap-4 mb-8 md:mb-12 ${
        centered
          ? 'items-center text-center'
          : 'sm:flex-row sm:items-end sm:justify-between'
      }`}
    >
      <div className={centered ? 'max-w-2xl' : 'max-w-2xl'}>
        <Tag
          id={id}
          className="font-normal text-2xl sm:text-3xl md:text-4xl tracking-tight"
        >
          {title}
        </Tag>
        {subtitle ? (
          <p className="mt-2 text-store-subtle text-sm sm:text-base leading-relaxed">
            {subtitle}
          </p>
        ) : null}
      </div>
      {children}
      {href && linkLabel ? (
        <Link
          href={href}
          className="self-start sm:self-auto pb-0.5 border-current border-b font-medium text-sm hover:opacity-60 whitespace-nowrap transition-opacity"
        >
          {linkLabel}
        </Link>
      ) : null}
    </div>
  );
}
