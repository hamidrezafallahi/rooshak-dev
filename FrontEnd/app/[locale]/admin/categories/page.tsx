import React from 'react';

import { ITreeContext } from '@components/atoms/defaultElements/tree';
import AdminCategoryTemplate from '@components/templates/admin/categories';
import {
  getAll,
  getFormConfigByEntityName,
} from '@lib/getAll';

export const dynamic = 'force-dynamic';

export default async function Page({
  searchParams,
}: {
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const resolvedSearchParams = searchParams ? await searchParams : undefined;

  const page = parseInt((resolvedSearchParams?.page as string) ?? '1');
  const pageSize = parseInt(
    (resolvedSearchParams?.pageSize as string) ?? '10000',
  );

  // EntityName/EndPoint در DB به‌صورت lowercase است (categories).
  // lookup فرم case-sensitive است؛ Categories ≠ categories → form not found.
  const list = await getAll<ITreeContext>('categories', {
    page,
    pageSize,
    byConfig: true,
    onlyActives: false,
    revalidate: false,
  });

  const res = await getFormConfigByEntityName('categories');

  if (!res) {
    return (
      <div className="admin-page">
        <div className="admin-panel admin-empty">
          <p className="admin-empty-title">Form configuration not found</p>
        </div>
      </div>
    );
  }

  return (
    <AdminCategoryTemplate
      categories={list?.data.records ?? []}
      entityFormConfig={res}
    />
  );
}
