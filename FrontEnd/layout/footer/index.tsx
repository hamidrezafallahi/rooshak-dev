'use client';

import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import BrandLogo from '@components/atoms/brandLogo';

import {
  InstagramIcon,
  TelegramIcon,
} from '@components/atoms/iconComponents';

const SOCIAL = [
  { icon: <InstagramIcon />, href: 'https://instagram.com/roshak_kitchenware', label: 'Instagram' },
  { icon: <TelegramIcon />, href: 'https://t.me/Arash71tj', label: 'Telegram' },
] as const;

/**
 * Four balanced columns on every screen size (2 × 2 on mobile, 4 across on desktop):
 * three link columns of 4–5 links each, plus the brand column.
 */
const COLUMNS = [
  {
    titleKey: 'shop' as const,
    links: [
      { href: 'products', labelKey: 'products' as const },
      { href: 'categories', labelKey: 'categories' as const },
      { href: 'brands', labelKey: 'brands' as const },
      { href: 'discounts', labelKey: 'discounts' as const },
      { href: 'suppliers', labelKey: 'suppliers' as const },
    ],
  },
  {
    titleKey: 'guides' as const,
    links: [
      { href: 'blog', labelKey: 'blog' as const },
      { href: 'tags', labelKey: 'tags' as const },
      { href: 'faq', labelKey: 'faq' as const },
      { href: 'about', labelKey: 'about' as const },
    ],
  },
  {
    titleKey: 'service' as const,
    links: [
      { href: 'register', labelKey: 'register' as const },
      { href: 'shoppingCart', labelKey: 'cart' as const },
      { href: 'order', labelKey: 'orders' as const },
      { href: 'cooperation', labelKey: 'cooperation' as const },
      { href: 'sitemap', labelKey: 'sitemap' as const },
    ],
  },
] as const;

const Footer: React.FC = () => {
  const locale = useLocale();
  const t = useTranslations('footer');
  const tBrand = useTranslations('brand');
  const year = new Date().getFullYear();

  return (
    <footer
      className="mt-auto pt-16 pb-8 border-t border-store-border bg-store-surface text-store-text"
      role="contentinfo"
    >
      <div className="gap-x-6 gap-y-10 lg:gap-10 grid grid-cols-2 md:grid-cols-4 mx-auto px-4 sm:px-6 lg:px-10 max-w-[1440px] text-center md:text-start">
        {COLUMNS.map((col) => (
          <nav key={col.titleKey} aria-label={t(`columns.${col.titleKey}`)}>
            <h3 className="mb-5 font-semibold text-xs uppercase ltr:tracking-[0.14em] store-footer-title">
              {t(`columns.${col.titleKey}`)}
            </h3>
            <ul className="flex flex-col gap-3 text-sm">
              {col.links.map((link) => (
                <li key={link.href}>
                  <Link
                    href={`/${locale}/${link.href}`}
                    className="relative inline-block pb-0.5 text-store-subtle hover:text-store-text transition-colors duration-300 after:content-[''] after:absolute after:bottom-0 after:start-0 after:h-px after:w-full after:bg-current after:origin-left rtl:after:origin-right after:scale-x-0 hover:after:scale-x-100 focus-visible:after:scale-x-100 after:transition-transform after:duration-300 after:ease-linear"
                  >
                    {t(`links.${link.labelKey}`)}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        ))}

        <div className="flex flex-col items-center md:items-start">
          <BrandLogo alt={tBrand('name')} className="md:-ms-2 mb-4 w-36 md:w-48" />
          <p className="max-w-xs text-store-subtle text-sm leading-relaxed">
            {t('tagline')}
          </p>
          <div className="flex flex-wrap justify-center md:justify-start gap-1 mt-5 md:-ms-2">
            {SOCIAL.map((s) => (
              <Link
                key={s.label}
                href={s.href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={s.label}
                className="inline-flex justify-center items-center w-9 h-9 text-store-text hover:opacity-60 transition-opacity"
              >
                {s.icon}
              </Link>
            ))}
          </div>
        </div>
      </div>

      <div className="mx-auto mt-14 px-4 sm:px-6 lg:px-10 pt-6 border-t border-store-border max-w-[1440px] text-store-subtle text-xs text-center md:text-start">
        {t('copyright', { year, brand: tBrand('name') })}
      </div>
    </footer>
  );
};

export default Footer;
