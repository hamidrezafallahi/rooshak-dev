import React from 'react';

import { notFound } from 'next/navigation';

import {
  pickPalette,
  pickTypography,
  toThemeRecord,
} from '@components/templates/admin/themeSettings/palette';
import ThemeStudio from '@components/templates/admin/themeSettings/themeStudio';
import { getAdminCustomFonts } from '@components/templates/admin/themeSettings/customFontsApi';
import { getById } from '@lib/getAll';

export const dynamic = 'force-dynamic';

export default async function EditThemePage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  if (!/^\d+$/.test(id)) notFound();

  const [row, customFonts] = await Promise.all([getById('themeSettings', id), getAdminCustomFonts()]);
  const theme = toThemeRecord(row);
  if (!theme) notFound();

  return (
    <ThemeStudio
      id={theme.id}
      isActive={theme.isActive}
      initialName={theme.name}
      initialPalette={pickPalette(theme)}
      initialFont={theme.fontFamily}
      initialHeadingFont={theme.headingFontFamily}
      initialTypography={pickTypography(theme)}
      customFonts={customFonts}
    />
  );
}
