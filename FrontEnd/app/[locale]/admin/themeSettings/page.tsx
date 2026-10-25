import React from 'react';

import ThemeGallery from '@components/templates/admin/themeSettings';
import {
  ThemeRecord,
  toThemeRecord,
} from '@components/templates/admin/themeSettings/palette';
import { getAll } from '@lib/getAll';

export const dynamic = 'force-dynamic';

/** Dedicated page instead of the generic [field] table: themes are visual, so they are shown as previews. */
export default async function ThemeSettingsPage() {
  const res = await getAll<Record<string, unknown>>('themeSettings', {
    page: 1,
    pageSize: 100,
    onlyActives: false,
    revalidate: false,
  });

  const themes = res.isSuccess
    ? (res.data?.records ?? [])
        .map(toThemeRecord)
        .filter((theme): theme is ThemeRecord => theme !== null)
    : null;

  return <ThemeGallery themes={themes} />;
}
