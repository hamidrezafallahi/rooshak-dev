"use client";

import React, {
  useEffect,
  useMemo,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';
import {
  shallowEqual,
  useDispatch,
} from 'react-redux';

import { setTheme } from '@slice/config';
import { TTheme } from '@slice/config/type';
import { useAppSelector } from '@store/index';

const themes: TTheme[] = [
  "default",
  "dark",
  "theme2",
  "theme3",
  "theme4",
];

export default function ThemeSwitcher() {
  const t = useTranslations();

  const dispatch = useDispatch();

  const { theme } = useAppSelector(
    (state) => {
      return {
        theme: state.withPersist.config.theme,
      };
    },
    shallowEqual
  );

  const currentIndex = useMemo(() => {
    const foundedIndex = themes.findIndex(
      (item) => item === theme
    );

    return foundedIndex >= 0 ? foundedIndex : 0;
  }, [theme]);

  const [index, setIndex] = useState(currentIndex);

  useEffect(() => {
    setIndex(currentIndex);
  }, [currentIndex]);

  useEffect(() => {
    if (!theme) return;

    applyTheme(theme);
  }, [theme]);

  const applyTheme = (selectedTheme: TTheme) => {
    if (selectedTheme === "default") {
      document.documentElement.removeAttribute(
        "data-theme"
      );
    } else {
      document.documentElement.setAttribute(
        "data-theme",
        selectedTheme
      );
    }

    document.cookie = `theme=${selectedTheme}; path=/; max-age=31536000`;
  };

  const handleNextTheme = () => {
    const nextIndex = (index + 1) % themes.length;

    const nextTheme = themes[nextIndex];

    setIndex(nextIndex);

    applyTheme(nextTheme);

    dispatch(
      setTheme({
        theme: nextTheme,
      })
    );
  };

  return (
    <button
      onClick={handleNextTheme}
      aria-label={t("header.theme")}
      className="flex justify-center items-center w-10 h-10 hover:opacity-60 transition-opacity"
    >
      <span className="block border border-current rounded-full w-3 h-3 bg-current/20" />
    </button>
  );
}
