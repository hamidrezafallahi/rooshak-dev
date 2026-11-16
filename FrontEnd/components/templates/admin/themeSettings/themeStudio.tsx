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

import {
  customFontFaceCss,
  DEFAULT_FONT_KEY,
  DEFAULT_HEADING_FONT_KEY,
  findFont,
  FONT_CATEGORIES,
  FONT_OPTIONS,
  type FontCategory,
  type FontOption,
  fontStack,
  headingFontStack,
  isFontKey,
} from '@lib/fontCatalog';
import {
  BODY_BASE_PX,
  BODY_PX_RANGE,
  bodyTextScale,
  bodyWeightShift,
  FONT_WEIGHTS,
  type FontWeight,
  HEADING_BASE_PX,
  HEADING_PX_RANGE,
  headingTextScale,
  headingWeightShift,
  TEXT_SIZE_KEYS,
  type TextSizeKey,
} from '@lib/typography';
import { saveEntity } from '@lib/saveEntity';
import {
  showErrorToast,
  showSuccessToast,
} from '@utils/core';

import {
  refreshStorefrontTheme,
  THEME_ENDPOINT,
  uploadCustomFont,
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
  DEFAULT_TYPOGRAPHY,
  Palette,
  PRESETS,
  Typography,
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
  /** Main font key (lib/fontCatalog.ts); defaults to the site's current font. */
  initialFont?: string | null;
  /** Heading font key; defaults to the site's current heading face. */
  initialHeadingFont?: string | null;
  /** Body / heading text sizes (lib/typography.ts). */
  initialTypography?: Typography;
  /** Fonts the admin uploaded (CustomFonts table), already validated. */
  customFonts?: FontOption[];
};

type FontTarget = 'body' | 'heading';

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

/** Upload box at the end of each font category: file + Persian/English name. */
function AddFontCard({ category, onAdded }: { category: FontCategory; onAdded: () => void }) {
  const t = useTranslations('admin.themeSettings');
  const [open, setOpen] = useState(false);
  const [file, setFile] = useState<File | null>(null);
  const [nameFa, setNameFa] = useState('');
  const [nameEn, setNameEn] = useState('');
  const [busy, setBusy] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  const submit = async () => {
    if (busy) return;
    if (!file) {
      showErrorToast(t('addFontNeedFile'), '', 4000);
      return;
    }
    if (!nameFa.trim() && !nameEn.trim()) {
      showErrorToast(t('addFontNeedName'), '', 4000);
      return;
    }
    setBusy(true);
    const res = await uploadCustomFont({ file, nameFa: nameFa.trim(), nameEn: nameEn.trim(), category });
    setBusy(false);
    if (res.isSuccess) {
      showSuccessToast(t('addFontDone'), '', 4000);
      setOpen(false);
      setFile(null);
      setNameFa('');
      setNameEn('');
      if (fileRef.current) fileRef.current.value = '';
      onAdded();
    } else {
      showErrorToast(res.error || t('addFontFailed'), '', 6000);
    }
  };

  if (!open) {
    return (
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="flex justify-center items-center gap-2 px-3.5 py-3 border border-[var(--admin-border)] hover:border-[var(--admin-text)] border-dashed w-full h-full min-h-[84px] text-[13px] text-[var(--admin-text-muted)] hover:text-[var(--admin-text)] transition-colors"
      >
        <span aria-hidden className="text-lg leading-none">+</span>
        {t('addFontTitle')}
      </button>
    );
  }

  return (
    <div className="flex flex-col gap-2.5 px-3.5 py-3 border border-[var(--admin-text)] w-full">
      <p className="m-0 text-[13px] text-[var(--admin-text-muted)] leading-6">{t('addFontHint')}</p>
      <label className="flex flex-col gap-1 text-[13px]">
        {t('addFontFile')}
        <input
          ref={fileRef}
          type="file"
          accept=".woff2,.woff,.ttf,.otf,font/woff2,font/woff,font/ttf,font/otf"
          onChange={(e) => setFile(e.target.files?.[0] ?? null)}
          className="text-[13px]"
        />
      </label>
      <input
        value={nameFa}
        onChange={(e) => setNameFa(e.target.value)}
        placeholder={t('addFontNameFa')}
        aria-label={t('addFontNameFa')}
        maxLength={60}
        className="luca-input"
      />
      <input
        value={nameEn}
        onChange={(e) => setNameEn(e.target.value)}
        placeholder={t('addFontNameEn')}
        aria-label={t('addFontNameEn')}
        maxLength={60}
        dir="ltr"
        className="luca-input"
      />
      <div className="flex gap-2">
        <button type="button" className="flex-1 admin-btn admin-btn-primary" onClick={submit} disabled={busy}>
          {busy ? t('addFontUploading') : t('addFontSubmit')}
        </button>
        <button type="button" className="admin-btn" onClick={() => setOpen(false)} disabled={busy}>
          {t('addFontCancel')}
        </button>
      </div>
    </div>
  );
}

