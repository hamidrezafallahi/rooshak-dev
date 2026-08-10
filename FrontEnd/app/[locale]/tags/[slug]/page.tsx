import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import { notFound, permanentRedirect } from 'next/navigation';

import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import TagTemplate from '@components/templates/tagTemplate';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { buildPageMetadata } from '@lib/seo';
import { fetchStaticSlugParams } from '@lib/staticParams';
import { SimpleResponse } from '@models/base';
import { ITag } from '@models/tag';

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};

export async function generateStaticParams() {
  return fetchStaticSlugParams('Tags/getslugs', 'Tags/getids');
}

async function fetchTag(slug: string): Promise<ITag | null> {
  const result = await safeFetchJson<SimpleResponse<ITag>>(
    `${serverApiBaseUrl}/Tags/${slug}`,
    { next: { revalidate: 36 } },
  );

  if (!result.ok || !result.data?.data) return null;
  if (result.data.isSuccess === false) return null;
  return result.data.data;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const tag = await fetchTag(slug);

  if (!tag) {
    return buildPageMetadata({
      locale,
      path: `tags/${slug}`,
      title: tStore('notFound'),
      description: tStore('notFoundHint'),
      noIndex: true,
    });
  }

  const canonical = tag.slug || String(tag.id);
  return buildPageMetadata({
    locale,
    path: `tags/${canonical}`,
    title: tag.name,
    description: tag.name,
  });
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const tag = await fetchTag(slug);

  if (!tag) {
    notFound();
  }

  if (tag.slug && tag.slug !== slug && /^\d+$/.test(slug)) {
    permanentRedirect(`/${locale}/tags/${tag.slug}`);
  }

  return (
    <div className="store-page !pt-6">
      <StoreBreadcrumbs
        locale={locale}
        items={[
          { name: locale === 'fa' ? 'خانه' : 'Home', path: '' },
          { name: locale === 'fa' ? 'برچسب‌ها' : 'Tags', path: 'tags' },
          { name: tag.name },
        ]}
      />
      <TagTemplate Tag={tag} />
    </div>
  );
}
