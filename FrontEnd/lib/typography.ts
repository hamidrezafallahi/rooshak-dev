/**
 * Text sizes the admin picks in «تم سایت»: three presets or an exact pixel size, for
 * body text and for headings separately. Mirrors ThemeTextSizes in
 * BackEnd/Domain/Entities/ThemeSetting.cs (keys and pixel ranges).
 *
 * They become two CSS multipliers on :root: --text-scale (body) and --heading-scale
 * (headings). postcss/font-scale.cjs multiplies every fixed font-size by --text-scale,
 * and globals.css points --text-scale at --heading-scale inside headings.
 */
export type TextSizeKey = 'small' | 'medium' | 'large' | 'custom';

export const TEXT_SIZE_KEYS: TextSizeKey[] = ['small', 'medium', 'large', 'custom'];

/** Body text at «medium»: the browser default the whole stylesheet is built on. */
export const BODY_BASE_PX = 16;
/** Largest H1 size at «medium» (the home hero title, text-6xl = 3.75rem). */
export const HEADING_BASE_PX = 60;

export const BODY_PX_RANGE = { min: 12, max: 24 } as const;
export const HEADING_PX_RANGE = { min: 24, max: 120 } as const;

const BODY_PRESETS: Record<Exclude<TextSizeKey, 'custom'>, number> = {
  small: 0.875,
  medium: 1,
  large: 1.125,
};

const HEADING_PRESETS: Record<Exclude<TextSizeKey, 'custom'>, number> = {
  small: 0.85,
  medium: 1,
  large: 1.15,
};

const clamp = (value: number, min: number, max: number) => Math.min(max, Math.max(min, value));

function scaleOf(
  size: string | null | undefined,
  px: number | null | undefined,
  presets: Record<Exclude<TextSizeKey, 'custom'>, number>,
  base: number,
  range: { min: number; max: number },
): number {
  if (size === 'custom' && typeof px === 'number' && Number.isFinite(px)) {
    return Math.round((clamp(px, range.min, range.max) / base) * 1000) / 1000;
  }
  return presets[size as keyof typeof presets] ?? 1;
}

/** Multiplier for body text (1 = the site as designed). */
export const bodyTextScale = (size?: string | null, px?: number | null) =>
  scaleOf(size, px, BODY_PRESETS, BODY_BASE_PX, BODY_PX_RANGE);

/** Multiplier for headings (1 = the site as designed). */
export const headingTextScale = (size?: string | null, px?: number | null) =>
  scaleOf(size, px, HEADING_PRESETS, HEADING_BASE_PX, HEADING_PX_RANGE);
