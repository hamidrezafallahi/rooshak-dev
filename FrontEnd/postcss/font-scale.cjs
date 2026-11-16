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
 */
const ABSOLUTE = /(\d|\))\s*(px|rem|vw|vh|vmin|vmax|svh|dvh|pt)\b/i;
const RELATIVE = /(^|[^r])em\b|%/i;

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
  },
});
module.exports.postcss = true;
