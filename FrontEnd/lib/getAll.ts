import { cookies } from 'next/headers';

import {
  ApiResponse,
  PagedResponse,
} from '@models/base';

import { serverApiBaseUrl } from './api';

export async function getAll<T>(
  entity: string,
  { page, pageSize, byConfig, filter, onlyActives }: { page?: number; pageSize?: number; byConfig?: boolean, onlyActives?: boolean, filter?: string } = {}
): Promise<PagedResponse<T>> {
  const params = new URLSearchParams();
  if (page !== undefined) params.append("page", String(page));
  if (pageSize !== undefined) params.append("pageSize", String(pageSize));
  if (byConfig !== undefined) params.append("byConfig", String(byConfig));
  if (filter !== undefined) params.append("q", String(filter));
  if (onlyActives !== undefined) params.append("onlyActives", String(onlyActives));
  const url = `${serverApiBaseUrl}/${entity}?${params.toString()}`;
  try {
    const res = await fetch(url, { cache: "no-store", next: { tags: [entity] } });

    const text = await res.text();

    if (!res.ok) {
      console.error(`getAll failed: ${res.status} ${url}`, text.slice(0, 300));
      return {
        isSuccess: false,
        error: `HTTP ${res.status} from ${url}`,
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

    return JSON.parse(text) as PagedResponse<T>;
  } catch (e) {
    console.error(`Invalid JSON response from ${url}`, e);
    return {
      isSuccess: false, error: `Invalid JSON response from ${url}`, data: {
        records: [],
        columnsJson: '',
        actionsJson: '',
        totalCount: 0,
        pageNumber: 0,
        pageSize: 0,
        totalPages: 0
      }
    };
  }
}



export async function getById(entity: string, id: string): Promise<Record<string, any>> {
  const cookieStore = await cookies();
  const token = cookieStore.get('candyAccess')?.value;
  const res = await fetch(`${serverApiBaseUrl}/${entity}/${id}`, {
 headers: {
      "Authorization": `Bearer ${token || ''}`,
      "Content-Type": 'application/json'
    },




    //     next: {
    //       revalidate: 1,
    //       tags: ["genderFilter", "statusFilter", "speciesFilter"],
    //     },
    //   }


    //, {
      cache: "no-store",
    });
   const data: Promise<ApiResponse<any>> = await res.json();
  return (await data).data;
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

