/**
 * Fonts the admin can pick in «تم سایت» for body text and for headings.
 * Keys must match ThemeFonts.Keys in BackEnd/Domain/Entities/ThemeSetting.cs;
 * the backend rejects any other key, and themeToCss ignores unknown keys, so
 * nothing but these stacks can reach the page's CSS.
 *
 * `cssVar` is the variable next/font sets on <html> (see lib/fonts.ts). Every
 * font is self-hosted from /_next/static/media; a font file is only downloaded
 * when a page actually renders text in it.
 */
export type FontCategory = 'general' | 'formal' | 'fancy';

export type FontOption = {
  key: string;
  /** Persian display name. */
  fa: string;
  /** English (original) name. */
  en: string;
  category: FontCategory;
  /** Bundled fonts: the next/font CSS variable. */
  cssVar?: string;
  /** Uploaded fonts: the @font-face family name (see customFontFaceCss). */
  family?: string;
  /** Uploaded fonts: root-absolute URL of the font file. */
  src?: string;
};

/** Mirrors CustomFontDto (BackEnd): a font the admin uploaded in «تم سایت». */
export type CustomFontDto = {
  id: number;
  key: string;
  nameFa: string;
  nameEn: string;
  category: string;
  fileUrl: string;
  isActive?: boolean;
};

/** Rooshak's current face for body text and headings (IRANSans). */
export const DEFAULT_FONT_KEY = 'iransans';
export const DEFAULT_HEADING_FONT_KEY = 'iransans';

export const FONT_CATEGORIES: FontCategory[] = ['general', 'formal', 'fancy'];

export const FONT_OPTIONS: FontOption[] = [
  // ===== عمومی =====
  { key: 'vazirmatn', fa: 'وزیرمتن', en: 'Vazirmatn', category: 'general', cssVar: '--font-vazirmatn' },
  { key: 'iransans', fa: 'ایران‌سنس', en: 'IRANSans', category: 'general', cssVar: '--IRANSans-font' },
  { key: 'noto-sans-arabic', fa: 'نوتو سنس', en: 'Noto Sans Arabic', category: 'general', cssVar: '--font-noto-sans-arabic' },
  { key: 'harmattan', fa: 'هرمتان', en: 'Harmattan', category: 'general', cssVar: '--font-harmattan' },
  { key: 'mada', fa: 'مدی', en: 'Mada', category: 'general', cssVar: '--font-mada' },
  { key: 'tajawal', fa: 'تجول', en: 'Tajawal', category: 'general', cssVar: '--font-tajawal' },
  { key: 'cairo', fa: 'قاهره', en: 'Cairo', category: 'general', cssVar: '--font-cairo' },
  { key: 'readex-pro', fa: 'ریدکس پرو', en: 'Readex Pro', category: 'general', cssVar: '--font-readex-pro' },
  // ===== رسمی =====
  { key: 'noto-naskh-arabic', fa: 'نوتو نسخ', en: 'Noto Naskh Arabic', category: 'formal', cssVar: '--font-noto-naskh-arabic' },
  { key: 'amiri', fa: 'امیری', en: 'Amiri', category: 'formal', cssVar: '--font-amiri' },
  { key: 'scheherazade-new', fa: 'شهرزاد', en: 'Scheherazade New', category: 'formal', cssVar: '--font-scheherazade-new' },
  { key: 'lateef', fa: 'لطیف', en: 'Lateef', category: 'formal', cssVar: '--font-lateef' },
  { key: 'markazi-text', fa: 'مرکزی', en: 'Markazi Text', category: 'formal', cssVar: '--font-markazi' },
  // ===== فانتزی و تزئینی =====
  { key: 'lalezar', fa: 'لاله‌زار', en: 'Lalezar', category: 'fancy', cssVar: '--font-lalezar' },
  { key: 'reem-kufi', fa: 'ریم کوفی', en: 'Reem Kufi', category: 'fancy', cssVar: '--font-reem-kufi' },
  { key: 'el-messiri', fa: 'المسیری', en: 'El Messiri', category: 'fancy', cssVar: '--font-el-messiri' },
  { key: 'changa', fa: 'چنگا', en: 'Changa', category: 'fancy', cssVar: '--font-changa' },
  { key: 'aref-ruqaa', fa: 'عارف رقعه', en: 'Aref Ruqaa', category: 'fancy', cssVar: '--font-aref-ruqaa' },
  { key: 'rakkas', fa: 'رقاص', en: 'Rakkas', category: 'fancy', cssVar: '--font-rakkas' },
  { key: 'lemonada', fa: 'لیموناد', en: 'Lemonada', category: 'fancy', cssVar: '--font-lemonada' },
  { key: 'noto-nastaliq-urdu', fa: 'نستعلیق', en: 'Noto Nastaliq Urdu', category: 'fancy', cssVar: '--font-noto-nastaliq-urdu' },
];

