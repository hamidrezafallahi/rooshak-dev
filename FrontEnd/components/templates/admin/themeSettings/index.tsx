'use client';

import React, { useState } from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

import {
  activateTheme,
  deleteTheme,
  refreshStorefrontTheme,
} from './api';
import {
  Glint,
  Glyph,
} from './icons';
import { ThemeRecord } from './palette';
import {
  LiveBadge,
  PrismBar,
  SwatchStack,
} from './parts';
import StorefrontPreview, { StorefrontThumb } from './storefrontPreview';

export default function ThemeGallery({ themes }: { themes: ThemeRecord[] | null }) {
  const t = useTranslations('admin.themeSettings');
  const tf = useTranslations('admin.themeSettings.fields');
  const locale = useLocale();
  const router = useRouter();
  const [busyId, setBusyId] = useState<number | null>(null);
  const [confirmId, setConfirmId] = useState<number | null>(null);
  const base = `/${locale}/admin/themeSettings`;

  if (themes === null) {
    return (
      <div className="admin-page">
        <div className="admin-panel admin-empty">
          <p className="admin-empty-title">{t('loadError')}</p>
          <button type="button" className="mt-2 admin-btn" onClick={() => router.refresh()}>
            {t('retry')}
          </button>
        </div>
      </div>
    );
  }

  const active = themes.find((theme) => theme.isActive) ?? null;

  const run = async (theme: ThemeRecord, action: 'activate' | 'delete') => {
    if (busyId !== null) return;
    setBusyId(theme.id);
    const res = action === 'activate' ? await activateTheme(theme.id) : await deleteTheme(theme.id);
    if (res.isSuccess) {
      showSuccessToast(
        t(action === 'activate' ? 'activated' : 'deleted', { name: theme.name }),
        '',
        4000,
      );
      if (action === 'activate') await refreshStorefrontTheme();
      setConfirmId(null);
      router.refresh();
    } else {
      showErrorToast(res.error || t('actionFailed'), '', 5000);
    }
    setBusyId(null);
  };

  return (
    <div className="admin-page">
      <header className="admin-page-header">
        <div className="min-w-0">
          <p className="flex items-center gap-1.5 font-medium text-[var(--highlight-color)] text-xs">
            <Glint className="w-3 h-3" />
            {t('eyebrow')}
          </p>
          <h1 className="mt-1 admin-page-title">{t('title')}</h1>
          <p className="max-w-xl admin-page-subtitle">{t('subtitle')}</p>
        </div>
        <div className="flex gap-2 shrink-0">
          <Link href={`/${locale}`} target="_blank" className="flex-1 sm:flex-none admin-btn">
            <Glyph name="external" />
            {t('viewStore')}
          </Link>
          <Link href={`${base}/new`} className="flex-1 sm:flex-none admin-btn admin-btn-primary">
            <Glyph name="plus" />
            {t('newTheme')}
          </Link>
        </div>
      </header>

      {active ? (
        <section className="grid lg:grid-cols-[minmax(0,1.3fr)_minmax(0,1fr)] overflow-hidden admin-panel">
          <div className="relative bg-[radial-gradient(120%_90%_at_85%_0%,color-mix(in_srgb,var(--highlight-color)_22%,transparent),transparent_60%),linear-gradient(160deg,color-mix(in_srgb,var(--neutral-color)_90%,white),var(--admin-surface-muted))] p-4 sm:p-6">
            <div className="relative shadow-[0_28px_60px_-28px_rgba(15,23,42,0.45)] ring-1 ring-black/5 max-h-[300px] sm:max-h-[360px] overflow-hidden">
              <StorefrontPreview palette={active} />
              <div className="bottom-0 absolute inset-x-0 bg-gradient-to-t from-[var(--admin-surface-muted)] to-transparent h-16" />
            </div>
          </div>

          <div className="flex flex-col gap-5 p-5 sm:p-6 border-[var(--admin-border)] border-t lg:border-t-0 lg:border-s">
            <div className="flex flex-col items-start gap-3">
              <LiveBadge label={t('liveNow')} />
              <div>
                <h2 className="font-semibold text-[var(--admin-text)] text-xl sm:text-2xl tracking-tight">
                  {active.name}
                </h2>
                <p className="mt-1 text-[var(--admin-text-muted)] text-sm leading-6">{t('liveHint')}</p>
              </div>
            </div>

            <PrismBar palette={active} className="h-4" />

            <dl className="gap-3 grid grid-cols-2">
              {(['primaryColor', 'highlightColor', 'surfaceColor', 'textColor'] as const).map((key) => (
                <div
                  key={key}
                  className="flex items-center gap-3 bg-[var(--admin-surface-elevated)] p-2.5 border border-[var(--admin-border)] rounded-xl min-w-0"
                >
                  <span
                    className="rounded-lg ring-1 ring-black/10 w-9 h-9 shrink-0"
                    style={{ background: active[key] }}
                  />
                  <div className="min-w-0">
                    <dt className="text-[var(--admin-text-muted)] text-xs truncate">{tf(`${key}.label`)}</dt>
                    <dd dir="ltr" className="font-mono text-[var(--admin-text)] text-sm text-end uppercase">
                      {active[key]}
                    </dd>
                  </div>
                </div>
              ))}
            </dl>

            <div className="flex gap-2 mt-auto">
              <Link href={`${base}/${active.id}`} className="flex-1 admin-btn admin-btn-primary">
                <Glyph name="pencil" />
                {t('edit')}
              </Link>
              <Link href={`${base}/new?from=${active.id}`} className="flex-1 admin-btn">
                <Glyph name="copy" />
                {t('duplicate')}
              </Link>
            </div>
          </div>
        </section>
      ) : (
        <section className="flex sm:flex-row flex-col sm:items-center gap-4 p-5 sm:p-6 admin-panel">
          <span className="place-items-center grid bg-[color-mix(in_srgb,var(--warning-color)_14%,white)] rounded-2xl w-12 h-12 text-[var(--warning-color)] shrink-0">
            <Glyph name="warning" className="w-6 h-6" />
          </span>
          <div className="flex-1 min-w-0">
            <p className="font-semibold text-[var(--admin-text)]">{t('noActiveTitle')}</p>
            <p className="mt-1 text-[var(--admin-text-muted)] text-sm leading-6">{t('noActiveHint')}</p>
          </div>
        </section>
      )}

      <section className="flex flex-col gap-3">
        <div className="flex justify-between items-center gap-3">
          <h2 className="font-semibold text-[var(--admin-text)] text-base">{t('allThemes')}</h2>
          <span className="admin-badge admin-badge-success">{t('count', { count: themes.length })}</span>
        </div>

        <ul className="gap-4 grid sm:grid-cols-2 xl:grid-cols-3">
          {themes.map((theme) => {
            const busy = busyId === theme.id;
            return (
              <li
                key={theme.id}
                className={`admin-panel group flex flex-col overflow-hidden transition-transform duration-300 hover:-translate-y-0.5 ${
                  theme.isActive ? 'ring-2 ring-[color:var(--highlight-color)]' : ''
                }`}
              >
                <Link
                  href={`${base}/${theme.id}`}
                  aria-label={t('editNamed', { name: theme.name })}
                  className="relative block bg-[linear-gradient(160deg,color-mix(in_srgb,var(--neutral-color)_80%,white),var(--admin-surface-muted))] p-3 aspect-[16/10]"
                >
                  <div className="shadow-[0_14px_30px_-18px_rgba(15,23,42,0.5)] ring-1 ring-black/5 h-full overflow-hidden group-hover:scale-[1.015] transition-transform duration-500">
                    <StorefrontThumb palette={theme} />
                  </div>
                  {theme.isActive && (
                    <span className="top-5 absolute start-5">
                      <LiveBadge label={t('liveShort')} />
                    </span>
                  )}
                </Link>

                <div className="flex flex-col flex-1 gap-3 p-4">
                  <div className="flex justify-between items-start gap-3">
                    <div className="min-w-0">
                      <h3 className="font-semibold text-[var(--admin-text)] truncate">{theme.name}</h3>
                      <p className="text-[var(--admin-text-muted)] text-xs">{t('themeId', { id: theme.id })}</p>
                    </div>
                    <SwatchStack colors={[theme.primaryColor, theme.highlightColor, theme.surfaceColor, theme.textColor]} />
                  </div>

                  <PrismBar palette={theme} className="h-2" />

                  {confirmId === theme.id ? (
                    <div className="flex flex-wrap justify-between items-center gap-2 bg-[var(--admin-danger-soft)] mt-auto px-3 py-2 rounded-xl text-sm">
                      <span className="text-[var(--error-color)]">{t('confirmDelete')}</span>
                      <span className="flex gap-1.5">
                        <button type="button" className="!px-3 !py-1.5 admin-btn" onClick={() => setConfirmId(null)} disabled={busy}>
                          {t('cancel')}
                        </button>
                        <button
                          type="button"
                          className="!bg-[var(--error-color)] !px-3 !py-1.5 !border-[var(--error-color)] !text-white admin-btn"
                          onClick={() => run(theme, 'delete')}
                          disabled={busy}
                        >
                          {busy ? t('working') : t('delete')}
                        </button>
                      </span>
                    </div>
                  ) : (
                    <div className="flex items-center gap-2 mt-auto">
                      {theme.isActive ? (
                        <span className="flex-1 opacity-80 admin-btn cursor-default">
                          <Glyph name="check" />
                          {t('isLive')}
                        </span>
                      ) : (
                        <button
                          type="button"
                          className="flex-1 admin-btn admin-btn-primary"
                          onClick={() => run(theme, 'activate')}
                          disabled={busyId !== null}
                        >
                          {busy ? t('working') : t('activate')}
                        </button>
                      )}
                      <Link
                        href={`${base}/${theme.id}`}
                        className="admin-icon-btn"
                        aria-label={t('edit')}
                        title={t('edit')}
                      >
                        <Glyph name="pencil" />
                      </Link>
                      <Link
                        href={`${base}/new?from=${theme.id}`}
                        className="admin-icon-btn"
                        aria-label={t('duplicate')}
                        title={t('duplicate')}
                      >
                        <Glyph name="copy" />
                      </Link>
                      <button
                        type="button"
                        className="disabled:opacity-40 admin-icon-btn admin-icon-btn-danger disabled:cursor-not-allowed"
                        aria-label={t('delete')}
                        title={theme.isActive ? t('cannotDeleteActive') : t('delete')}
                        disabled={theme.isActive || busyId !== null}
                        onClick={() => setConfirmId(theme.id)}
                      >
                        <Glyph name="trash" />
                      </button>
                    </div>
                  )}
                </div>
              </li>
            );
          })}

          <li>
            <Link
              href={`${base}/new`}
              className="group flex flex-col justify-center items-center gap-3 hover:bg-[var(--admin-hover)] p-6 border-[var(--admin-border)] border-2 border-dashed rounded-[var(--admin-radius)] h-full min-h-[260px] text-center transition-colors"
            >
              <span className="place-items-center grid bg-[var(--admin-active)] rounded-full w-14 h-14 text-[var(--primary-color)] group-hover:scale-105 transition-transform">
                <Glyph name="plus" className="w-6 h-6" />
              </span>
              <span className="font-semibold text-[var(--admin-text)]">{t('newTileTitle')}</span>
              <span className="max-w-[16rem] text-[var(--admin-text-muted)] text-xs leading-5">{t('newTileHint')}</span>
            </Link>
          </li>
        </ul>
      </section>
    </div>
  );
}
