import 'server-only';

import { PagedResponse } from '@models/base';
import {
  CategoryRequestQueries,
  ICategory,
} from '@models/category';

import { serverApiBaseUrl } from './api';
import { describeStatus, readApiResult } from './apiError';

const emptyCategoryQueries: CategoryRequestQueries['queries'] = {};

export async function getCategories(
  { queries }: CategoryRequestQueries = { queries: emptyCategoryQueries }
) {
  const { ByConfig, IsShowInLanding, Page, PageSize } = (await queries) || {};
  const params = new URLSearchParams();
  if (Page !== undefined) params.append("page", Page.toString());
  if (PageSize !== undefined) params.append("PageSize", PageSize.toString());
  if (ByConfig !== undefined) params.append("ByConfig", ByConfig.toString());
  if (IsShowInLanding !== undefined) params.append("IsShowInLanding", IsShowInLanding.toString());
  const url = `${serverApiBaseUrl}/Categories?${params.toString()}`;
  try {
    const res = await fetch(url, {
      next: { revalidate: 60, tags: ['categories'] },
    });
    const result = await readApiResult<PagedResponse<ICategory>['data']>(res);
    if (!result.isSuccess) {
      console.error('getCategories failed', { url, status: res.status, error: result.error });
    }
    return result;
  } catch (error) {
    console.error('getCategories: no answer', { url }, error);
    return { isSuccess: false, data: null, error: describeStatus(503) };
  }
}
