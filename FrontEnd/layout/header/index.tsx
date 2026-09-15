'use client';

import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import { UserIcon } from '@components/atoms/iconComponents';
import LangSwitcher from '@components/molecules/lang';
import ThemeSwitcher from '@components/molecules/theme';

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

export default function Header() {
  const locale = useLocale();
  const t = useTranslations('header');
  const tBrand = useTranslations('brand');

  return (
    <header className="store-nav" role="banner">
      <div className="store-nav-bar">
        <div className="flex md:hidden items-center gap-2">
          <MobileMenu />
        </div>

        <Link
          href={`/${locale}`}
          className="font-bold text-base sm:text-lg tracking-tight shrink-0"
          style={{ color: 'var(--primary-color)' }}
        >
          {tBrand('name')}
        </Link>

        <nav
          className="hidden md:flex flex-1 justify-center items-center gap-1 lg:gap-2"
          aria-label={t('mainNav')}
        >
          {NAV_KEYS.map((item) => (
            <Link
              key={item.href}
              href={`/${locale}/${item.href}`}
              className="store-nav-link"
            >
              {t(item.labelKey)}
            </Link>
          ))}
        </nav>

        <div className="flex items-center gap-1.5 sm:gap-2 shrink-0">
          <Link
            href={`/${locale}/register`}
            aria-label={t('register')}
            className="store-icon-btn"
          >
            <UserIcon />
          </Link>
          <ShoppingCart />
          <div className="hidden sm:flex items-center gap-1.5">
            <ThemeSwitcher />
            <LangSwitcher />
          </div>
        </div>
      </div>
    </header>
  );
}
