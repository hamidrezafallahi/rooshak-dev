import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import CustomPagination from '@components/molecules/pagination';
import EmptyState from '@components/molecules/storefront/EmptyState';
import PageHeader from '@components/molecules/storefront/PageHeader';
import { getAll } from '@lib/getAll';
import { buildPageMetadata } from '@lib/seo';
import { IBlog } from '@models/Blog';

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export const revalidate = 60;

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });

  return buildPageMetadata({
    locale,
    path: 'discounts',
    title: t('title'),
    description: t('description'),
  });
}

export default async function Page({ params, searchParams }: Props) {
  const { locale } = await params;
  const queryString = searchParams ? await searchParams : undefined;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });
  const page = parseInt((queryString?.page as string) ?? '1');
  const pageRecordCount = 12;

  const response = await getAll<IBlog>('discounts', {
    page,
    pageSize: pageRecordCount,
    byConfig: false,
  });

  const items = response?.data.records ?? [];

  return (
    <article className="store-page !pt-6">
      <PageHeader title={t('title')} description={t('description')} />

      {items.length === 0 ? (
        <EmptyState
          title={t('empty')}
          description={t('comingSoon')}
        />
      ) : (
        <section className="store-panel p-5 md:p-6">
          <ul className="gap-3 grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3">
            {items.map((item, index) => (
              <li
                key={(item as { id?: string | number }).id ?? index}
                className="rounded-xl border border-[var(--store-border)] bg-[var(--store-surface-muted)] p-4"
              >
                <h2 className="font-semibold text-[var(--store-text)]">
                  {(item as { title?: string; name?: string }).title ||
                    (item as { name?: string }).name ||
                    t('title')}
                </h2>
              </li>
            ))}
          </ul>
        </section>
      )}

      <div className="flex justify-center">
        <CustomPagination
          pageSize={pageRecordCount}
          total={response?.data.totalCount || 0}
          current={page}
        />
      </div>
    </article>
  );
}
