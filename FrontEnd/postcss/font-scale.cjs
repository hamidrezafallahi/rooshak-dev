/**
 * PostCSS plugin: makes every fixed font size follow the admin's text-size setting.
 *
 * Each `font-size` with an absolute value (px, rem, vw, clamp(...) ...) becomes
 * `calc(<value> * var(--text-scale, 1))`. `--text-scale` is set on :root from the
 * active theme (lib/theme.ts), and headings re-point it to `--heading-scale`
 * (see globals.css), so body text and titles scale independently. This covers
 * Tailwind utilities, arbitrary values like text-[13px] and hand-written CSS alike.
 *
 * Left alone: values that already use a variable, relative units (em, %) that
 * inherit the scale anyway, and keywords (inherit, larger ...).
 *
 * Font weights work the same way: `font-weight: 500` becomes
 * `calc(500 + var(--weight-shift, 0))`, where --weight-shift is the admin's chosen
 * weight minus the design weight (lib/typography.ts). Every weight moves by the same
 * step, so bold stays bolder than regular. @font-face descriptors are never touched.
 */
const ABSOLUTE = /(\d|\))\s*(px|rem|vw|vh|vmin|vmax|svh|dvh|pt)\b/i;
const RELATIVE = /(^|[^r])em\b|%/i;
const WEIGHT_KEYWORDS = { normal: '400', bold: '700' };

const insideFontFace = (decl) => {
  for (let node = decl.parent; node; node = node.parent) {
    if (node.type === 'atrule' && node.name.toLowerCase() === 'font-face') return true;
  }
  return false;
};

module.exports = () => ({
  postcssPlugin: 'font-scale',
  Declaration: {
    'font-size': (decl) => {
      const value = decl.value.trim();
      if (!value || value.includes('var(') || value.includes('--text-scale')) return;
      if (!ABSOLUTE.test(value) || RELATIVE.test(value)) return;
      const important = decl.important;
      decl.value = `calc(${value} * var(--text-scale, 1))`;
      decl.important = important;
    },
    'font-weight': (decl) => {
      if (insideFontFace(decl)) return;
      const raw = decl.value.trim().toLowerCase();
      const value = WEIGHT_KEYWORDS[raw] ?? raw;
      if (!/^\d{3}$/.test(value)) return;
      const important = decl.important;
      decl.value = `calc(${value} + var(--weight-shift, 0))`;
      decl.important = important;
    },
  },
});
module.exports.postcss = true;
