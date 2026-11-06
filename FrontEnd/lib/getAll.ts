import { PagedResponse } from '@models/base';

import { serverApiBaseUrl } from './api';
import {
  ApiRequestError,
  describeStatus,
  errorTextOf,
  readApiResult,
} from './apiError';
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

/**
 * Storefront calls answer a failure with an empty page whose `error` the page shows: these
 * pages also render at build time, when there may be no API at all. Admin calls throw
 * ApiRequestError instead, so error.tsx explains it and offers a retry.
 */
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

  const fail = (message: string): PagedResponse<T> => {
    if (isAdmin) throw new ApiRequestError(message);
    return emptyPage<T>(message);
  };

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
    console.error(`getAll: no answer from ${url}`, e);
    return fail(describeStatus(503));
  }

  // Outside the try: redirect() works by throwing.
  if (res.status === 401 && isAdmin) {
    await handleServerUnauthorized();
  }

  let body: PagedResponse<T>;
  try {
    body = JSON.parse(text) as PagedResponse<T>;
  } catch (e) {
    console.error(`Invalid JSON response from ${url}`, e);
    return fail(describeStatus(res.ok ? 'PARSING_ERROR' : res.status));
  }

  if (!res.ok || body?.isSuccess === false) {
    console.error(`getAll failed: ${res.status} ${url}`, text.slice(0, 300));
    return fail(errorTextOf(body) ?? describeStatus(res.ok ? 500 : res.status));
  }

  return body;
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
  }).catch((e) => {
    console.error(`getById: no answer for ${entity}/${id}`, e);
    return null;
  });
  if (!res) {
    throw new ApiRequestError(describeStatus(503));
  }
  if (res.status === 401) {
    await handleServerUnauthorized();
  }
  const result = await readApiResult<Record<string, any>>(res);
  if (!result.isSuccess) {
    throw new ApiRequestError(result.error ?? describeStatus(res.status));
  }
  return result.data;
}

/** Throws ApiRequestError: no page can be drawn without its form configuration. */
export async function getFormConfigByEntityName(entity: string) {
  // Prefer EndPoint/EntityName casing from DB (usually camelCase starting lower).
  // Still pass through as-is; backend lookup is case-insensitive as a safety net.
  const url = `${serverApiBaseUrl}/EntityConfigs/entityFormConfig/${encodeURIComponent(entity)}`;
  const res = await fetch(url, { cache: 'no-store' }).catch((e) => {
    console.error(`getFormConfigByEntityName: no answer for ${entity}`, e);
    return null;
  });
  if (!res) {
    throw new ApiRequestError(describeStatus(503));
  }
  const result = await readApiResult<any>(res);
  if (!result.isSuccess || !result.data) {
    console.error(`getFormConfigByEntityName failed for ${entity}:`, result.error);
    throw new ApiRequestError(result.error ?? describeStatus(404));
  }
  return result.data;
}
