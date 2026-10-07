'use client';

import React, {
  useEffect,
  useMemo,
  useState,
} from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { createPortal } from 'react-dom';

import { CloseIcon, MenuIcon } from '@components/atoms/iconComponents';
import LangSwitcher from '@components/molecules/lang';
import ThemeSwitcher from '@components/molecules/theme';

const LINKS = [
  { href: '', labelKey: 'home' as const },
  { href: 'products', labelKey: 'products' as const },
  { href: 'categories', labelKey: 'categories' as const },
  { href: 'brands', labelKey: 'brands' as const },
  { href: 'suppliers', labelKey: 'suppliers' as const },
  { href: 'tags', labelKey: 'tags' as const },
  { href: 'exhibition', labelKey: 'exhibition' as const },
  { href: 'discounts', labelKey: 'discounts' as const },
  { href: 'blog', labelKey: 'blogs' as const },
  { href: 'shoppingCart', labelKey: 'shopping cart' as const },
  { href: 'register', labelKey: 'register' as const },
] as const;

function MobileMenu() {
  const [isMounted, setIsMounted] = useState(false);
  const [isVisible, setIsVisible] = useState(false);
  const [isOpen, setIsOpen] = useState(false);
  const locale = useLocale();
  const pathname = usePathname();
  const t = useTranslations('header');

  const openDrawer = () => {
    setIsVisible(true);
  };

  const closeDrawer = () => {
    setIsOpen(false);
  };

  const navLinks = useMemo(
    () =>
      LINKS.map((item) => ({
        ...item,
        hrefValue: item.href ? `/${locale}/${item.href}` : `/${locale}`,
      })),
    [locale],
  );

  useEffect(() => {
    setIsMounted(true);
  }, []);

  useEffect(() => {
    if (!isVisible) return;

    const frame = window.requestAnimationFrame(() => {
      setIsOpen(true);
    });

    return () => {
      window.cancelAnimationFrame(frame);
    };
  }, [isVisible]);

  useEffect(() => {
    if (isOpen) return;
    if (!isVisible) return;

    const timeout = window.setTimeout(() => {
      setIsVisible(false);
    }, 300);

    return () => {
      window.clearTimeout(timeout);
    };
  }, [isOpen, isVisible]);

  useEffect(() => {
    if (!isVisible) return;

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') closeDrawer();
    };

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';
    window.addEventListener('keydown', onKeyDown);

    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener('keydown', onKeyDown);
    };
  }, [isVisible]);

  useEffect(() => {
    closeDrawer();
  }, [pathname]);

  const drawer = isMounted && isVisible
    ? createPortal(
        <div
          className={`fixed inset-0 z-[90] ${isOpen ? 'pointer-events-auto' : 'pointer-events-none'}`}
          role="dialog"
          aria-modal="true"
          aria-labelledby="mobile-nav-title"
        >
          <button
            type="button"
            className={`absolute inset-0 h-full w-full cursor-default border-0 bg-black/50 p-0 transition-opacity duration-300 ${
              isOpen ? 'opacity-100' : 'opacity-0'
            }`}
            aria-label={t('closeMenu')}
            onClick={closeDrawer}
          />

          <aside
            id="mobile-nav-drawer"
            aria-label={t('mainNav')}
            className={`absolute inset-y-0 start-0 flex w-[min(88vw,420px)] flex-col overflow-hidden bg-store-surface text-store-text shadow-2xl transition-transform duration-300 ease-out ${
              isOpen
                ? 'translate-x-0'
                : '-translate-x-full rtl:translate-x-full'
            }`}
          >
            <div className="flex justify-between items-center px-5 border-b border-store-border h-16 shrink-0">
              <p id="mobile-nav-title" className="font-normal text-lg">
                {t('mainNav')}
              </p>
              <button
                type="button"
                onClick={closeDrawer}
                className="inline-flex justify-center items-center w-10 h-10 hover:opacity-60 transition-opacity"
                aria-label={t('closeMenu')}
              >
                <CloseIcon config={{ size: 18 }} />
              </button>
            </div>

            <ul className="flex-1 overflow-y-auto">
              {navLinks.map((item) => {
                const isActive = pathname === item.hrefValue;

                return (
                  <li key={item.href || 'home'} className="border-b border-store-border">
                    <Link
                      href={item.hrefValue}
                      onClick={closeDrawer}
                      aria-current={isActive ? 'page' : undefined}
                      className="flex justify-between items-center px-5 py-4 text-base aria-[current=page]:font-semibold hover:bg-store-muted transition-colors"
                    >
                      <span>{t(item.labelKey)}</span>
                      <span aria-hidden className="text-store-subtle rtl:rotate-180">
                        ›
                      </span>
                    </Link>
                  </li>
                );
              })}
            </ul>

            <div className="flex items-center gap-2 px-5 py-4 border-t border-store-border shrink-0 pb-[calc(env(safe-area-inset-bottom)+1rem)]">
              <Link
                href={`/${locale}/register`}
                onClick={closeDrawer}
                className="flex-1 bg-primary px-5 py-3 border border-primary font-medium text-primary-foreground text-sm text-center hover:bg-transparent hover:text-store-text transition-colors"
              >
                {t('register')}
              </Link>
              <ThemeSwitcher />
              <LangSwitcher />
            </div>
          </aside>
        </div>,
        document.body,
      )
    : null;

  return (
    <>
      <button
        type="button"
        onClick={openDrawer}
        className="inline-flex justify-center items-center w-10 h-10 hover:opacity-60 transition-opacity"
        aria-expanded={isOpen}
        aria-controls="mobile-nav-drawer"
        aria-label={t('openMenu')}
      >
        <MenuIcon config={{ size: 22 }} />
      </button>

      {drawer}
    </>
  );
}

export default MobileMenu;
