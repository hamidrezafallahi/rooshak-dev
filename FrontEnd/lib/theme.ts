import 'server-only';

import { serverApiBaseUrl } from './api';
import {
  customFontFaceCss,
  findFont,
  type FontOption,
  fontStack,
  headingFontStack,
} from './fontCatalog';
import { bodyTextScale, bodyWeightShift, headingTextScale, headingWeightShift } from './typography';
import { logger } from './logger';

/** Mirrors ThemeSettingDto (BackEnd). Every colour is a #RRGGBB string. */
export type SiteTheme = {
  id: number;
  name: string;
  primaryColor: string;
  secondaryColor: string;
  highlightColor: string;
  neutralColor: string;
  successColor: string;
  errorColor: string;
  warningColor: string;
  infoColor: string;
  surfaceColor: string;
  surfaceMutedColor: string;
  borderColor: string;
  textColor: string;
  textMutedColor: string;
  /** Key from lib/fontCatalog.ts (e.g. "vazirmatn"); unknown keys are ignored. */
  fontFamily?: string | null;
  /** Heading font key (--font-display); unknown keys are ignored. */
  headingFontFamily?: string | null;
  /** small / medium / large / custom (lib/typography.ts). */
  bodyTextSize?: string | null;
  bodyTextSizePx?: number | null;
  headingTextSize?: string | null;
  headingTextSizePx?: number | null;
  /** 100..900; the site's design weight when missing. */
  bodyFontWeight?: number | null;
  headingFontWeight?: number | null;
};

const HEX = /^#[0-9a-fA-F]{6}$/;

/** Revalidated every minute; the admin panel edits the table behind `themeSettings`. */
export async function getActiveTheme(): Promise<SiteTheme | null> {
  const url = `${serverApiBaseUrl}/ThemeSettings/active`;
  try {
    const res = await fetch(url, {
      next: { revalidate: 60, tags: ['themeSettings'] },
      signal: AbortSignal.timeout(2500),
    });
    if (!res.ok) return null;
    const json = await res.json();
    return json?.isSuccess && json?.data ? (json.data as SiteTheme) : null;
  } catch (err) {
    logger.error('getActiveTheme failed', { scope: 'theme', source: 'server', url }, err);
    return null;
  }
}

/**
 * CSS variables for :root (colours, plus --font-body / --font-display for the chosen fonts).
 * Doubled selector (`:root:root`) so it wins over the
 * fallback palette in globals.css regardless of stylesheet order. Values are
 * re-validated as hex and the font key against lib/fontCatalog.ts, so nothing
 * else can ever reach the <style> tag.
 */
export function themeToCss(theme: SiteTheme | null, customFonts: FontOption[] = []): string {
  if (!theme) return '';

  const pairs: [string, string][] = [
    ['--primary-color', theme.primaryColor],
    ['--secondary-color', theme.secondaryColor],
    ['--highlight-color', theme.highlightColor],
    ['--neutral-color', theme.neutralColor],
    ['--success-color', theme.successColor],
    ['--error-color', theme.errorColor],
    ['--warning-color', theme.warningColor],
    ['--info-color', theme.infoColor],
    ['--store-surface', theme.surfaceColor],
    ['--store-surface-solid', theme.surfaceColor],
    ['--store-surface-muted', theme.surfaceMutedColor],
    ['--store-border', theme.borderColor],
    ['--store-border-strong', theme.textColor],
    ['--store-text', theme.textColor],
    ['--store-text-muted', theme.textMutedColor],
  ];

  const colors = pairs
    .filter(([, value]) => HEX.test(value ?? ''))
    .map(([name, value]) => `${name}:${value}`);

  // Only whitelisted keys (bundled, or uploaded fonts the API listed and
  // toCustomFontOptions validated) become a font stack, so nothing can inject CSS.
  const bodyFont = findFont(theme.fontFamily, customFonts);
  const headingFont = findFont(theme.headingFontFamily, customFonts);
  if (bodyFont) colors.push(`--font-body:${fontStack(theme.fontFamily, customFonts)}`);
  if (headingFont) {
    colors.push(`--font-display:${headingFontStack(theme.headingFontFamily, customFonts)}`);
  }

  // Numbers only: computed from whitelisted keys and clamped pixel values.
  const textScale = bodyTextScale(theme.bodyTextSize, theme.bodyTextSizePx);
  const headingScale = headingTextScale(theme.headingTextSize, theme.headingTextSizePx);
  if (textScale !== 1) colors.push(`--text-scale:${textScale}`);
  if (headingScale !== 1) colors.push(`--heading-scale:${headingScale}`);
  const weightShift = bodyWeightShift(theme.bodyFontWeight);
  const headingShift = headingWeightShift(theme.headingFontWeight);
  if (weightShift !== 0) colors.push(`--weight-shift:${weightShift}`);
  if (headingShift !== 0) colors.push(`--heading-weight-shift:${headingShift}`);

  const body = colors.join(';');
  // Only the uploaded fonts this theme uses are declared, so no other file is fetched.
  const faces = customFontFaceCss(
    [bodyFont, headingFont].filter((f): f is FontOption => Boolean(f?.family)),
  );

  return (faces + (body ? `:root:root{${body}}` : '')) || '';
}
