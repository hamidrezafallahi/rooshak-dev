import type { CSSProperties } from 'react';

/**
 * Palette model of the theme studio. Mirrors ThemeSettingDto (BackEnd) and
 * SiteTheme (lib/theme.ts); every colour is a lower-case #RRGGBB string.
 */
export const COLOR_KEYS = [
  'primaryColor',
  'secondaryColor',
  'highlightColor',
  'neutralColor',
  'successColor',
  'errorColor',
  'warningColor',
  'infoColor',
  'surfaceColor',
  'surfaceMutedColor',
  'borderColor',
  'textColor',
  'textMutedColor',
] as const;

export type ColorKey = (typeof COLOR_KEYS)[number];
export type Palette = Record<ColorKey, string>;
export type ThemeRecord = Palette & { id: number; name: string; isActive: boolean };

export const HEX = /^#[0-9a-fA-F]{6}$/;

/** Studio sections, in order. Labels and help texts: admin.themeSettings.groups / fields. */
export const COLOR_GROUPS: { id: 'brand' | 'canvas' | 'status'; keys: ColorKey[] }[] = [
  { id: 'brand', keys: ['primaryColor', 'highlightColor', 'secondaryColor', 'neutralColor'] },
  {
    id: 'canvas',
    keys: ['surfaceColor', 'surfaceMutedColor', 'borderColor', 'textColor', 'textMutedColor'],
  },
  { id: 'status', keys: ['successColor', 'errorColor', 'warningColor', 'infoColor'] },
];

/** The storefront fallback in globals.css ("Maison Crystal"). */
export const DEFAULT_PALETTE: Palette = {
  primaryColor: '#000000',
  secondaryColor: '#f2f2f2',
  highlightColor: '#a38a52',
  neutralColor: '#f7f7f7',
  successColor: '#0d9488',
  errorColor: '#c8102e',
  warningColor: '#b45309',
  infoColor: '#3b82f6',
  surfaceColor: '#ffffff',
  surfaceMutedColor: '#f7f7f7',
  borderColor: '#e3e3e3',
  textColor: '#000000',
  textMutedColor: '#6b6b6b',
};

const LIGHT_CANVAS = {
  surfaceColor: '#ffffff',
  surfaceMutedColor: '#f7f7f7',
  borderColor: '#e3e3e3',
  textColor: '#000000',
  textMutedColor: '#6b6b6b',
};

/** Starting points for a new theme: the named palettes already defined in globals.css. */
export const PRESETS: { id: string; palette: Palette }[] = [
  { id: 'maison', palette: DEFAULT_PALETTE },
  {
    id: 'onyx',
    palette: {
      primaryColor: '#ffffff',
      secondaryColor: '#1a1a1a',
      highlightColor: '#d4b66a',
      neutralColor: '#0b0b0b',
      successColor: '#14b8a6',
      errorColor: '#fb7185',
      warningColor: '#fbbf24',
      infoColor: '#7dd3fc',
      surfaceColor: '#111111',
      surfaceMutedColor: '#181818',
      borderColor: '#2a2a2a',
      textColor: '#f5f5f5',
      textMutedColor: '#a1a1a1',
    },
  },
  {
    id: 'champagne',
    palette: {
      ...LIGHT_CANVAS,
      primaryColor: '#9a7b4f',
      secondaryColor: '#1c39bb',
      highlightColor: '#f5e6c8',
      neutralColor: '#faf7f2',
      successColor: '#0d9488',
      errorColor: '#be123c',
      warningColor: '#b45309',
      infoColor: '#5b7cfa',
    },
  },
  {
    id: 'emerald',
    palette: {
      ...LIGHT_CANVAS,
      primaryColor: '#0f766e',
      secondaryColor: '#5eead4',
      highlightColor: '#c9a84c',
      neutralColor: '#f0fdfa',
      successColor: '#059669',
      errorColor: '#e11d48',
      warningColor: '#d97706',
      infoColor: '#22d3ee',
    },
  },
  {
    id: 'amethyst',
    palette: {
      ...LIGHT_CANVAS,
      primaryColor: '#6d28d9',
      secondaryColor: '#c4b5fd',
      highlightColor: '#e8c547',
      neutralColor: '#f5f3ff',
      successColor: '#0d9488',
      errorColor: '#e11d48',
      warningColor: '#d97706',
      infoColor: '#818cf8',
    },
  },
  {
    id: 'frost',
    palette: {
      ...LIGHT_CANVAS,
      primaryColor: '#0284c7',
      secondaryColor: '#a5f3fc',
      highlightColor: '#1c39bb',
      neutralColor: '#f0f9ff',
      successColor: '#0d9488',
      errorColor: '#e11d48',
      warningColor: '#d97706',
      infoColor: '#38bdf8',
    },
  },
];

