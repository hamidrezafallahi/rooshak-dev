'use client';

import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import BrandLogo from '@components/atoms/brandLogo';
import { usePathname } from 'next/navigation';

import { UserIcon } from '@components/atoms/iconComponents';
import LangSwitcher from '@components/molecules/lang';

import MobileMenu from './mobileMenu';
import ShoppingCart from './shoppingCart';

const NAV_KEYS = [
  { href: 'products', labelKey: 'products' as const },
  { href: 'categories', labelKey: 'categories' as const },
  { href: 'brands', labelKey: 'brands' as const },
  { href: 'exhibition', labelKey: 'exhibition' as const },
  { href: 'discounts', labelKey: 'discounts' as const },
  { href: 'blog', labelKey: 'blogs' as const },
] as const;

export type HeaderAnnouncement = {
  message: string;
  link: { href: string; external: boolean } | null;
  backgroundColor: string;
  textColor: string;
  backgroundImage: string | null;
};

type HeaderProps = {
  /** Kept for API compatibility; the header is always solid now (see below). */
  overlay?: boolean;
  /** Admin-managed bar above the navigation; null = nothing scheduled right now. */
  announcement?: HeaderAnnouncement | null;
};

/**
 * The header always has a solid surface background — it no longer floats transparently
 * over the home-page hero video, where the cart / account / menu icons were unreadable.
 * Mobile: the menu button sits on the physical RIGHT edge in both languages; the
 * account + cart pair sits tight against the left edge as its visual counterweight.
 */
export default function HeaderClient({ announcement = null }: HeaderProps) {
  const locale = useLocale();
  const pathname = usePathname()!;
  const t = useTranslations('header');
  const tBrand = useTranslations('brand');

  return (
    <header
      role="banner"
      className="top-0 inset-x-0 z-50 fixed bg-store-surface border-b border-store-border text-store-text"
    >
      {announcement ? <AnnouncementStrip bar={announcement} /> : null}

      <div className="relative items-center gap-3 grid grid-cols-[1fr_auto_1fr] mx-auto px-1 sm:px-6 lg:px-10 max-w-[1600px] h-[var(--store-nav-h)]">
        <div className="flex max-md:ltr:justify-end items-center gap-1 max-md:ltr:order-3">
          <div className="md:hidden">
            <MobileMenu />
          </div>
          <nav
            className="hidden md:flex items-center -ms-3"
            aria-label={t('mainNav')}
          >
            {NAV_KEYS.map((item) => {
              const href = `/${locale}/${item.href}`;
              const active = pathname?.startsWith(href);
              return (
                <Link
                  key={item.href}
                  href={href}
                  aria-current={active ? 'page' : undefined}
                  className="after:bottom-1 after:absolute relative after:inset-x-3 after:bg-highlight px-3 py-2 after:h-px font-medium text-sm hover:after:scale-x-100 aria-[current=page]:after:scale-x-100 after:content-[''] after:scale-x-0 after:transition-transform after:duration-300"
                >
                  {t(item.labelKey)}
                </Link>
              );
            })}
          </nav>
        </div>

        <Link
          href={`/${locale}`}
          aria-label={tBrand('name')}
          className="flex justify-center max-md:ltr:order-2"
        >
          <BrandLogo
            alt={tBrand('name')}
            priority
            className="w-32 sm:w-44"
          />
        </Link>

        <div className="flex max-md:ltr:flex-row-reverse justify-end items-center gap-0 md:gap-1 max-md:ltr:order-1">
          <div className="hidden sm:flex items-center gap-1">
            <LangSwitcher />
          </div>
          <Link
            href={`/${locale}/register`}
            aria-label={t('register')}
            className="inline-flex justify-center items-center w-10 h-10 hover:opacity-60 transition-opacity"
          >
            <UserIcon />
          </Link>
          <ShoppingCart />
        </div>
      </div>
    </header>
  );
}

function AnnouncementStrip({ bar }: { bar: HeaderAnnouncement }) {
  const className =
    'flex justify-center items-center px-4 w-full h-[var(--store-announce-h)] overflow-hidden font-medium text-[0.7rem] sm:text-xs text-center bg-cover bg-center';
  const style: React.CSSProperties = {
    backgroundColor: bar.backgroundColor,
    color: bar.textColor,
    backgroundImage: bar.backgroundImage ? `url("${bar.backgroundImage}")` : undefined,
  };
  const text = <span className="line-clamp-2 leading-tight">{bar.message}</span>;

  if (!bar.link) {
    return (
      <p className={className} style={style}>
        {text}
      </p>
    );
  }

  return bar.link.external ? (
    <a
      href={bar.link.href}
      target="_blank"
      rel="noopener noreferrer"
      className={className}
      style={style}
    >
      {text}
    </a>
  ) : (
    <Link href={bar.link.href} className={className} style={style}>
      {text}
    </Link>
  );
}
