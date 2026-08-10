import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { SimpleResponse } from '@models/base';

type SlugRecord = {
  slug?: string | number | null;
  id?: string | number | null;
};

function toSlugParam(item: SlugRecord): string {
  const slug = String(item.slug ?? '').trim();
  if (slug) return slug;
  const id = item.id;
  return id == null ? '' : String(id).trim();
}

async function fetchSlugRecords(endpoint: string): Promise<SlugRecord[]> {
  const result = await safeFetchJson<SimpleResponse<SlugRecord[] | null>>(
    `${serverApiBaseUrl}/${endpoint}`,
    { next: { revalidate: 60 } },
  );

  if (!result.ok || !result.data) return [];
  if (result.data.isSuccess === false) return [];

  const data = result.data.data;
  return Array.isArray(data) ? data : [];
}

/**
 * Builds `{ slug }` params for App Router SSG.
 * Prefers getslugs; optionally falls back to getids / another endpoint.
 */
export async function fetchStaticSlugParams(
  primaryEndpoint: string,
  fallbackEndpoint?: string,
): Promise<{ slug: string }[]> {
  try {
    let records = await fetchSlugRecords(primaryEndpoint);

    if (records.length === 0 && fallbackEndpoint) {
      records = await fetchSlugRecords(fallbackEndpoint);
    }

    return records
      .map((item) => ({ slug: toSlugParam(item) }))
      .filter((item) => Boolean(item.slug));
  } catch (error) {
    console.error(`Error generating static params for ${primaryEndpoint}:`, error);
    return [];
  }
}