function FontPicker({
  value,
  onChange,
  target,
  customFonts,
  onFontAdded,
}: {
  value: string;
  onChange: (key: string) => void;
  target: FontTarget;
  customFonts: FontOption[];
  onFontAdded: () => void;
}) {
  const t = useTranslations('admin.themeSettings');
  const locale = useLocale();
  const heading = target === 'heading';
  const allFonts = [...FONT_OPTIONS, ...customFonts];

  return (
    <div className="flex flex-col gap-6" role="radiogroup" aria-label={t(`fontTargets.${target}`)}>
      {FONT_CATEGORIES.map((category) => (
        <div key={category}>
          <p className="mb-2 font-medium text-[13px] text-[var(--admin-text-muted)]">
            {t(`fontCategories.${category}`)}
          </p>
          <ul className="gap-2 grid grid-cols-1 sm:grid-cols-2">
            {allFonts.filter((font) => font.category === category).map((font) => {
              const selected = font.key === value;
              return (
                <li key={font.key}>
                  <button
                    type="button"
                    role="radio"
                    aria-checked={selected}
                    onClick={() => onChange(font.key)}
                    className={`flex flex-col items-start gap-1.5 px-3.5 py-3 border w-full h-full text-start transition-colors ${
                      selected
                        ? 'border-[var(--admin-text)] ring-1 ring-[color:var(--admin-text)]'
                        : 'border-[var(--admin-border)] hover:border-[var(--admin-text)]'
                    }`}
                  >
                    <span className="flex justify-between items-baseline gap-2 w-full text-[13px] text-[var(--admin-text-muted)]">
                      <span>
                        {locale === 'fa' ? font.fa : font.en}
                        {locale === 'fa' && (
                          <span dir="ltr" className="ms-1.5 text-[11px]">
                            {font.en}
                          </span>
                        )}
                      </span>
                      <span className="flex gap-1.5 shrink-0">
                        {font.family && <span className="admin-badge">{t('customFontBadge')}</span>}
                        {selected && (
                          <span className="admin-badge admin-badge-success">{t('fontCurrent')}</span>
                        )}
                      </span>
                    </span>
                    <span
                      className={`text-[var(--admin-text)] leading-relaxed ${heading ? 'text-[22px]' : 'text-[17px]'}`}
                      style={{
                        fontFamily: heading
                          ? headingFontStack(font.key, customFonts)
                          : fontStack(font.key, customFonts),
                      }}
                    >
                      {t(heading ? 'fontHeadingSample' : 'fontSample')}
                    </span>
                  </button>
                </li>
              );
            })}
            <li>
              <AddFontCard category={category} onAdded={onFontAdded} />
            </li>
          </ul>
        </div>
      ))}
    </div>
  );
}

