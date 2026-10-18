import type { Metadata } from 'next';
import { notFound, permanentRedirect } from 'next/navigation';

import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import TagTemplate from '@components/templates/tagTemplate';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson, throwIfUnavailable } from '@lib/safeFetch';
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

  throwIfUnavailable(result, `Tags/${slug}`);
  if (!result.ok || !result.data?.data) return null;
  if (result.data.isSuccess === false) return null;
  return result.data.data;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const tag = await fetchTag(slug);

  // Also here, not only in the page: metadata resolves before the response starts
  // streaming (htmlLimitedBots), so this is what turns the status into a real 404.
  if (!tag) notFound();

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
