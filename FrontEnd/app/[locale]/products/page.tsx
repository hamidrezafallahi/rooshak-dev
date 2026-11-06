import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

import CustomPagination from '@components/molecules/pagination';
import { SimpleProductCard } from '@components/molecules/productCard';
import EmptyState from '@components/molecules/storefront/EmptyState';
import EntityGrid from '@components/molecules/storefront/EntityGrid';
import JsonLd from '@components/molecules/storefront/JsonLd';
import PageHeader from '@components/molecules/storefront/PageHeader';
import { serverApiBaseUrl } from '@lib/api';
import { absoluteUrl, buildPageMetadata } from '@lib/seo';
import { PagedResponse } from '@models/base';
import { IProduct } from '@models/product';
import {
  ApiRequestError,
  describeStatus,
  describeThrown,
  readApiResult,
} from '@lib/apiError';

type Props = {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
};

export async function generateMetadata({
  params,
  searchParams,
}: Props): Promise<Metadata> {
  const { locale } = await params;
  const resolvedSearch = searchParams ? await searchParams : {};
  const page = parseInt(
    (Array.isArray(resolvedSearch.page)
      ? resolvedSearch.page[0]
      : resolvedSearch.page) ?? '1'
  );
  const t = await getTranslations({ locale, namespace: 'productsPage' });
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const pageSuffix =
    page > 1 ? tStore('pageSuffix', { page: String(page) }) : '';

  return buildPageMetadata({
    locale,
    path: 'products',
    title: `${t('title')}${pageSuffix}`,
    description: t('description'),
    listingSearchParams: resolvedSearch,
  });
}

export default async function Page({ params, searchParams }: Props) {
  const { locale } = await params;
  const resolvedSearch = searchParams ? await searchParams : {};
  const pageNumber = parseInt(
    (Array.isArray(resolvedSearch.page)
      ? resolvedSearch.page[0]
      : resolvedSearch.page) ?? '1'
  );
  const pageSize = 12;

  const t = await getTranslations({ locale, namespace: 'productsPage' });
  const tStore = await getTranslations({ locale, namespace: 'store' });

  let productsResponse: PagedResponse<IProduct>;
  let products: IProduct[] = [];
  let totalCount = 0;
  let currentPage = pageNumber;
  let resolvedPageSize = pageSize;
  let totalPages = 1;

  try {
    const response = await fetch(
      `${serverApiBaseUrl}/Products?page=${pageNumber}&PageSize=${pageSize}`,
      { cache: 'no-store' }
    );

    if (!response.ok) {
      const { error } = await readApiResult(response);
      throw new ApiRequestError(error ?? describeStatus(response.status));
    }

    productsResponse = await response.json();

    if (productsResponse.isSuccess && productsResponse.data) {
      products = productsResponse.data.records;
      totalCount = productsResponse.data.totalCount || products.length;
      currentPage = productsResponse.data.pageNumber || pageNumber;
      resolvedPageSize = productsResponse.data.pageSize || pageSize;
      totalPages =
        productsResponse.data.totalPages ||
        Math.ceil(totalCount / resolvedPageSize);
    } else {
      products = [];
    }
  } catch (error) {
    productsResponse = {
      data: {
        records: [],
        actionsJson: '',
        columnsJson: '',
        pageNumber: 0,
        pageSize: 0,
        totalCount: 0,
        totalPages: 0,
      },
      isSuccess: false,
      error: describeThrown(error),
    };
    products = [];
  }

  const collectionLd = {
    '@context': 'https://schema.org',
    '@type': 'CollectionPage',
    name: t('title'),
    description: t('description'),
    url: absoluteUrl(locale, 'products'),
    inLanguage: locale,
  };

  return (
    <article className="store-page !pt-6">
      <JsonLd data={collectionLd} />
      <PageHeader title={t('title')} description={t('description')} />

      {!productsResponse.isSuccess && (
        <div
          className="store-panel px-4 py-3 border-[color-mix(in_srgb,var(--error-color)_35%,transparent)] text-[var(--error-color)]"
          role="alert"
        >
          <p className="font-medium">{tStore('error')}</p>
          <p>{productsResponse.error || tStore('fetchError')}</p>
        </div>
      )}

      {productsResponse.isSuccess && (
        <div className="store-meta-row">
          <p>
            {tStore('showingCount', { count: products.length })}
            {totalCount > 0 ? (
              <span className="ms-1">
                {tStore('ofTotal', { total: totalCount })}
              </span>
            ) : null}
          </p>
        </div>
      )}

      {products.length === 0 ? (
        <EmptyState
          title={
            productsResponse.isSuccess ? t('empty') : t('loadError')
          }
          description={
            productsResponse.isSuccess
              ? t('emptyHint')
              : tStore('serverError')
          }
          action={
            <Link href={`/${locale}`} className="store-btn store-btn-primary">
              {tStore('backHome')}
            </Link>
          }
        />
      ) : (
        <>
          <EntityGrid cols="products">
            {products.map((product) => (
              <SimpleProductCard product={product} key={product.id} />
            ))}
          </EntityGrid>

          {totalPages > 1 && (
            <div className="flex justify-center pt-4">
              <CustomPagination
                pageSize={resolvedPageSize}
                total={totalCount}
                current={currentPage}
              />
            </div>
          )}
        </>
      )}
    </article>
  );
}