const BY_KEY = new Map(FONT_OPTIONS.map((f) => [f.key, f]));

/** Keys of uploaded fonts: custom-{id} (CustomFont.KeyFor in the backend). */
export const CUSTOM_FONT_KEY = /^custom-[1-9]\d{0,8}$/;
/** Only files the backend stored under uploads/fonts/{id}/ ever reach a @font-face rule. */
const FONT_FILE = /^\/?uploads\/fonts\/\d+\/[A-Za-z0-9_-]+\.(woff2|woff|ttf|otf)$/;
const FONT_FORMAT: Record<string, string> = {
  woff2: 'woff2',
  woff: 'woff',
  ttf: 'truetype',
  otf: 'opentype',
};

/** True for a bundled font key or a well-formed uploaded-font key. */
export function isFontKey(key?: string | null): boolean {
  return Boolean(key && (BY_KEY.has(key) || CUSTOM_FONT_KEY.test(key)));
}

/**
 * Turns API rows into picker options. Rows with an unexpected key, category or file
 * path are dropped, so nothing the API returns can inject CSS.
 */
export function toCustomFontOptions(rows: CustomFontDto[] | null | undefined): FontOption[] {
  if (!Array.isArray(rows)) return [];
  return rows.flatMap((row) => {
    const id = Number(row?.id);
    const key = `custom-${id}`;
    const file = typeof row?.fileUrl === 'string' ? row.fileUrl.trim() : '';
    const category = FONT_CATEGORIES.includes(row?.category as FontCategory)
      ? (row.category as FontCategory)
      : 'general';
    if (!Number.isInteger(id) || id <= 0 || row.key !== key || !FONT_FILE.test(file)) return [];
    const en = (row.nameEn || row.nameFa || key).trim();
    return [{
      key,
      fa: (row.nameFa || en).trim(),
      en,
      category,
      family: `site-custom-${id}`,
      src: file.startsWith('/') ? file : `/${file}`,
    }];
  });
}

/** `@font-face` rules for uploaded fonts (only validated options get here). */
export function customFontFaceCss(fonts: FontOption[]): string {
  return fonts
    .filter((f) => f.family && f.src && FONT_FILE.test(f.src))
    .map((f) => {
      const ext = f.src!.split('.').pop()!.toLowerCase();
      return `@font-face{font-family:"${f.family}";src:url("${f.src}") format("${FONT_FORMAT[ext]}");font-display:swap}`;
    })
    .join('');
}

export function findFont(key?: string | null, custom: FontOption[] = []): FontOption | undefined {
  if (!key) return undefined;
  return BY_KEY.get(key) ?? custom.find((f) => f.key === key);
}

/** The chosen font, then IRANSans for any glyph it lacks. */
function stackFor(font: FontOption): string {
  const first = font.family
    ? `"${font.family}"`
    : font.key === 'iransans'
      ? "var(--IRANSans-font, 'IRANSans')"
      : `var(${font.cssVar})`;
  const chain = [first];
  if (font.key !== 'iransans') chain.push("var(--IRANSans-font, 'IRANSans')");
  return [...chain, 'Tahoma', 'sans-serif'].join(', ');
}

/**
 * The full `font-family` stack for a key: the chosen font, then the site's
 * default font for any glyph it lacks. Unknown keys give the default stack.
 */
export function fontStack(key?: string | null, custom: FontOption[] = []): string {
  return stackFor(findFont(key, custom) ?? BY_KEY.get(DEFAULT_FONT_KEY)!);
}

/** `font-family` stack for headings (--font-display); unknown keys give IRANSans. */
export function headingFontStack(key?: string | null, custom: FontOption[] = []): string {
  return stackFor(findFont(key, custom) ?? BY_KEY.get(DEFAULT_HEADING_FONT_KEY)!);
}
