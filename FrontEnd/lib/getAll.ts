import {
  ApiResponse,
  PagedResponse,
} from '@models/base';

import { serverApiBaseUrl } from './api';
import {
  authHeaders,
  handleServerUnauthorized,
} from './server-auth';

function emptyPage<T>(error: string): PagedResponse<T> {
  return {
    isSuccess: false,
    error,
    data: {
      records: [],
      columnsJson: '',
      actionsJson: '',
      totalCount: 0,
      pageNumber: 0,
      pageSize: 0,
      totalPages: 0,
    },
  };
}

export async function getAll<T>(
  entity: string,
  {
    page,
    pageSize,
    byConfig,
    filter,
    onlyActives,
    /** Storefront ISR default 60s; pass `false` for admin (always fresh, sent with the session). */
    revalidate = 60,
  }: {
    page?: number;
    pageSize?: number;
    byConfig?: boolean;
    onlyActives?: boolean;
    filter?: string;
    revalidate?: number | false;
  } = {}
): Promise<PagedResponse<T>> {
  const params = new URLSearchParams();
  if (page !== undefined) params.append("page", String(page));
  if (pageSize !== undefined) params.append("pageSize", String(pageSize));
  if (byConfig !== undefined) params.append("byConfig", String(byConfig));
  if (filter !== undefined) params.append("q", String(filter));
  if (onlyActives !== undefined) params.append("onlyActives", String(onlyActives));
  const url = `${serverApiBaseUrl}/${entity}?${params.toString()}`;
  // Admin lists may need the logged-in user. Storefront calls stay cookie-free:
  // reading cookies would turn their ISR pages dynamic.
  const isAdmin = revalidate === false;

  let res: Response;
  let text: string;
  try {
    res = await fetch(
      url,
      isAdmin
        ? { cache: 'no-store', headers: await authHeaders(), next: { tags: [entity] } }
        : { next: { revalidate, tags: [entity] } },
    );
    text = await res.text();
  } catch (e) {
    console.error(`Invalid JSON response from ${url}`, e);
    return emptyPage(`Invalid JSON response from ${url}`);
  }

  // Outside the try: redirect() works by throwing.
  if (res.status === 401 && isAdmin) {
    await handleServerUnauthorized();
  }

  if (!res.ok) {
    console.error(`getAll failed: ${res.status} ${url}`, text.slice(0, 300));
    return emptyPage(`HTTP ${res.status} from ${url}`);
  }

  try {
    return JSON.parse(text) as PagedResponse<T>;
  } catch (e) {
    console.error(`Invalid JSON response from ${url}`, e);
    return emptyPage(`Invalid JSON response from ${url}`);
  }
}



export async function getById(entity: string, id: string): Promise<Record<string, any>> {
  // An expired candyAccess is refreshed in middleware.ts before this render,
  // because a Server Component cannot write the rotated cookies itself.
  const res = await fetch(`${serverApiBaseUrl}/${entity}/${id}`, {
    headers: {
      ...(await authHeaders()),
      "Content-Type": 'application/json'
    },
    cache: "no-store",
  });
  if (res.status === 401) {
    await handleServerUnauthorized();
    throw new Error(`HTTP 401 from ${entity}/${id}`);
  }
  const data: ApiResponse<any> = await res.json();
  return data.data;
}
export async function getFormConfigByEntityName(entity: string) {
  // Prefer EndPoint/EntityName casing from DB (usually camelCase starting lower).
  // Still pass through as-is; backend lookup is case-insensitive as a safety net.
  const url = `${serverApiBaseUrl}/EntityConfigs/entityFormConfig/${encodeURIComponent(entity)}`;
  try {
    const res = await fetch(url, { cache: 'no-store' });
    if (!res.ok) {
      console.error(`getFormConfigByEntityName failed: ${res.status} ${url}`);
      return null;
    }
    const data = await res.json();
    if (!data?.isSuccess || !data?.data) {
      console.error(
        `getFormConfigByEntityName empty for ${entity}:`,
        data?.error ?? 'no data',
      );
      return null;
    }
    return data.data;
  } catch (e) {
    console.error(`getFormConfigByEntityName error for ${entity}`, e);
    return null;
  }
}
