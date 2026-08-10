import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import CustomPagination from '@components/molecules/pagination';
import EmptyState from '@components/molecules/storefront/EmptyState';
import EntityGrid from '@components/molecules/storefront/EntityGrid';
import PageHeader from '@components/molecules/storefront/PageHeader';
import SupplierCard from '@components/molecules/supplierCard';
import { getAll } from '@lib/getAll';
import { buildPageMetadata } from '@lib/seo';
import { IUser } from '@models/user';

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export const revalidate = 60;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'suppliersPage' });

  return buildPageMetadata({
    locale,
    path: 'suppliers',
    title: t('title'),
    description: t('description'),
  });
}

export default async function Page({ params, searchParams }: Props) {
  const { locale } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const t = await getTranslations({ locale, namespace: 'suppliersPage' });
  const pageNumber = parseInt(
    ((resolvedSearchParams?.page ?? resolvedSearchParams?.Page) as string) ??
      '1'
  );
  const pageSize = 12;

  const response = await getAll<IUser>('productOffers/suppliers', {
    page: pageNumber,
    pageSize,
    byConfig: false,
  });

  const suppliers: IUser[] = response?.data.records ?? [];
  const totalCount = response?.data.totalCount ?? 0;
  const currentPage = response?.data.pageNumber ?? pageNumber;

  return (
    <article className="store-page !pt-6">
      <PageHeader title={t('title')} description={t('description')} />

      {suppliers.length === 0 ? (
        <EmptyState title={t('empty')} description={t('emptyHint')} />
      ) : (
        <EntityGrid cols="cards">
          {suppliers.map((s) => (
            <SupplierCard supplier={s} key={s.id} />
          ))}
        </EntityGrid>
      )}

      <div className="flex justify-center">
        <CustomPagination
          pageSize={pageSize}
          total={totalCount}
          current={currentPage}
        />
      </div>
    </article>
  );
}
