"use client";
import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import { useDispatch } from 'react-redux';

import { LangIcon } from '@components/atoms/iconComponents';
import { setLocale } from '@slice/config';
import { TLang } from '@slice/config/type';

export default function LangSwitcher() {
  // The URL is the source of truth: a locale persisted in the store (e.g. 'en' left
  // over from an earlier visit) must not decide where the switch goes.
  const locale = useLocale() as TLang;
  const router = useRouter();
  const t = useTranslations()
  const dispatch = useDispatch();

  const handleChangeLang = () => {
    const newLang: TLang = locale === 'fa' ? 'en' : 'fa';
    const locales = new Set(['fa', 'en']);
    const parts = window.location.pathname.split('/').filter(Boolean);

    if (parts.length > 0 && locales.has(parts[0])) {
      parts[0] = newLang;
    } else {
      parts.unshift(newLang);
    }

    const newPath = `/${parts.join('/')}`;
    dispatch(setLocale({ locale: newLang }));
    router.replace(newPath);
  };
  return (
    <button
      onClick={handleChangeLang}
      aria-label={t('header.lang')}
      className="flex justify-center items-center w-10 h-10 hover:opacity-60 transition-opacity"
    >
      <LangIcon config={{ size: 14 }} />
    </button>
  );
}
