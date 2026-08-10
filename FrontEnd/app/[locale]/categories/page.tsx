import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import CategoryCard from '@components/molecules/categoryCart';
import CustomPagination from '@components/molecules/pagination';
import EmptyState from '@components/molecules/storefront/EmptyState';
import EntityGrid from '@components/molecules/storefront/EntityGrid';
import PageHeader from '@components/molecules/storefront/PageHeader';
import { getAll } from '@lib/getAll';
import { buildPageMetadata } from '@lib/seo';
import { ICategory } from '@models/category';

export const revalidate = 60;

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'categoriesPage' });

  return buildPageMetadata({
    locale,
    path: 'categories',
    title: t('title'),
    description: t('description'),
  });
}

export default async function Page({ params, searchParams }: Props) {
  const { locale } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : {};
  const t = await getTranslations({ locale, namespace: 'categoriesPage' });

  const page = parseInt(
    (Array.isArray(resolvedSearchParams?.page)
      ? resolvedSearchParams?.page[0]
      : resolvedSearchParams?.page) ?? '1'
  );
  const pageSize = parseInt(
    (Array.isArray(resolvedSearchParams?.pageSize)
      ? resolvedSearchParams?.pageSize[0]
      : resolvedSearchParams?.pageSize) ?? '12'
  );

  const response = await getAll<ICategory>('categories', {
    page,
    pageSize,
    byConfig: false,
  });

  const categories = response?.data.records || [];

  return (
    <article className="store-page !pt-6">
      <PageHeader title={t('title')} description={t('description')} />

      {categories.length === 0 ? (
        <EmptyState title={t('empty')} description={t('emptyHint')} />
      ) : (
        <>
          <EntityGrid cols="dense">
            {categories.map((cat) => (
              <CategoryCard key={cat.id} category={cat} />
            ))}
          </EntityGrid>

          {response && response.data.totalPages > 1 && (
            <div className="flex justify-center">
              <CustomPagination
                pageSize={response.data.pageSize}
                total={response.data.totalCount}
                current={response.data.pageNumber}
              />
            </div>
          )}
        </>
      )}
    </article>
  );
}
