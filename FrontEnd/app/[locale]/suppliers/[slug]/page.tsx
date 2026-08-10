import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import { notFound, permanentRedirect } from 'next/navigation';

import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import SupplierTemplate from '@components/templates/supplierTemplate';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { buildPageMetadata } from '@lib/seo';
import { fetchStaticSlugParams } from '@lib/staticParams';
import { SimpleResponse } from '@models/base';
import { IUser } from '@models/user';

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};

export async function generateStaticParams() {
  return fetchStaticSlugParams(
    'Users/getslugs',
    'productOffers/suppliersIds',
  );
}

async function fetchSupplier(slug: string): Promise<IUser | null> {
  const result = await safeFetchJson<SimpleResponse<IUser>>(
    `${serverApiBaseUrl}/Users/${slug}`,
    { next: { revalidate: 36 } },
  );

  if (!result.ok || !result.data?.data) return null;
  if (result.data.isSuccess === false) return null;
  return result.data.data;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const supplier = await fetchSupplier(slug);

  if (!supplier) {
    return buildPageMetadata({
      locale,
      path: `suppliers/${slug}`,
      title: tStore('notFound'),
      description: tStore('notFoundHint'),
      noIndex: true,
    });
  }

  const canonical = supplier.slug || String(supplier.id);
  return buildPageMetadata({
    locale,
    path: `suppliers/${canonical}`,
    title: supplier.fullName,
    description: supplier.userDescription,
    images: [supplier.userImage],
  });
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const data = await fetchSupplier(slug);

  if (!data) {
    notFound();
  }

  if (data.slug && data.slug !== slug && /^\d+$/.test(slug)) {
    permanentRedirect(`/${locale}/suppliers/${data.slug}`);
  }

  return (
    <div className="store-page !pt-6">
      <StoreBreadcrumbs
        locale={locale}
        items={[
          { name: locale === 'fa' ? 'خانه' : 'Home', path: '' },
          {
            name: locale === 'fa' ? 'تأمین‌کنندگان' : 'Suppliers',
            path: 'suppliers',
          },
          { name: data.fullName || slug },
        ]}
      />
      <SupplierTemplate supplier={data} />
    </div>
  );
}
