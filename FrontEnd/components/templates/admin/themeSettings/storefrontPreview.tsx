'use client';

import React from 'react';

import { useTranslations } from 'next-intl';
import Image from 'next/image';

import {
  ColorKey,
  focusOutlineCss,
  isDarkColor,
  Palette,
  previewVars,
} from './palette';

type GlyphKind = 'bowl' | 'vase' | 'goblet';

/** Line drawings of crystal pieces, standing in for product photos. */
export function CrystalGlyph({ kind, className = '' }: { kind: GlyphKind; className?: string }) {
  return (
    <svg
      viewBox="0 0 64 64"
      fill="none"
      stroke="currentColor"
      strokeWidth={1.1}
      strokeLinejoin="round"
      strokeLinecap="round"
      className={className}
      aria-hidden
    >
      {kind === 'bowl' && (
        <>
          <path d="M8 24h48c-2 9-8 15-16 17H24C16 39 10 33 8 24Z" />
          <path d="M8 24c8 3 40 3 48 0" />
          <path d="M20 26l6 14M32 27v14M44 26l-6 14M14 25l6 12M50 25l-6 12" opacity={0.7} />
          <path d="M28 41v8h8v-8" />
          <path d="M20 55h24l-3-6H23z" />
        </>
      )}
      {kind === 'vase' && (
        <>
          <path d="M25 8h14M27 8c0 7-9 11-9 24 0 12 6 22 14 22s14-10 14-22c0-13-9-17-9-24" />
          <path d="M19 30h26M19 38h26M22 46h20" opacity={0.7} />
          <path d="M32 12v41" opacity={0.45} />
        </>
      )}
      {kind === 'goblet' && (
        <>
          <path d="M18 8h28l-3 18c-1 6-6 10-11 10s-10-4-11-10Z" />
          <path d="M22 10l4 24M42 10l-4 24M32 8v28" opacity={0.7} />
          <path d="M32 36v14M22 56h20l-3-6H25z" />
        </>
      )}
      <path
        d="M52 6l1.3 3.7L57 11l-3.7 1.3L52 16l-1.3-3.7L47 11l3.7-1.3z"
        fill="var(--t-highlight)"
        stroke="none"
      />
    </svg>
  );
}

/** The real Rooshak logo, multiplied onto light surfaces and inverted on dark ones (as globals.css does). */
function PreviewLogo({ surface, className }: { surface: string; className: string }) {
  const dark = isDarkColor(surface);
  return (
    <Image
      src="/brand/rooshak-logo.png"
      alt=""
      width={921}
      height={301}
      className={`h-auto ${className}`}
      style={
        dark
          ? { filter: 'invert(1) hue-rotate(180deg) brightness(2.6) contrast(1.15)', mixBlendMode: 'screen' }
          : { mixBlendMode: 'multiply' }
      }
    />
  );
}

const Icon = ({ d }: { d: string }) => (
  <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.5} className="w-4 h-4" aria-hidden>
    <path strokeLinecap="round" strokeLinejoin="round" d={d} />
  </svg>
);

const ICONS = {
  search: 'm21 21-5.2-5.2M10.5 18a7.5 7.5 0 1 0 0-15 7.5 7.5 0 0 0 0 15Z',
  user: 'M15.75 6a3.75 3.75 0 1 1-7.5 0 3.75 3.75 0 0 1 7.5 0ZM4.5 20.1a7.5 7.5 0 0 1 15 0',
  bag: 'M15.75 10.5V6a3.75 3.75 0 1 0-7.5 0v4.5m11.36-1.99 1.26 12A1.13 1.13 0 0 1 19.75 21H4.25a1.13 1.13 0 0 1-1.12-1.24l1.26-12A1.13 1.13 0 0 1 5.51 6.75h12.98c.58 0 1.06.43 1.12 1.01Z',
  menu: 'M3.75 6.75h16.5M3.75 12h16.5M3.75 17.25h16.5',
};

