import 'server-only';

import { serverApiBaseUrl } from './api';
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
 * CSS variables for :root. Doubled selector (`:root:root`) so it wins over the
 * fallback palette in globals.css regardless of stylesheet order. Values are
 * re-validated as hex, so nothing but colours can ever reach the <style> tag.
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

  const body = pairs
    .filter(([, value]) => HEX.test(value ?? ''))
    .map(([name, value]) => `${name}:${value}`)
    .join(';');

  return body ? `:root:root{${body}}` : '';
}
