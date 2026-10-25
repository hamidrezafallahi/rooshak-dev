'use client';

import React, {
  FormEvent,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';

import {
  useLocale,
  useTranslations,
} from 'next-intl';
import Link from 'next/link';
import { useRouter } from 'next/navigation';

import { saveEntity } from '@lib/saveEntity';
import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

import {
  refreshStorefrontTheme,
  THEME_ENDPOINT,
} from './api';
import {
  Glint,
  Glyph,
} from './icons';
import {
  COLOR_GROUPS,
  COLOR_KEYS,
  ColorKey,
  CONTRAST_CHECKS,
  contrastRatio,
  HEX,
  Palette,
  PRESETS,
} from './palette';
import {
  LiveBadge,
  PrismBar,
} from './parts';
import StorefrontPreview from './storefrontPreview';

type StudioProps = {
  /** Present when editing an existing theme. */
  id?: number;
  isActive?: boolean;
  initialName: string;
  initialPalette: Palette;
};

const samePalette = (a: Palette, b: Palette) => COLOR_KEYS.every((key) => a[key] === b[key]);

function ColorRow({
  colorKey,
  value,
  original,
  onChange,
  onFocusKey,
}: {
  colorKey: ColorKey;
  value: string;
  original: string;
  onChange: (key: ColorKey, hex: string) => void;
  onFocusKey: (key: ColorKey | null) => void;
}) {
  const t = useTranslations('admin.themeSettings');
  const [text, setText] = useState(value);
  const label = t(`fields.${colorKey}.label`);
  const changed = value !== original;

  useEffect(() => setText(value), [value]);

  const commit = (raw: string) => {
    const next = raw.trim().startsWith('#') ? raw.trim() : `#${raw.trim()}`;
    setText(next);
    if (HEX.test(next)) onChange(colorKey, next.toLowerCase());
  };

  return (
    <li
      className="flex items-center gap-3 py-3 first:pt-0 last:pb-0"
      onMouseEnter={() => onFocusKey(colorKey)}
      onMouseLeave={() => onFocusKey(null)}
      onFocus={() => onFocusKey(colorKey)}
      onBlur={() => onFocusKey(null)}
    >
      <label
        className="relative shadow-inner rounded-xl ring-1 ring-black/10 focus-within:ring-2 focus-within:ring-[color:var(--primary-color)] w-11 h-11 overflow-hidden cursor-pointer shrink-0"
        style={{ background: value }}
      >
        <input
          type="color"
          value={value}
          onChange={(e) => onChange(colorKey, e.target.value.toLowerCase())}
          aria-label={label}
          className="absolute inset-0 opacity-0 w-full h-full cursor-pointer"
        />
        <span className="absolute inset-0 bg-[linear-gradient(135deg,rgba(255,255,255,0.45),transparent_55%)] pointer-events-none" />
      </label>

      <div className="flex-1 min-w-0">
        <p className="flex items-center gap-1.5 font-medium text-[var(--admin-text)] text-sm">
          {label}
          {changed && (
            <span
              className="bg-[var(--highlight-color)] rounded-full w-1.5 h-1.5"
              title={t('changedFrom', { hex: original })}
            />
          )}
        </p>
        <p className="text-[var(--admin-text-muted)] text-xs line-clamp-2 leading-5">
          {t(`fields.${colorKey}.help`)}
        </p>
      </div>

      <input
        type="text"
        dir="ltr"
        value={text}
        maxLength={7}
        spellCheck={false}
        autoComplete="off"
        aria-label={t('hexOf', { label })}
        aria-invalid={!HEX.test(text)}
        onChange={(e) => commit(e.target.value)}
        onBlur={() => setText(value)}
        className={`bg-[var(--admin-surface-muted)] px-2.5 py-2 border rounded-lg outline-none w-[5.75rem] font-mono text-[13px] text-[var(--admin-text)] uppercase transition-shadow focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--primary-color)_18%,transparent)] ${
          HEX.test(text) ? 'border-[var(--admin-border)]' : 'border-[var(--error-color)]'
        }`}
      />

      {changed ? (
        <button
          type="button"
          onClick={() => onChange(colorKey, original)}
          className="!w-8 !h-8 admin-icon-btn shrink-0"
          aria-label={t('revertColor')}
          title={t('revertColor')}
        >
          <Glyph name="undo" className="w-3.5 h-3.5" />
        </button>
      ) : (
        <span className="w-8 shrink-0" aria-hidden />
      )}
    </li>
  );
}

export default function ThemeStudio({ id, isActive = false, initialName, initialPalette }: StudioProps) {
  const t = useTranslations('admin.themeSettings');
  const locale = useLocale();
  const router = useRouter();
  const base = `/${locale}/admin/themeSettings`;

  const [name, setName] = useState(initialName);
  const [palette, setPalette] = useState<Palette>(initialPalette);
  const [focusKey, setFocusKey] = useState<ColorKey | null>(null);
  const [device, setDevice] = useState<'desktop' | 'mobile'>('desktop');
  const [mobileTab, setMobileTab] = useState<'colors' | 'preview'>('colors');
  const [saving, setSaving] = useState(false);
  const [nameError, setNameError] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);
  const nameRef = useRef<HTMLInputElement>(null);
  const leavingRef = useRef(false);

  const dirty = name !== initialName || !samePalette(palette, initialPalette);
  const number = useMemo(() => new Intl.NumberFormat(locale, { maximumFractionDigits: 1 }), [locale]);

  const checks = CONTRAST_CHECKS.map((check) => {
    const ratio = contrastRatio(palette[check.fg], palette[check.bg]);
    return { ...check, ratio, ok: ratio >= check.min };
  });
  const passed = checks.filter((c) => c.ok).length;

  // Unsaved colours are easy to lose with a stray click on the sidebar.
  useEffect(() => {
    if (!dirty) return;
    const warn = (e: BeforeUnloadEvent) => {
      if (leavingRef.current) return;
      e.preventDefault();
    };
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, [dirty]);

  // Ctrl/⌘ + S saves.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
        e.preventDefault();
        formRef.current?.requestSubmit();
      }
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, []);

  const setColor = (key: ColorKey, hex: string) => setPalette((prev) => ({ ...prev, [key]: hex }));

  const reset = () => {
    setName(initialName);
    setPalette(initialPalette);
    setNameError(false);
  };

  const save = async (e: FormEvent) => {
    e.preventDefault();
    if (saving) return;
    if (!name.trim()) {
      setNameError(true);
      setMobileTab('colors');
      nameRef.current?.focus();
      return;
    }
    setSaving(true);
    const res = await saveEntity({
      endPoint: THEME_ENDPOINT,
      method: id ? 'PUT' : 'POST',
      body: { ...(id ? { id } : {}), name: name.trim(), ...palette },
    });
    if (res?.isSuccess) {
      showSuccessToast(t(isActive ? 'savedLive' : 'saved'), '', 4000);
      if (isActive) await refreshStorefrontTheme();
      leavingRef.current = true;
      router.push(base);
      router.refresh();
    } else {
      showErrorToast(res?.error || t('saveFailed'), '', 5000);
      setSaving(false);
    }
  };

  return (
    <form ref={formRef} onSubmit={save} className="admin-page" noValidate>
      <header className="top-0 z-20 sticky flex justify-between items-center gap-2 sm:gap-3 p-2.5 sm:p-4 md:px-5 admin-panel">
        <div className="flex flex-1 items-center gap-2 sm:gap-3 min-w-0">
          <Link
            href={base}
            className="admin-icon-btn shrink-0"
            aria-label={t('back')}
            title={t('back')}
          >
            <Glyph name="back" className="w-4 h-4 ltr:rotate-180" />
          </Link>
          <span className="hidden sm:grid place-items-center bg-[var(--admin-active)] rounded-xl w-11 h-11 text-[var(--primary-color)] shrink-0">
            <Glyph name="swatch" className="w-5 h-5" />
          </span>
          <div className="min-w-0">
            <div className="flex flex-wrap items-center gap-2">
              <h1 className="!text-base sm:!text-lg md:!text-xl admin-page-title">
                {id ? t('editTitle') : t('createTitle')}
              </h1>
              {isActive && <LiveBadge label={t('liveShort')} />}
              {dirty && <span className="admin-badge admin-badge-warning">{t('unsaved')}</span>}
            </div>
            <p className="hidden sm:block admin-page-subtitle">{t('studioSubtitle')}</p>
          </div>
        </div>

        <div className="flex items-center gap-2 shrink-0">
          <button
            type="button"
            className="!px-2.5 sm:!px-3.5 admin-btn"
            onClick={reset}
            disabled={!dirty || saving}
            aria-label={t('reset')}
            title={t('reset')}
          >
            <Glyph name="undo" />
            <span className="hidden sm:inline">{t('reset')}</span>
          </button>
          <button
            type="submit"
            className="disabled:opacity-60 sm:min-w-32 admin-btn admin-btn-primary"
            disabled={saving}
            title={t('saveShortcut')}
          >
            {saving ? t('saving') : t('save')}
          </button>
        </div>
      </header>

      <div className="lg:hidden gap-1 grid grid-cols-2 p-1 admin-panel" role="tablist">
        {(['colors', 'preview'] as const).map((tab) => (
          <button
            key={tab}
            type="button"
            role="tab"
            aria-selected={mobileTab === tab}
            onClick={() => setMobileTab(tab)}
            className={`py-2 rounded-xl font-medium text-sm transition-colors ${
              mobileTab === tab
                ? 'bg-[var(--primary-color)] text-white'
                : 'text-[var(--admin-text-muted)]'
            }`}
          >
            {t(tab === 'colors' ? 'tabColors' : 'tabPreview')}
          </button>
        ))}
      </div>

      <div className="items-start gap-4 lg:gap-5 grid lg:grid-cols-[minmax(0,26rem)_minmax(0,1fr)] xl:grid-cols-[minmax(0,29rem)_minmax(0,1fr)]">
        <div className={`${mobileTab === 'colors' ? 'flex' : 'hidden'} lg:flex flex-col gap-4 min-w-0`}>
          <section className="flex flex-col gap-2 p-4 sm:p-5 admin-panel">
            <label htmlFor="theme-name" className="admin-field-label">
              {t('nameLabel')}
            </label>
            <input
              id="theme-name"
              ref={nameRef}
              value={name}
              onChange={(e) => {
                setName(e.target.value);
                if (e.target.value.trim()) setNameError(false);
              }}
              placeholder={t('namePlaceholder')}
              aria-invalid={nameError}
              maxLength={100}
              className={`bg-[var(--admin-surface-muted)] px-3 py-2.5 border rounded-xl outline-none w-full text-[var(--admin-text)] text-sm focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--primary-color)_18%,transparent)] ${
                nameError ? 'border-[var(--error-color)]' : 'border-[var(--admin-border)]'
              }`}
            />
            {nameError ? (
              <p className="admin-field-error">{t('nameRequired')}</p>
            ) : (
              <p className="text-[var(--admin-text-muted)] text-xs">{t('nameHelp')}</p>
            )}
            <PrismBar palette={palette} className="mt-2 h-2.5" />
          </section>

          <section className="p-4 sm:p-5 admin-panel">
            <h2 className="flex items-center gap-1.5 font-semibold text-[var(--admin-text)] text-sm">
              <Glint className="w-3 h-3 text-[var(--highlight-color)]" />
              {t('presetsTitle')}
            </h2>
            <p className="mt-1 text-[var(--admin-text-muted)] text-xs leading-5">{t('presetsHint')}</p>
            <div className="flex gap-2 -mx-1 mt-3 px-1 pb-1 overflow-x-auto snap-x hidden-show-scrollbar">
              {PRESETS.map((preset) => {
                const selected = samePalette(preset.palette, palette);
                return (
                  <button
                    key={preset.id}
                    type="button"
                    aria-pressed={selected}
                    onClick={() => setPalette(preset.palette)}
                    className={`snap-start flex flex-col gap-2 bg-[var(--admin-surface-elevated)] hover:bg-[var(--admin-hover)] p-2 border rounded-xl w-[6.75rem] text-start transition-colors shrink-0 ${
                      selected
                        ? 'border-[var(--primary-color)] ring-2 ring-[color-mix(in_srgb,var(--primary-color)_25%,transparent)]'
                        : 'border-[var(--admin-border)]'
                    }`}
                  >
                    <span className="flex rounded-lg h-9 overflow-hidden ring-1 ring-black/5">
                      {(['primaryColor', 'highlightColor', 'surfaceMutedColor', 'textColor'] as const).map((key) => (
                        <span key={key} className="flex-1" style={{ background: preset.palette[key] }} />
                      ))}
                    </span>
                    <span className="font-medium text-[var(--admin-text)] text-xs truncate">
                      {t(`presets.${preset.id}`)}
                    </span>
                  </button>
                );
              })}
            </div>
          </section>

          {COLOR_GROUPS.map((group) => (
            <section key={group.id} className="p-4 sm:p-5 admin-panel">
              <header className="flex justify-between items-start gap-3 mb-4">
                <div className="min-w-0">
                  <h2 className="font-semibold text-[var(--admin-text)] text-sm">{t(`groups.${group.id}.title`)}</h2>
                  <p className="mt-1 text-[var(--admin-text-muted)] text-xs leading-5">
                    {t(`groups.${group.id}.hint`)}
                  </p>
                </div>
                <span className="flex rounded-full h-5 overflow-hidden ring-1 ring-black/5 w-16 shrink-0" aria-hidden>
                  {group.keys.map((key) => (
                    <span key={key} className="flex-1" style={{ background: palette[key] }} />
                  ))}
                </span>
              </header>
              <ul className="divide-y divide-[color:var(--admin-border)]">
                {group.keys.map((key) => (
                  <ColorRow
                    key={key}
                    colorKey={key}
                    value={palette[key]}
                    original={initialPalette[key]}
                    onChange={setColor}
                    onFocusKey={setFocusKey}
                  />
                ))}
              </ul>
            </section>
          ))}

          <section className="p-4 sm:p-5 admin-panel">
            <header className="flex justify-between items-center gap-3">
              <div>
                <h2 className="font-semibold text-[var(--admin-text)] text-sm">{t('contrastTitle')}</h2>
                <p className="mt-1 text-[var(--admin-text-muted)] text-xs">{t('contrastHint')}</p>
              </div>
              <span
                className={`admin-badge shrink-0 ${
                  passed === checks.length ? 'admin-badge-success' : 'admin-badge-warning'
                }`}
              >
                {t('contrastScore', { passed, total: checks.length })}
              </span>
            </header>
            <ul className="gap-2 grid mt-4">
              {checks.map((check) => (
                <li
                  key={check.id}
                  onMouseEnter={() => setFocusKey(check.fg)}
                  onMouseLeave={() => setFocusKey(null)}
                  className="flex items-center gap-3 bg-[var(--admin-surface-elevated)] p-2.5 border border-[var(--admin-border)] rounded-xl min-w-0"
                >
                  <span
                    className="place-items-center grid rounded-lg ring-1 ring-black/10 w-10 h-10 font-semibold text-sm shrink-0"
                    style={{ background: palette[check.bg], color: palette[check.fg] }}
                    aria-hidden
                  >
                    {t('sample')}
                  </span>
                  <div className="flex-1 min-w-0">
                    <p className="text-[var(--admin-text)] text-sm truncate">{t(`checks.${check.id}`)}</p>
                    <p className="text-[var(--admin-text-muted)] text-xs">
                      {t('ratio', { value: number.format(check.ratio), min: number.format(check.min) })}
                    </p>
                  </div>
                  <span
                    className={`shrink-0 rounded-full px-2 py-0.5 text-[11px] font-semibold ${
                      check.ok
                        ? 'bg-[color-mix(in_srgb,var(--success-color)_14%,white)] text-[var(--success-color)]'
                        : 'bg-[var(--admin-danger-soft)] text-[var(--error-color)]'
                    }`}
                  >
                    {check.ok ? t('pass') : t('fail')}
                  </span>
                </li>
              ))}
            </ul>
          </section>
        </div>

        <div
          className={`${mobileTab === 'preview' ? 'flex' : 'hidden'} lg:flex flex-col lg:sticky lg:top-[6.5rem] min-w-0`}
        >
          <section className="overflow-hidden admin-panel">
            <div className="flex justify-between items-center gap-3 px-4 py-3 border-[var(--admin-border)] border-b">
              <h2 className="flex items-center gap-1.5 font-semibold text-[var(--admin-text)] text-sm">
                <Glint className="w-3 h-3 text-[var(--highlight-color)]" />
                {t('livePreview')}
              </h2>
              <div className="inline-flex bg-[var(--admin-surface-muted)] p-0.5 border border-[var(--admin-border)] rounded-lg">
                {(['desktop', 'mobile'] as const).map((d) => (
                  <button
                    key={d}
                    type="button"
                    aria-pressed={device === d}
                    aria-label={t(d === 'desktop' ? 'deviceDesktop' : 'deviceMobile')}
                    title={t(d === 'desktop' ? 'deviceDesktop' : 'deviceMobile')}
                    onClick={() => setDevice(d)}
                    className={`grid place-items-center w-8 h-7 rounded-md transition-colors ${
                      device === d
                        ? 'bg-[var(--admin-surface-elevated)] text-[var(--primary-color)] shadow-sm'
                        : 'text-[var(--admin-text-muted)]'
                    }`}
                  >
                    <Glyph name={d} />
                  </button>
                ))}
              </div>
            </div>
            <div className="bg-[radial-gradient(110%_80%_at_90%_0%,color-mix(in_srgb,var(--highlight-color)_18%,transparent),transparent_60%),linear-gradient(160deg,color-mix(in_srgb,var(--neutral-color)_85%,white),var(--admin-surface-muted))] p-3 sm:p-5">
              <div className="shadow-[0_28px_60px_-30px_rgba(15,23,42,0.5)] rounded-xl ring-1 ring-black/5 overflow-hidden">
                <div className="flex items-center gap-2 bg-[var(--admin-surface-elevated)] px-3 border-[var(--admin-border)] border-b h-8" aria-hidden>
                  <span className="flex gap-1.5 w-12 shrink-0">
                    <i className="bg-[var(--admin-border)] rounded-full w-2.5 h-2.5" />
                    <i className="bg-[var(--admin-border)] rounded-full w-2.5 h-2.5" />
                    <i className="bg-[var(--admin-border)] rounded-full w-2.5 h-2.5" />
                  </span>
                  <span dir="ltr" className="flex-1 bg-[var(--admin-surface-muted)] mx-auto px-2 py-0.5 rounded-md max-w-[14rem] text-[11px] text-[var(--admin-text-muted)] text-center truncate">
                    rooshakshop.ir
                  </span>
                  <span className="w-12 shrink-0" />
                </div>
                <div className="bg-[var(--admin-surface-muted)] lg:max-h-[calc(100dvh-16rem)] overflow-y-auto">
                  <StorefrontPreview palette={palette} device={device} focusKey={focusKey} />
                </div>
              </div>
            </div>
            <p className="px-4 py-2.5 border-[var(--admin-border)] border-t text-[var(--admin-text-muted)] text-xs">
              {t('previewHint')}
            </p>
          </section>

        </div>
      </div>
    </form>
  );
}
