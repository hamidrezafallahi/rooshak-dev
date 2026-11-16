/** @type {import('postcss-load-config').Config} */
const config = {
  plugins: {
    tailwindcss: {},
    // Runs after Tailwind so its utilities (text-sm, text-[13px] ...) are scaled too.
    './postcss/font-scale.cjs': {},
  },
};

export default config;
