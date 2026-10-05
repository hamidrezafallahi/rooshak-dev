'use client';

import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import {
  FacebookIcon,
  InstagramIcon,
  TelegramIcon,
  TwitterIcon,
  WhatsAppIcon,
  YouTubeIcon,
} from '@components/atoms/iconComponents';

const SOCIAL = [
  { icon: <FacebookIcon />, href: 'https://facebook.com', label: 'Facebook' },
  { icon: <InstagramIcon />, href: 'https://instagram.com', label: 'Instagram' },
  { icon: <TwitterIcon />, href: 'https://twitter.com', label: 'Twitter' },
  { icon: <YouTubeIcon />, href: 'https://youtube.com', label: 'YouTube' },
  { icon: <TelegramIcon />, href: 'https://t.me', label: 'Telegram' },
  { icon: <WhatsAppIcon />, href: 'https://wa.me', label: 'WhatsApp' },
] as const;

const COLUMNS = [
  {
    titleKey: 'shop' as const,
    links: [
      { href: 'products', labelKey: 'products' as const },
      { href: 'categories', labelKey: 'categories' as const },
      { href: 'brands', labelKey: 'brands' as const },
      { href: 'discounts', labelKey: 'discounts' as const },
    ],
  },
  {
    titleKey: 'discover' as const,
    links: [
      { href: 'blog', labelKey: 'blog' as const },
      { href: 'suppliers', labelKey: 'suppliers' as const },
      { href: 'tags', labelKey: 'tags' as const },
      { href: 'faq', labelKey: 'faq' as const },
      { href: 'cooperation', labelKey: 'cooperation' as const },
      { href: 'sitemap', labelKey: 'sitemap' as const },
    ],
  },
  {
    titleKey: 'account' as const,
    links: [
      { href: 'register', labelKey: 'register' as const },
      { href: 'shoppingCart', labelKey: 'cart' as const },
      { href: 'order', labelKey: 'orders' as const },
    ],
  },
] as const;

const Footer: React.FC = () => {
  const locale = useLocale();
  const t = useTranslations('footer');
  const tBrand = useTranslations('brand');
  const year = new Date().getFullYear();

  return (
    <footer className="store-footer" role="contentinfo">
      <div className="gap-10 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 mx-auto px-4 sm:px-6 lg:px-8 max-w-7xl">
        <div className="sm:col-span-2 lg:col-span-2">
          <h2 className="mb-3 font-bold text-white text-xl sm:text-2xl">
            {tBrand('name')}
          </h2>
          <p className="max-w-md text-sm leading-relaxed text-white/95">
            {t('tagline')}
          </p>
          <div className="flex flex-wrap gap-3 mt-5">
            {SOCIAL.map((s) => (
              <Link
                key={s.label}
                href={s.href}
                target="_blank"
                rel="noopener noreferrer"
                aria-label={s.label}
                className="inline-flex justify-center items-center text-white/95 hover:text-white rounded-lg w-9 h-9 transition"
              >
                {s.icon}
              </Link>
            ))}
          </div>
        </div>

        {COLUMNS.map((col) => (
          <div key={col.titleKey}>
            <h3 className="mb-4 font-semibold text-white text-sm tracking-wide">
              {t(`columns.${col.titleKey}`)}
            </h3>
            <ul className="flex flex-col gap-2.5 text-sm">
              {col.links.map((link) => (
                <li key={link.href}>
                  <Link
                    href={`/${locale}/${link.href}`}
                    className="text-white/95 hover:text-white transition"
                  >
                    {t(`links.${link.labelKey}`)}
                  </Link>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </div>

      <div className="mx-auto mt-12 pt-6 px-4 max-w-7xl border-white/10 border-t text-sm text-center text-white/90">
        {t('copyright', { year, brand: tBrand('name') })}
      </div>
    </footer>
  );
};

export default Footer;
