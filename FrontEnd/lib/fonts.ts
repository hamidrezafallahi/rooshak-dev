import {
  Amiri,
  Aref_Ruqaa,
  Cairo,
  Changa,
  El_Messiri,
  Harmattan,
  Lalezar,
  Lateef,
  Lemonada,
  Mada,
  Markazi_Text,
  Noto_Naskh_Arabic,
  Noto_Nastaliq_Urdu,
  Noto_Sans_Arabic,
  Rakkas,
  Readex_Pro,
  Reem_Kufi,
  Scheherazade_New,
  Tajawal,
  Vazirmatn,
} from 'next/font/google';
import localFont from 'next/font/local';

// Rooshak's default typeface is IRANSans (body and headings). The other fonts
// are selectable in admin → تم سایت. next/font downloads them at build time and
// serves them from /_next/static/media, so no request goes to Google Fonts.
const markaziText = Markazi_Text({
  subsets: ['arabic', 'latin'],
  weight: ['400', '500', '600'],
  display: 'swap',
  preload: false,
  variable: '--font-markazi',
});

const vazirmatn = Vazirmatn({
  subsets: ['arabic', 'latin'],
  weight: ['300', '400', '500', '600'],
  display: 'swap',
  preload: false,
  variable: '--font-vazirmatn',
});

export const iranSansFont = localFont({
  src: [{ path: '../public/fonts/IRANSansWeb(FaNum).woff', weight: '400', style: 'normal' }],
  variable: '--IRANSans-font',
  display: 'swap',
});

// ===== Selectable main fonts (admin → تم سایت → فونت اصلی), see lib/fontCatalog.ts =====
// Not preloaded: the browser downloads a font file only when the active theme
// (or the admin's font preview) actually renders text in it.
const notoSansArabic = Noto_Sans_Arabic({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-noto-sans-arabic' });
const harmattan = Harmattan({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-harmattan' });
const mada = Mada({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-mada' });
const tajawal = Tajawal({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-tajawal' });
const cairo = Cairo({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-cairo' });
const readexPro = Readex_Pro({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-readex-pro' });
const notoNaskhArabic = Noto_Naskh_Arabic({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-noto-naskh-arabic' });
const amiri = Amiri({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-amiri' });
const scheherazadeNew = Scheherazade_New({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-scheherazade-new' });
const lateef = Lateef({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-lateef' });
const lalezar = Lalezar({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: '400', variable: '--font-lalezar' });
const reemKufi = Reem_Kufi({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-reem-kufi' });
const elMessiri = El_Messiri({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-el-messiri' });
const changa = Changa({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-changa' });
const arefRuqaa = Aref_Ruqaa({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: ['400', '700'], variable: '--font-aref-ruqaa' });
const rakkas = Rakkas({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, weight: '400', variable: '--font-rakkas' });
const lemonada = Lemonada({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-lemonada' });
const notoNastaliqUrdu = Noto_Nastaliq_Urdu({ subsets: ['arabic', 'latin'], display: 'swap', preload: false, variable: '--font-noto-nastaliq-urdu' });

export const fontVariables = [
  iranSansFont,
  markaziText,
  vazirmatn,
  notoSansArabic,
  harmattan,
  mada,
  tajawal,
  cairo,
  readexPro,
  notoNaskhArabic,
  amiri,
  scheherazadeNew,
  lateef,
  lalezar,
  reemKufi,
  elMessiri,
  changa,
  arefRuqaa,
  rakkas,
  lemonada,
  notoNastaliqUrdu,
]
  .map((font) => font.variable)
  .join(' ');
