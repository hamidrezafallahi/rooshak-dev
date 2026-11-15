import 'server-only';

import { serverApiBaseUrl } from './api';
import { findFont, fontStack, headingFontStack } from './fontCatalog';
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
export function themeToCss(theme: SiteTheme | null): string {
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

  // Only whitelisted keys become a font stack, so the value can't inject CSS.
  if (findFont(theme.fontFamily)) colors.push(`--font-body:${fontStack(theme.fontFamily)}`);
  if (findFont(theme.headingFontFamily)) {
    colors.push(`--font-display:${headingFontStack(theme.headingFontFamily)}`);
  }

  const body = colors.join(';');

  return body ? `:root:root{${body}}` : '';
}