const STATUS: { key: ColorKey; msg: string; cssVar: string }[] = [
  { key: 'successColor', msg: 'alertSuccess', cssVar: '--t-success' },
  { key: 'errorColor', msg: 'alertError', cssVar: '--t-error' },
  { key: 'warningColor', msg: 'alertWarning', cssVar: '--t-warning' },
  { key: 'infoColor', msg: 'alertInfo', cssVar: '--t-info' },
];

type PreviewProps = {
  palette: Palette;
  device?: 'desktop' | 'mobile';
  /** Colour row currently hovered in the studio; its uses get outlined. */
  focusKey?: ColorKey | null;
};

/** A faithful, square-cornered miniature of the Rooshak storefront painted with `palette`. */
export default function StorefrontPreview({ palette, device = 'desktop', focusKey }: PreviewProps) {
  const t = useTranslations('admin.themeSettings.preview');
  const mobile = device === 'mobile';

  const products: { kind: GlyphKind; name: string; price: string; old?: string; badge?: 'new' | 'sale' }[] = [
    { kind: 'bowl', name: t('product1'), price: t('price1'), badge: 'new' },
    { kind: 'vase', name: t('product2'), price: t('price2'), old: t('oldPrice2'), badge: 'sale' },
    { kind: 'goblet', name: t('product3'), price: t('price3') },
  ];

  return (
    <div
      className={`rk-preview mx-auto overflow-hidden bg-[var(--t-surface)] text-[var(--t-text)] text-start transition-[max-width] duration-300 select-none ${
        mobile ? 'max-w-[360px] border border-[var(--t-border)]' : 'max-w-full'
      }`}
      style={previewVars(palette)}
      data-focus={focusKey ?? undefined}
      aria-hidden
    >
      <style>{focusOutlineCss('.rk-preview')}</style>

      <div
        data-c="primaryColor"
        className="bg-[var(--t-primary)] px-3 py-1.5 text-[10.5px] text-[var(--t-surface)] text-center"
      >
        {t('announcement')}
      </div>

      <div
        data-c="surfaceColor borderColor"
        className="items-center gap-2 grid grid-cols-[1fr_auto_1fr] bg-[var(--t-surface)] px-3 sm:px-4 border-[var(--t-border)] border-b h-12"
      >
        <nav className="flex items-center gap-3 min-w-0 text-[11px] whitespace-nowrap">
          {mobile ? (
            <Icon d={ICONS.menu} />
          ) : (
            <>
              <span className="relative pb-0.5 after:absolute after:inset-x-0 after:-bottom-0.5 after:bg-current after:h-px font-medium">
                {t('nav1')}
              </span>
              <span className="hidden sm:inline">{t('nav2')}</span>
              <span className="hidden xl:inline">{t('nav3')}</span>
            </>
          )}
        </nav>
        <PreviewLogo surface={palette.surfaceColor} className={mobile ? 'w-20' : 'w-24'} />
        <div className="flex justify-end items-center gap-2.5">
          {!mobile && <Icon d={ICONS.search} />}
          <Icon d={ICONS.user} />
          <span className="relative">
            <Icon d={ICONS.bag} />
            <span
              data-c="primaryColor"
              className="-top-1.5 -end-1.5 absolute place-items-center grid bg-[var(--t-primary)] rounded-full w-3.5 h-3.5 text-[8px] text-[var(--t-surface)]"
            >
              2
            </span>
          </span>
        </div>
      </div>

      <section
        data-c="neutralColor"
        className={`grid items-center gap-5 bg-[var(--t-neutral)] px-4 sm:px-6 py-6 ${
          mobile ? 'grid-cols-1' : 'grid-cols-[1.15fr_1fr]'
        }`}
      >
        <div className="min-w-0">
          <p data-c="highlightColor" className="font-medium text-[11px] text-[var(--t-highlight)]">
            {t('eyebrow')}
          </p>
          <p data-c="textColor" className="mt-1.5 font-semibold text-[var(--t-text)] text-lg leading-8">
            {t('heroTitle')}
          </p>
          <p data-c="textMutedColor" className="mt-1.5 text-[11.5px] text-[var(--t-text-muted)] leading-6">
            {t('heroText')}
          </p>
          <div className="flex flex-wrap gap-2 mt-4">
            <span
              data-c="primaryColor"
              className="bg-[var(--t-primary)] px-4 py-2 border border-[var(--t-primary)] text-[11px] text-[var(--t-surface)]"
            >
              {t('shopNow')}
            </span>
            <span className="px-4 py-2 border border-[var(--t-text)] text-[11px] text-[var(--t-text)]">
              {t('explore')}
            </span>
          </div>
        </div>
        {!mobile && (
          <div
            data-c="surfaceMutedColor"
            className="relative place-items-center grid bg-[var(--t-surface-muted)] aspect-[4/3] text-[var(--t-text)]"
          >
            <CrystalGlyph kind="bowl" className="w-2/3 h-2/3" />
            <span
              data-c="highlightColor"
              className="bottom-3 absolute inset-x-6 bg-[var(--t-highlight)] h-px"
            />
          </div>
        )}
      </section>

      <div className="flex gap-2 px-4 pt-4 overflow-hidden">
        {[t('chip1'), t('chip2'), t('chip3'), t('chip4')].map((chip, i) => (
          <span
            key={chip}
            data-c={i === 0 ? 'textColor' : 'secondaryColor'}
            className={`shrink-0 px-3 py-1 text-[10.5px] ${
              i === 0
                ? 'bg-[var(--t-text)] text-[var(--t-surface)]'
                : 'bg-[var(--t-secondary)] text-[var(--t-text)]'
            }`}
          >
            {chip}
          </span>
        ))}
      </div>

      <div className={`grid gap-3 px-4 py-4 ${mobile ? 'grid-cols-2' : 'grid-cols-3'}`}>
        {(mobile ? products.slice(0, 2) : products).map((p) => (
          <article key={p.name} className="min-w-0">
            <div
              data-c="surfaceMutedColor"
              className="relative place-items-center grid bg-[var(--t-surface-muted)] aspect-[4/5] text-[var(--t-text)]"
            >
              <CrystalGlyph kind={p.kind} className="w-3/5 h-3/5" />
              {p.badge === 'new' && (
                <span className="top-2 absolute bg-[var(--t-surface)] px-1.5 py-0.5 font-medium text-[9px] text-[var(--t-text)] start-2">
                  {t('badgeNew')}
                </span>
              )}
              {p.badge === 'sale' && (
                <span
                  data-c="errorColor"
                  className="top-2 absolute bg-[var(--t-error)] px-1.5 py-0.5 font-medium text-[9px] text-[var(--t-surface)] start-2"
                >
                  {t('badgeSale')}
                </span>
              )}
            </div>
            <p className="mt-2 text-[11.5px] truncate">{p.name}</p>
            <p className="flex flex-wrap items-baseline gap-x-1.5 text-[11px]">
              <span
                data-c={p.old ? 'errorColor' : undefined}
                className={`font-medium ${p.old ? 'text-[var(--t-error)]' : ''}`}
              >
                {p.price}
              </span>
              {p.old && (
                <span data-c="textMutedColor" className="text-[9.5px] text-[var(--t-text-muted)] line-through">
                  {p.old}
                </span>
              )}
            </p>
          </article>
        ))}
      </div>

      <div className={`grid gap-2 px-4 pb-4 ${mobile ? 'grid-cols-1' : 'grid-cols-2'}`}>
        {STATUS.map((s) => (
          <p
            key={s.key}
            data-c={s.key}
            className="flex items-center gap-2 px-2.5 py-2 border-s-2 text-[10.5px]"
            style={{
              borderColor: `var(${s.cssVar})`,
              color: `var(${s.cssVar})`,
              background: `color-mix(in srgb, var(${s.cssVar}) 9%, var(--t-surface))`,
            }}
          >
            <span className="rounded-full w-1.5 h-1.5 shrink-0" style={{ background: `var(${s.cssVar})` }} />
            {t(s.msg)}
          </p>
        ))}
      </div>

      <footer
        data-c="borderColor textMutedColor"
        className="flex justify-between items-center gap-3 px-4 py-3 border-[var(--t-border)] border-t text-[10px] text-[var(--t-text-muted)]"
      >
        <span>{t('footer')}</span>
        <span className="flex gap-3">
          <span>{t('footerLink1')}</span>
          <span>{t('footerLink2')}</span>
        </span>
      </footer>
    </div>
  );
}

