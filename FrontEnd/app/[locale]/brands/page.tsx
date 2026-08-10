import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import CustomPagination from '@components/molecules/pagination';
import EmptyState from '@components/molecules/storefront/EmptyState';
import EntityGrid from '@components/molecules/storefront/EntityGrid';
import PageHeader from '@components/molecules/storefront/PageHeader';
import { getAll } from '@lib/getAll';
import { buildPageMetadata } from '@lib/seo';
import { IBrand } from '@models/brand';

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export const revalidate = 60;

export async function generateMetadata({ params, searchParams }: Props): Promise<Metadata> {
  const { locale } = await params;
  const resolvedSearch = searchParams ? await searchParams : {};
  const t = await getTranslations({ locale, namespace: 'brandsPage' });

  return buildPageMetadata({
    locale,
    path: 'brands',
    title: t('title'),
    description: t('description'),
    listingSearchParams: resolvedSearch,
  });
}

export default async function Page({ searchParams, params }: Props) {
  const { locale } = await params;
  const resolvedSearchParams = searchParams ? await searchParams : undefined;
  const t = await getTranslations({ locale, namespace: 'brandsPage' });
  const tStore = await getTranslations({ locale, namespace: 'store' });

  const page = parseInt((resolvedSearchParams?.page as string) ?? '1');
  const pageRecordCount = 12;

  const response = await getAll<IBrand>('brands', {
    page,
    pageSize: pageRecordCount,
    byConfig: false,
  });

  const brands: IBrand[] = response?.data?.records ?? [];

  return (
    <article className="store-page !pt-6">
      <PageHeader
        title={t('title')}
        description={t('description')}
        align="center"
      />

      {brands.length === 0 ? (
        <EmptyState title={t('empty')} description={t('emptyHint')} />
      ) : (
        <EntityGrid cols="dense">
          {brands.map((brand, index) => (
            <Link
              href={`/${locale}/brands/${brand.slug || brand.id}`}
              key={brand.id}
              className="store-card group"
            >
              <div className="relative w-full h-40 md:h-44 overflow-hidden">
                <MediaImage
                  src={brand.logoFile}
                  alt={brand.name}
                  fill
                  className="object-cover group-hover:scale-105 transition-transform duration-500"
                  loading={index < 5 ? 'eager' : 'lazy'}
                  priority={index < 5}
                  sizes="(max-width: 768px) 50vw, 20vw"
                />
              </div>
              <div className="store-card-body">
                <h2 className="font-semibold text-base">{brand.name}</h2>
                {brand.description ? (
                  <p className="line-clamp-2 text-[var(--store-text-muted)] text-xs">
                    {brand.description}
                  </p>
                ) : null}
                <span className="mt-1 font-medium text-[var(--primary-color)] text-sm">
                  {tStore('viewBrand')}
                </span>
              </div>
            </Link>
          ))}
        </EntityGrid>
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
