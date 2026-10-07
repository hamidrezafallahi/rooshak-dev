"use client";
import React from 'react';

import { useTranslations } from 'next-intl';
import { useRouter } from 'next/navigation';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { LangIcon } from '@components/atoms/iconComponents';
import { setLocale } from '@slice/config';
import { TLang } from '@slice/config/type';
import { useAppSelector } from '@store/index';

export default function LangSwitcher() {
  const { locale } = useAppSelector((state) => {
    const locale = state.withPersist.config.locale;
    return { locale };
  }, shallowEqual);
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