/** Builds a ThemeRecord from an API row; colours that are not valid hex fall back to the default. */
export function toThemeRecord(row: Record<string, unknown> | null | undefined): ThemeRecord | null {
  if (!row || typeof row.id !== 'number') return null;
  const palette = { ...DEFAULT_PALETTE };
  COLOR_KEYS.forEach((key) => {
    const value = row[key];
    if (typeof value === 'string' && HEX.test(value)) palette[key] = value.toLowerCase();
  });
  return {
    ...palette,
    id: row.id,
    name: typeof row.name === 'string' ? row.name : '',
    isActive: Boolean(row.isActive),
  };
}

export function pickPalette(source: Palette): Palette {
  const palette = {} as Palette;
  COLOR_KEYS.forEach((key) => {
    palette[key] = source[key];
  });
  return palette;
}

// ===== WCAG 2.1 contrast =====

function channel(value: number) {
  const s = value / 255;
  return s <= 0.03928 ? s / 12.92 : ((s + 0.055) / 1.055) ** 2.4;
}

export function luminance(hex: string) {
  const n = parseInt(hex.slice(1), 16);
  return 0.2126 * channel((n >> 16) & 255) + 0.7152 * channel((n >> 8) & 255) + 0.0722 * channel(n & 255);
}

export function contrastRatio(a: string, b: string) {
  if (!HEX.test(a) || !HEX.test(b)) return 1;
  const [hi, lo] = [luminance(a), luminance(b)].sort((x, y) => y - x);
  return (hi + 0.05) / (lo + 0.05);
}

export const isDarkColor = (hex: string) => HEX.test(hex) && luminance(hex) < 0.2;

/** Pairs the storefront actually renders; `min` is the WCAG AA threshold for that use. */
export const CONTRAST_CHECKS: { id: string; fg: ColorKey; bg: ColorKey; min: number }[] = [
  { id: 'body', fg: 'textColor', bg: 'surfaceColor', min: 4.5 },
  { id: 'muted', fg: 'textMutedColor', bg: 'surfaceColor', min: 4.5 },
  { id: 'button', fg: 'surfaceColor', bg: 'primaryColor', min: 4.5 },
  { id: 'frame', fg: 'textColor', bg: 'surfaceMutedColor', min: 4.5 },
  { id: 'sale', fg: 'errorColor', bg: 'surfaceColor', min: 4.5 },
  { id: 'accent', fg: 'highlightColor', bg: 'surfaceColor', min: 3 },
];

// ===== Preview plumbing =====

const VAR_NAME: Record<ColorKey, string> = {
  primaryColor: '--t-primary',
  secondaryColor: '--t-secondary',
  highlightColor: '--t-highlight',
  neutralColor: '--t-neutral',
  successColor: '--t-success',
  errorColor: '--t-error',
  warningColor: '--t-warning',
  infoColor: '--t-info',
  surfaceColor: '--t-surface',
  surfaceMutedColor: '--t-surface-muted',
  borderColor: '--t-border',
  textColor: '--t-text',
  textMutedColor: '--t-text-muted',
};

/** Scoped CSS variables, so a preview never touches the admin's own palette. */
export function previewVars(palette: Palette): CSSProperties {
  const vars: Record<string, string> = {};
  COLOR_KEYS.forEach((key) => {
    vars[VAR_NAME[key]] = palette[key];
  });
  return vars as CSSProperties;
}

/**
 * Outlines every preview element tagged `data-c="<key>"` while that colour row is
 * hovered or focused in the studio, so the admin sees where a colour is used.
 */
export function focusOutlineCss(scope: string) {
  const selectors = COLOR_KEYS.map(
    (key) => `${scope}[data-focus="${key}"] [data-c~="${key}"]`,
  ).join(',');
  return `${selectors}{outline:2px dashed var(--primary-color);outline-offset:2px;transition:outline-color .2s}`;
}