/** Three presets or an exact pixel size, for body text or for headings. */
function TextSizePicker({
  target,
  size,
  px,
  onChange,
}: {
  target: FontTarget;
  size: TextSizeKey;
  px: number | null;
  onChange: (size: TextSizeKey, px: number | null) => void;
}) {
  const t = useTranslations('admin.themeSettings');
  const range = target === 'heading' ? HEADING_PX_RANGE : BODY_PX_RANGE;
  const base = target === 'heading' ? HEADING_BASE_PX : BODY_BASE_PX;
  const inputId = `text-size-px-${target}`;
  const invalid = size === 'custom' && (px === null || px < range.min || px > range.max);

  return (
    <div className="flex flex-col gap-2.5">
      <p className="m-0 font-medium text-[13px] text-[var(--admin-text-muted)]">{t(`fontTargets.${target}`)}</p>
      <div className="flex flex-wrap" role="radiogroup" aria-label={t(`fontTargets.${target}`)}>
        {TEXT_SIZE_KEYS.map((key) => (
          <button
            key={key}
            type="button"
            role="radio"
            aria-checked={size === key}
            onClick={() => onChange(key, key === 'custom' ? (px ?? base) : null)}
            className={`flex-1 sm:flex-none px-4 h-10 border text-[13px] transition-colors -ms-px first:ms-0 ${
              size === key
                ? 'relative z-10 border-[var(--admin-text)] bg-[var(--admin-text)] text-[var(--admin-surface-elevated)]'
                : 'border-[var(--admin-border)] text-[var(--admin-text)] hover:border-[var(--admin-text)]'
            }`}
          >
            {t(`textSizes.${key}`)}
          </button>
        ))}
      </div>
      {size === 'custom' && (
        <div className="flex items-center gap-2">
          <label htmlFor={inputId} className="text-[13px]">
            {t(target === 'heading' ? 'textSizePxHeading' : 'textSizePxBody')}
          </label>
          <input
            id={inputId}
            type="number"
            inputMode="numeric"
            min={range.min}
            max={range.max}
            value={px ?? ''}
            onChange={(e) => {
              const n = Number.parseInt(e.target.value, 10);
              onChange('custom', Number.isFinite(n) ? n : null);
            }}
            aria-invalid={invalid}
            dir="ltr"
            className={`w-24 luca-input ${invalid ? '!border-[var(--error-color)]' : ''}`}
          />
          <span className="text-[13px] text-[var(--admin-text-muted)]">px</span>
        </div>
      )}
      {size === 'custom' && (
        <p className={`m-0 text-[13px] ${invalid ? 'text-[var(--error-color)]' : 'text-[var(--admin-text-muted)]'}`}>
          {t('textSizePxRange', { min: range.min, max: range.max, base })}
        </p>
      )}
    </div>
  );
}

/** Font weight (100..900) for body text or for headings; every other weight moves with it. */
function FontWeightPicker({
  target,
  value,
  onChange,
}: {
  target: FontTarget;
  value: FontWeight;
  onChange: (weight: FontWeight) => void;
}) {
  const t = useTranslations('admin.themeSettings');
  const label = `${t(`fontTargets.${target}`)} — ${t('fontWeightTitle')}`;
  return (
    <div className="flex flex-col gap-2.5">
      <p className="m-0 font-medium text-[13px] text-[var(--admin-text-muted)]">{label}</p>
      <div className="flex flex-wrap gap-1.5" role="radiogroup" aria-label={label}>
        {FONT_WEIGHTS.map((weight) => (
          <button
            key={weight}
            type="button"
            role="radio"
            aria-checked={value === weight}
            onClick={() => onChange(weight)}
            className={`flex flex-col items-center justify-center px-3 min-w-[4.5rem] h-14 border text-[13px] transition-colors ${
              value === weight
                ? 'border-[var(--admin-text)] bg-[var(--admin-text)] text-[var(--admin-surface-elevated)]'
                : 'border-[var(--admin-border)] text-[var(--admin-text)] hover:border-[var(--admin-text)]'
            }`}
          >
            {/* The sample keeps its real weight: [font-weight:var(--w)] is not rewritten by the theme. */}
            <span className="text-[17px] leading-none [font-weight:var(--w)]" style={{ ['--w' as string]: weight }}>
              {t('fontWeightSample')}
            </span>
            <span className="mt-1 text-[11px] opacity-75">{t(`fontWeights.${weight}`)}</span>
          </button>
        ))}
      </div>
    </div>
  );
}

