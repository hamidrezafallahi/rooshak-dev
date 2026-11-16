import 'server-only';

import { type CustomFontDto, type FontOption, toCustomFontOptions } from '@lib/fontCatalog';
import { getAll } from '@lib/getAll';

/**
 * Active uploaded fonts for the theme admin, always fresh (not the storefront cache),
 * so a font added a moment ago shows up in the picker right away.
 */
export async function getAdminCustomFonts(): Promise<FontOption[]> {
  try {
    const res = await getAll<CustomFontDto>('customFonts', {
      page: 1,
      pageSize: 200,
      onlyActives: true,
      revalidate: false,
    });
    return res.isSuccess ? toCustomFontOptions(res.data?.records ?? []) : [];
  } catch {
    // The picker still works with the bundled fonts.
    return [];
  }
}
