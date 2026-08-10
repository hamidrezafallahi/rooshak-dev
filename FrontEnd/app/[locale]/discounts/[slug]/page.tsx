import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import { notFound } from 'next/navigation';

import PageHeader from '@components/molecules/storefront/PageHeader';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { buildPageMetadata } from '@lib/seo';
import { fetchStaticSlugParams } from '@lib/staticParams';
import { SimpleResponse } from '@models/base';

export const revalidate = 60;

type Props = {
  params: Promise<{ locale: string; slug: string }>;
};

type DiscountItem = {
  id?: number;
  title?: string;
  name?: string;
  description?: string;
  content?: string;
  image?: string;
  banner?: string;
};

export async function generateStaticParams() {
  // Prefer getslugs; fall back to active list (id used as route key).
  return fetchStaticSlugParams('discounts/getslugs', 'discounts/active');
}

async function fetchDiscount(slug: string): Promise<DiscountItem | null> {
  const result = await safeFetchJson<SimpleResponse<DiscountItem> | DiscountItem>(
    `${serverApiBaseUrl}/discounts/${slug}`,
    { next: { revalidate: 60 } },
  );

  if (!result.ok || !result.data) return null;
  const envelope = result.data as SimpleResponse<DiscountItem>;
  if (envelope && typeof envelope === 'object' && 'data' in envelope) {
    if (envelope.isSuccess === false) return null;
    return envelope.data ?? null;
  }
  return result.data as DiscountItem;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, slug } = await params;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });
  const item = await fetchDiscount(slug);

  return buildPageMetadata({
    locale,
    path: `discounts/${slug}`,
    title: item?.title || item?.name || t('title'),
    description: item?.description || t('description'),
    images: [item?.image || item?.banner],
  });
}

export default async function Page({ params }: Props) {
  const { locale, slug } = await params;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });
  const item = await fetchDiscount(slug);

  if (!item) {
    notFound();
  }

  const title = item.title || item.name || t('title');
  const description = item.description || t('description');

  return (
    <article className="store-page !pt-6">
      <PageHeader title={title} description={description} />
      <div className="store-panel p-5 md:p-8 prose max-w-none">
        {item.content ? (
          <div dangerouslySetInnerHTML={{ __html: String(item.content) }} />
        ) : (
          <p className="text-[var(--store-text-muted)]">{description}</p>
        )}
      </div>
    </article>
  );
}