export default function ThemeStudio({
  id,
  isActive = false,
  initialName,
  initialPalette,
  initialFont,
  initialHeadingFont,
  initialTypography = DEFAULT_TYPOGRAPHY,
  customFonts = [],
}: StudioProps) {
  const t = useTranslations('admin.themeSettings');
  const locale = useLocale();
  const router = useRouter();
  const base = `/${locale}/admin/themeSettings`;

  const [name, setName] = useState(initialName);
  const [palette, setPalette] = useState<Palette>(initialPalette);
  // Uploaded fonts keep their key even if the list could not be loaded, so saving
  // never silently swaps a custom font for the default one.
  const startFont = initialFont && isFontKey(initialFont) ? initialFont : DEFAULT_FONT_KEY;
  const [font, setFont] = useState<string>(startFont);
  const startHeadingFont =
    initialHeadingFont && isFontKey(initialHeadingFont) ? initialHeadingFont : DEFAULT_HEADING_FONT_KEY;
  const [headingFont, setHeadingFont] = useState<string>(startHeadingFont);
  const [typography, setTypography] = useState<Typography>(initialTypography);
  const [fontTarget, setFontTarget] = useState<FontTarget>('body');
  const [focusKey, setFocusKey] = useState<ColorKey | null>(null);
  const [device, setDevice] = useState<'desktop' | 'mobile'>('desktop');
  const [mobileTab, setMobileTab] = useState<'colors' | 'preview'>('colors');
  const [saving, setSaving] = useState(false);
  const [nameError, setNameError] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);
  const nameRef = useRef<HTMLInputElement>(null);
  const leavingRef = useRef(false);

  const sameTypography =
    typography.bodyTextSize === initialTypography.bodyTextSize &&
    typography.bodyTextSizePx === initialTypography.bodyTextSizePx &&
    typography.headingTextSize === initialTypography.headingTextSize &&
    typography.headingTextSizePx === initialTypography.headingTextSizePx &&
    typography.bodyFontWeight === initialTypography.bodyFontWeight &&
    typography.headingFontWeight === initialTypography.headingFontWeight;
  const dirty =
    name !== initialName ||
    !samePalette(palette, initialPalette) ||
    font !== startFont ||
    headingFont !== startHeadingFont ||
    !sameTypography;
  const fontFaces = useMemo(() => customFontFaceCss(customFonts), [customFonts]);
  const fontName = (key: string) => findFont(key, customFonts)?.[locale === 'fa' ? 'fa' : 'en'];
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
    setFont(startFont);
    setHeadingFont(startHeadingFont);
    setTypography(initialTypography);
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
      body: {
        ...(id ? { id } : {}),
        name: name.trim(),
        ...palette,
        fontFamily: font,
        headingFontFamily: headingFont,
        bodyTextSize: typography.bodyTextSize,
        bodyTextSizePx: typography.bodyTextSize === 'custom' ? typography.bodyTextSizePx : null,
        headingTextSize: typography.headingTextSize,
        headingTextSizePx: typography.headingTextSize === 'custom' ? typography.headingTextSizePx : null,
        bodyFontWeight: typography.bodyFontWeight,
        headingFontWeight: typography.headingFontWeight,
      },
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
      {/* Uploaded fonts for the previews below (validated paths under /uploads/fonts only). */}
      {fontFaces ? <style dangerouslySetInnerHTML={{ __html: fontFaces }} /> : null}
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
            <h2 className="font-semibold text-[var(--admin-text)] text-sm">{t('fontTitle')}</h2>
            <p className="mt-1 mb-4 text-[var(--admin-text-muted)] text-xs leading-5">{t('fontHint')}</p>
            <div
              className="inline-flex bg-[var(--admin-surface-muted)] mb-4 p-0.5 border border-[var(--admin-border)] rounded-lg"
              role="tablist"
              aria-label={t('fontTitle')}
            >
              {(['body', 'heading'] as const).map((target) => (
                <button
                  key={target}
                  type="button"
                  role="tab"
                  aria-selected={fontTarget === target}
                  onClick={() => setFontTarget(target)}
                  className={`px-3 h-8 rounded-md text-xs transition-colors ${
                    fontTarget === target
                      ? 'bg-[var(--admin-surface-elevated)] text-[var(--primary-color)] shadow-sm font-semibold'
                      : 'text-[var(--admin-text-muted)]'
                  }`}
                >
                  {t(`fontTargets.${target}`)}
                  <span className="ms-1.5 opacity-75">
                    ({fontName(target === 'body' ? font : headingFont)})
                  </span>
                </button>
              ))}
            </div>
            {fontTarget === 'body' ? (
              <FontPicker
                target="body"
                value={font}
                onChange={setFont}
                customFonts={customFonts}
                onFontAdded={() => router.refresh()}
              />
            ) : (
              <FontPicker
                target="heading"
                value={headingFont}
                onChange={setHeadingFont}
                customFonts={customFonts}
                onFontAdded={() => router.refresh()}
              />
            )}
          </section>

          <section className="p-4 sm:p-5 admin-panel">
            <h2 className="font-semibold text-[var(--admin-text)] text-sm">{t('textSizeTitle')}</h2>
            <p className="mt-1 mb-4 text-[var(--admin-text-muted)] text-xs leading-5">{t('textSizeHint')}</p>
            <div className="flex flex-col gap-6">
              <TextSizePicker
                target="body"
                size={typography.bodyTextSize}
                px={typography.bodyTextSizePx}
                onChange={(size, px) => setTypography((prev) => ({ ...prev, bodyTextSize: size, bodyTextSizePx: px }))}
              />
              <TextSizePicker
                target="heading"
                size={typography.headingTextSize}
                px={typography.headingTextSizePx}
                onChange={(size, px) =>
                  setTypography((prev) => ({ ...prev, headingTextSize: size, headingTextSizePx: px }))
                }
              />
              <FontWeightPicker
                target="body"
                value={typography.bodyFontWeight}
                onChange={(w) => setTypography((prev) => ({ ...prev, bodyFontWeight: w }))}
              />
              <FontWeightPicker
                target="heading"
                value={typography.headingFontWeight}
                onChange={(w) => setTypography((prev) => ({ ...prev, headingFontWeight: w }))}
              />
            </div>
          </section>

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
                <div
                  className="bg-[var(--admin-surface-muted)] lg:max-h-[calc(100dvh-16rem)] overflow-y-auto"
                  style={{
                    ['--font-body' as string]: fontStack(font, customFonts),
                    ['--font-display' as string]: headingFontStack(headingFont, customFonts),
                    ['--text-scale' as string]: String(bodyTextScale(typography.bodyTextSize, typography.bodyTextSizePx)),
                    ['--heading-scale' as string]: String(
                      headingTextScale(typography.headingTextSize, typography.headingTextSizePx),
                    ),
                    ['--weight-shift' as string]: String(bodyWeightShift(typography.bodyFontWeight)),
                    ['--heading-weight-shift' as string]: String(headingWeightShift(typography.headingFontWeight)),
                    fontFamily: 'var(--font-body)',
                  }}
                >
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
