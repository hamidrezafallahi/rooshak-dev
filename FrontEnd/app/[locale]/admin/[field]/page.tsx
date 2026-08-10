import React from 'react';

import AdminList from '@components/templates/admin/adminList';
import { getAll } from '@lib/getAll';

export const dynamic = "force-dynamic";

export default async function Page({
  params,
  searchParams,
}: {
  params: Promise<{ locale: string; field: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const { field: entity } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  
  const page = parseInt((resolvedSearchParams?.page as string) ?? "1");
  const pageSizeNumber = parseInt((resolvedSearchParams?.pageSize as string) ?? "10");
  const filter = resolvedSearchParams?.q as string;

  const list = await getAll(entity, {
    page: page,
    pageSize: pageSizeNumber,
    byConfig: true,
    onlyActives: false,
    filter,
    revalidate: false,
  });

  return (
    <AdminList
      list={
        list?.data as
          | {
              records?: Record<string, unknown>[];
              actionsJson?: string;
              columnsJson?: string;
              totalCount?: number;
            }
          | undefined
      }
      entity={entity}
    />
  );
}