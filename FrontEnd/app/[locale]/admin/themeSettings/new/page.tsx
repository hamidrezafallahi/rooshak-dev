import React from 'react';

import { getTranslations } from 'next-intl/server';

import {
  DEFAULT_PALETTE,
  pickPalette,
  toThemeRecord,
} from '@components/templates/admin/themeSettings/palette';
import ThemeStudio from '@components/templates/admin/themeSettings/themeStudio';
import { getById } from '@lib/getAll';

export const dynamic = 'force-dynamic';

/** `?from=<id>` starts the new theme as a copy of an existing one. */
export default async function NewThemePage({
  searchParams,
}: {
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const from = (await searchParams)?.from;
  const source =
    typeof from === 'string' && /^\d+$/.test(from)
      ? toThemeRecord(await getById('themeSettings', from))
      : null;
  const t = await getTranslations('admin.themeSettings');

  return (
    <ThemeStudio
      initialName={source ? t('copyName', { name: source.name }) : ''}
      initialPalette={source ? pickPalette(source) : DEFAULT_PALETTE}
    />
  );
}