/** Card-sized thumbnail of the storefront: no text, just the theme's structure and colours. */
export function StorefrontThumb({ palette }: { palette: Palette }) {
  const kinds: GlyphKind[] = ['bowl', 'vase', 'goblet'];
  return (
    <div
      className="flex flex-col bg-[var(--t-surface)] h-full overflow-hidden text-[var(--t-text)] select-none"
      style={previewVars(palette)}
      aria-hidden
    >
      <div className="place-items-center grid bg-[var(--t-primary)] h-3.5 shrink-0">
        <span className="bg-[var(--t-surface)] opacity-80 w-16 h-[2px]" />
      </div>
      <div className="items-center grid grid-cols-[1fr_auto_1fr] px-3 border-[var(--t-border)] border-b h-9 shrink-0">
        <span className="flex gap-1.5">
          <i className="bg-[var(--t-text)] w-4 h-[2px]" />
          <i className="bg-[var(--t-text)] opacity-50 w-4 h-[2px]" />
        </span>
        <PreviewLogo surface={palette.surfaceColor} className="w-14" />
        <span className="flex justify-end gap-1">
          <i className="border border-[var(--t-text)] rounded-full w-2 h-2" />
          <i className="bg-[var(--t-primary)] rounded-full w-2 h-2" />
        </span>
      </div>
      <div className="flex-[5] items-center gap-3 grid grid-cols-[1.2fr_1fr] bg-[var(--t-neutral)] px-3 py-2.5 min-h-0">
        <div className="flex flex-col gap-1.5">
          <i className="bg-[var(--t-highlight)] w-1/4 h-[2px]" />
          <i className="bg-[var(--t-text)] w-4/5 h-1.5" />
          <i className="bg-[var(--t-text-muted)] w-3/5 h-[2px]" />
          <span className="flex gap-1 mt-1">
            <i className="bg-[var(--t-primary)] w-9 h-3" />
            <i className="border border-[var(--t-text)] w-9 h-3" />
          </span>
        </div>
        <div className="place-items-center grid bg-[var(--t-surface-muted)] h-full max-h-full text-[var(--t-text)]">
          <CrystalGlyph kind="bowl" className="w-1/2 h-4/5" />
        </div>
      </div>
      <div className="flex-[6] gap-2 grid grid-cols-3 bg-[var(--t-surface)] px-3 pt-2.5 pb-2 min-h-0">
        {kinds.map((kind, i) => (
          <div key={kind} className="flex flex-col gap-1 min-h-0">
            <div className="relative place-items-center grid bg-[var(--t-surface-muted)] min-h-0 text-[var(--t-text)] grow">
              <CrystalGlyph kind={kind} className="w-2/5 h-3/5" />
              {i === 1 && <i className="top-1 absolute bg-[var(--t-error)] w-3 h-1.5 start-1" />}
            </div>
            <i className="bg-[var(--t-text)] opacity-80 w-4/5 h-[2px] shrink-0" />
            <i className={`w-2/5 h-[2px] shrink-0 ${i === 1 ? 'bg-[var(--t-error)]' : 'bg-[var(--t-text-muted)]'}`} />
          </div>
        ))}
      </div>
    </div>
  );
}
