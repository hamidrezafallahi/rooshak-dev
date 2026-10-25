'use client';

import React from 'react';

import { useTranslations } from 'next-intl';

import {
  COLOR_GROUPS,
  Palette,
} from './palette';

const GROUP_WEIGHT = { brand: 3, canvas: 2, status: 1 } as const;

/** Every colour of a palette as one faceted band, brand colours widest. */
export function PrismBar({ palette, className = 'h-3' }: { palette: Palette; className?: string }) {
  const t = useTranslations('admin.themeSettings.fields');
  return (
    <div className={`relative flex rounded-full overflow-hidden ring-1 ring-black/5 ${className}`}>
      {COLOR_GROUPS.flatMap((group) =>
        group.keys.map((key) => (
          <span
            key={key}
            title={`${t(`${key}.label`)} · ${palette[key]}`}
            style={{ background: palette[key], flexGrow: GROUP_WEIGHT[group.id] }}
            className="basis-0"
          />
        )),
      )}
      <span className="absolute inset-0 bg-[linear-gradient(115deg,rgba(255,255,255,0.45)_0%,transparent_38%,transparent_62%,rgba(255,255,255,0.25)_100%)] pointer-events-none" />
    </div>
  );
}

export function SwatchStack({ colors }: { colors: string[] }) {
  return (
    <span className="flex -space-x-2 rtl:space-x-reverse shrink-0" aria-hidden>
      {colors.map((c, i) => (
        <span
          key={i}
          className="border-[var(--admin-surface-elevated)] border-2 rounded-full ring-1 ring-black/10 w-6 h-6"
          style={{ background: c }}
        />
      ))}
    </span>
  );
}

export function LiveBadge({ label }: { label: string }) {
  return (
    <span className="inline-flex items-center gap-1.5 bg-[color-mix(in_srgb,var(--success-color)_14%,white)] px-2.5 py-1 rounded-full font-medium text-[var(--success-color)] text-xs">
      <span className="relative flex w-2 h-2">
        <span className="absolute inset-0 bg-[var(--success-color)] opacity-60 rounded-full motion-safe:animate-ping" />
        <span className="relative bg-[var(--success-color)] rounded-full w-2 h-2" />
      </span>
      {label}
    </span>
  );
}
