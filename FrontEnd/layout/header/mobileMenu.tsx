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
            className={`absolute inset-0 h-full w-full border-0 bg-[rgba(3,7,18,0.52)] p-0 backdrop-blur-[4px] transition-opacity duration-300 ${
              isOpen ? 'opacity-100' : 'opacity-0'
            }`}
            aria-label={t('closeMenu')}
            onClick={closeDrawer}
          />

          <aside
            id="mobile-nav-drawer"
            aria-label={t('mainNav')}
            className={`absolute inset-x-0 bottom-0 mx-auto flex max-h-[85dvh] w-full max-w-screen-sm flex-col overflow-hidden rounded-t-[28px] border border-b-0 border-[color:var(--store-border)] bg-[color:var(--store-surface-solid)] text-[var(--store-text)] shadow-[0_-24px_60px_rgba(0,0,0,0.24)] transition-transform duration-300 ease-out ${
              isOpen ? 'translate-y-0' : 'translate-y-full'
            }`}
          >
            <div className="flex justify-center px-4 pt-3 pb-1">
              <span className="rounded-full w-12 h-1.5 bg-[color:color-mix(in_srgb,var(--store-text-muted)_22%,transparent)]" />
            </div>

            <div className="flex justify-between items-center gap-3 px-4 sm:px-5 pt-3 pb-4 border-[color:var(--store-border)] border-b">
              <div className="min-w-0">
                <p
                  id="mobile-nav-title"
                  className="font-semibold text-[var(--store-text)] text-base truncate"
                >
                  {t('mainNav')}
                </p>
                
              </div>

              <div className="flex items-center gap-2 shrink-0">
                <ThemeSwitcher />
                <LangSwitcher />
                <button
                  type="button"
                  onClick={closeDrawer}
                  className="store-icon-btn shrink-0"
                  aria-label={t('closeMenu')}
                >
                  <CloseIcon config={{ size: 16 }} />
                </button>
              </div>
            </div>

            <div className="flex flex-col flex-1 bg-[linear-gradient(180deg,color-mix(in_srgb,var(--primary-color)_3%,transparent)_0%,transparent_100%)] min-h-0">
              <div className="px-4 sm:px-5 pt-4 pb-2 font-semibold text-[11px] text-[var(--store-text-muted)] uppercase tracking-[0.18em]">
                {t('mainNav')}
              </div>

              <ul className="flex-1 gap-2 grid grid-cols-1 sm:grid-cols-2 px-3 sm:px-4 pb-4 overflow-y-auto">
                {navLinks.map((item) => {
                  const isActive = pathname === item.hrefValue;

                  return (
                    <li key={item.href || 'home'}>
                      <Link
                        href={item.hrefValue}
                        onClick={closeDrawer}
                        className={`flex items-center justify-between gap-3 rounded-2xl border px-4 py-3 text-sm font-medium transition-all duration-200 ${
                          isActive
                            ? 'border-[color:color-mix(in_srgb,var(--primary-color)_18%,transparent)] bg-[color:color-mix(in_srgb,var(--primary-color)_12%,var(--store-surface-solid))] text-[var(--primary-color)] shadow-[inset_0_-3px_0_var(--primary-color)]'
                            : 'border-transparent bg-transparent text-[var(--store-text)] hover:border-[color:color-mix(in_srgb,var(--primary-color)_14%,transparent)] hover:bg-[color:color-mix(in_srgb,var(--primary-color)_9%,var(--store-surface-solid))] hover:text-[var(--primary-color)]'
                        }`}
                      >
                        <span>{t(item.labelKey)}</span>
                        <span
                          aria-hidden="true"
                          className={`block h-2 w-2 shrink-0 rounded-full transition-all duration-200 ${
                            isActive
                              ? 'scale-110 bg-[var(--primary-color)]'
                              : 'bg-[color:color-mix(in_srgb,var(--store-text-muted)_28%,transparent)]'
                          }`}
                        />
                      </Link>
                    </li>
                  );
                })}
              </ul>
            </div>

            <div className="[padding-bottom:calc(env(safe-area-inset-bottom)+1rem)] flex sm:flex-row flex-col gap-3 bg-[color:color-mix(in_srgb,var(--store-surface-solid)_94%,transparent)] px-4 sm:px-5 py-4 sm:py-5 border-[color:var(--store-border)] border-t">
              <Link
                href={`/${locale}/register`}
                className="flex-1 store-btn store-btn-primary"
                onClick={closeDrawer}
              >
                {t('register')}
              </Link>

              <button
                type="button"
                className="flex-1 store-btn"
                onClick={closeDrawer}
              >
                {t('closeMenu')}
              </button>
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
        className="inline-flex items-center gap-2 bg-[color:var(--store-surface-muted)] px-3 border border-[color:var(--store-border)] hover:border-[color:color-mix(in_srgb,var(--primary-color)_35%,transparent)] rounded-xl h-10 font-medium text-[var(--store-text)] hover:text-[var(--primary-color)] text-sm transition-all duration-200"
        aria-expanded={isOpen}
        aria-controls="mobile-nav-drawer"
        aria-label={t('openMenu')}
      >
        <MenuIcon config={{ size: 20 }} />
        
      </button>

      {drawer}
    </>
  );
}

export default MobileMenu;
