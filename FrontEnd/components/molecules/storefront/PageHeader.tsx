import React from 'react';

type PageHeaderProps = {
  title: string;
  description?: string;
  eyebrow?: string;
  align?: 'start' | 'center';
  children?: React.ReactNode;
};

export default function PageHeader({
  title,
  description,
  eyebrow,
  align = 'start',
  children,
}: PageHeaderProps) {
  const alignClass =
    align === 'center' ? 'text-center items-center' : 'text-start items-start';

  return (
    <header className={`mb-2 flex flex-col gap-3 ${alignClass}`}>
      {eyebrow ? <p className="text-[0.7rem] sm:text-xs font-medium uppercase text-store-subtle ltr:tracking-[0.14em]">{eyebrow}</p> : null}
      <h1 className="font-normal text-3xl sm:text-4xl md:text-5xl leading-tight tracking-tight text-store-text">{title}</h1>
      {description ? <p className="max-w-2xl text-sm sm:text-base leading-relaxed text-store-subtle">{description}</p> : null}
      {children}
    </header>
  );
}
