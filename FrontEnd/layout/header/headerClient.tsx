'use client';

import React, { useEffect, useState } from 'react';

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
  /** Float transparently over a full-bleed hero (home page); turns solid after scroll. */
  overlay?: boolean;
  /** Admin-managed bar above the navigation; null = nothing scheduled right now. */
  announcement?: HeaderAnnouncement | null;
};

export default function HeaderClient({ overlay = false, announcement = null }: HeaderProps) {
  const locale = useLocale();
  const pathname = usePathname();
  const t = useTranslations('header');
  const tBrand = useTranslations('brand');
  const [scrolled, setScrolled] = useState(false);

  useEffect(() => {
    if (!overlay) return;
    const onScroll = () => setScrolled(window.scrollY > 24);
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, [overlay]);

  const floating = overlay && !scrolled;

  return (
    <header
      role="banner"
      data-floating={floating}
      className="top-0 inset-x-0 z-50 fixed bg-store-surface data-[floating=true]:bg-transparent data-[floating=true]:bg-gradient-to-b data-[floating=true]:from-black/50 data-[floating=true]:to-transparent border-b border-store-border data-[floating=true]:border-transparent text-store-text data-[floating=true]:text-white transition-colors duration-300"
    >
      {announcement ? <AnnouncementStrip bar={announcement} /> : null}

      <div className="relative items-center gap-3 grid grid-cols-[1fr_auto_1fr] mx-auto px-4 sm:px-6 lg:px-10 max-w-[1600px] h-[var(--store-nav-h)]">
        <div className="flex items-center gap-1">
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
                  className="after:bottom-1 after:absolute relative after:inset-x-3 after:bg-current px-3 py-2 after:h-px font-medium text-sm hover:after:scale-x-100 aria-[current=page]:after:scale-x-100 after:content-[''] after:scale-x-0 after:transition-transform after:duration-300"
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
          className="flex justify-center"
        >
          <BrandLogo
            alt={tBrand('name')}
            onDark={floating}
            priority
            className="w-32 sm:w-44"
          />
        </Link>

        <div className="flex justify-end items-center gap-1">
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
