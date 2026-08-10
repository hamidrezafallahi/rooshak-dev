import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import CustomPagination from '@components/molecules/pagination';
import EmptyState from '@components/molecules/storefront/EmptyState';
import PageHeader from '@components/molecules/storefront/PageHeader';
import TagCard from '@components/molecules/tagCard';
import { getAll } from '@lib/getAll';
import { buildPageMetadata } from '@lib/seo';
import { IProductTag } from '@models/tag';

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export const revalidate = 60;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'tagsPage' });

  return buildPageMetadata({
    locale,
    path: 'tags',
    title: t('title'),
    description: t('description'),
  });
}

export default async function Page({ params, searchParams }: Props) {
  const { locale } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const t = await getTranslations({ locale, namespace: 'tagsPage' });
  const pageNumber = parseInt((resolvedSearchParams?.page as string) ?? '1');
  const pageRecordCount = 24;

  const response = await getAll<IProductTag>('ProductTag', {
    page: pageNumber,
    pageSize: pageRecordCount,
    byConfig: false,
  });

  const tags: IProductTag[] = response?.data?.records ?? [];

  return (
    <article className="store-page !pt-6">
      <PageHeader
        title={t('title')}
        description={t('description')}
        align="center"
      />

      {tags.length === 0 ? (
        <EmptyState title={t('empty')} description={t('emptyHint')} />
      ) : (
        <div className="store-panel flex flex-wrap gap-3 p-5 md:p-6">
          {tags.map((tag) => (
            <TagCard
              key={tag.id}
              tag={{ id: tag.id, name: tag.tagName }}
            />
          ))}
        </div>
      )}

      <div className="flex justify-center">
        <CustomPagination
          pageSize={pageRecordCount}
          total={response?.data.totalCount || 0}
          current={pageNumber}
        />
      </div>
    </article>
  );
}
