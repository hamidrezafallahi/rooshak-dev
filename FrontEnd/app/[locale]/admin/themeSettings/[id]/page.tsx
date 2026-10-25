import React from 'react';

import { notFound } from 'next/navigation';

import {
  pickPalette,
  toThemeRecord,
} from '@components/templates/admin/themeSettings/palette';
import ThemeStudio from '@components/templates/admin/themeSettings/themeStudio';
import { getById } from '@lib/getAll';

export const dynamic = 'force-dynamic';

export default async function EditThemePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  if (!/^\d+$/.test(id)) notFound();

  const theme = toThemeRecord(await getById('themeSettings', id));
  if (!theme) notFound();

  return (
    <ThemeStudio
      id={theme.id}
      isActive={theme.isActive}
      initialName={theme.name}
      initialPalette={pickPalette(theme)}
    />
  );
}
