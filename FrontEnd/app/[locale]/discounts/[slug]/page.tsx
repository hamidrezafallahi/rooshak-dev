import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import { notFound } from 'next/navigation';

import PageHeader from '@components/molecules/storefront/PageHeader';
import { serverApiBaseUrl } from '@lib/api';
import { buildPageMetadata } from '@lib/seo';

export const revalidate = 60;

type Props = {
  params: Promise<{ locale: string; slug: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale, slug } = await params;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });

  try {
    const response = await fetch(`${serverApiBaseUrl}/discounts/${slug}`, {
      next: { revalidate: 60 },
    });
    if (!response.ok) {
      return buildPageMetadata({
        locale,
        path: `discounts/${slug}`,
        title: t('title'),
        description: t('description'),
      });
    }
    const result = await response.json();
    const item = result.data || result;
    return buildPageMetadata({
      locale,
      path: `discounts/${slug}`,
      title: item?.title || item?.name || t('title'),
      description: item?.description || t('description'),
      images: [item?.image || item?.banner],
    });
  } catch {
    return buildPageMetadata({
      locale,
      path: `discounts/${slug}`,
      title: t('title'),
      description: t('description'),
    });
  }
}

export default async function Page({ params }: Props) {
  const { locale, slug } = await params;
  const t = await getTranslations({ locale, namespace: 'discountsPage' });
  const tStore = await getTranslations({ locale, namespace: 'store' });

  const response = await fetch(`${serverApiBaseUrl}/discounts/${slug}`, {
    next: { revalidate: 60 },
  });

  if (response.status === 404) {
    notFound();
  }

  if (!response.ok) {
    return (
      <article className="store-page !pt-6">
        <div className="store-empty">
          <h1 className="store-empty-title">{tStore('loadError')}</h1>
          <p className="store-empty-desc">{tStore('loadErrorHint')}</p>
        </div>
      </article>
    );
  }

  const result = await response.json();
  const item = result.data || result;
  const title = item?.title || item?.name || t('title');
  const description = item?.description || t('description');

  return (
    <article className="store-page !pt-6">
      <PageHeader title={title} description={description} />
      <div className="store-panel p-5 md:p-8 prose max-w-none">
        {item?.content ? (
          <div dangerouslySetInnerHTML={{ __html: String(item.content) }} />
        ) : (
          <p className="text-[var(--store-text-muted)]">{description}</p>
        )}
      </div>
    </article>
  );
}
