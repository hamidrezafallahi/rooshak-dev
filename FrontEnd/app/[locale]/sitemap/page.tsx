import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

import PageHeader from '@components/molecules/storefront/PageHeader';
import Footer from '@layout/footer';
import Header from '@layout/header';
import { serverApiBaseUrl } from '@lib/api';
import { buildPageMetadata, LOCALES } from '@lib/seo';

export const revalidate = 3600;

type Props = {
  params: Promise<{ locale: string }>;
};

type SitemapSection = {
  title: string;
  links: Array<{ href: string; label: string }>;
};

async function fetchKeys(endpoint: string): Promise<string[]> {
  try {
    const res = await fetch(`${serverApiBaseUrl}/${endpoint}`, {
      next: { revalidate },
    });
    if (!res.ok) return [];

    const json = await res.json();
    const payload = json?.data ?? json;
    const list = Array.isArray(payload)
      ? payload
      : Array.isArray(payload?.records)
        ? payload.records
        : [];

    return list
      .map((item: { slug?: string | number; id?: string | number; name?: string }) =>
        String(item?.slug ?? item?.id ?? item?.name ?? '')
      )
      .filter(Boolean);
  } catch {
    return [];
  }
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'htmlSitemap' });

  return buildPageMetadata({
    locale,
    path: 'sitemap',
    title: t('title'),
    description: t('description'),
  });
}

export default async function HtmlSitemapPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'htmlSitemap' });
  const tHeader = await getTranslations({ locale, namespace: 'header' });

  const [products, blogs, brands, categories, suppliers, tags] =
    await Promise.all([
      fetchKeys('products/getslugs'),
      fetchKeys('blogs/getslugs'),
      fetchKeys('brands/getids'),
      fetchKeys('categories/getids'),
      fetchKeys('productOffers/suppliersIds'),
      fetchKeys('tags/getids'),
    ]);

  const staticLinks = [
    { href: '', label: tHeader('home') },
    { href: 'products', label: tHeader('products') },
    { href: 'categories', label: tHeader('categories') },
    { href: 'brands', label: tHeader('brands') },
    { href: 'suppliers', label: tHeader('suppliers') },
    { href: 'tags', label: tHeader('tags') },
    { href: 'discounts', label: tHeader('discounts') },
    { href: 'blog', label: tHeader('blogs') },
  ];

  const sections: SitemapSection[] = [
    {
      title: t('mainPages'),
      links: staticLinks.map((item) => ({
        href: `/${locale}${item.href ? `/${item.href}` : ''}`,
        label: item.label,
      })),
    },
    {
      title: t('products'),
      links: products.slice(0, 200).map((slug) => ({
        href: `/${locale}/products/${slug}`,
        label: slug,
      })),
    },
    {
      title: t('blogs'),
      links: blogs.slice(0, 100).map((slug) => ({
        href: `/${locale}/blog/${slug}`,
        label: slug,
      })),
    },
    {
      title: t('brands'),
      links: brands.map((id) => ({
        href: `/${locale}/brands/${id}`,
        label: `${t('brandPrefix')} ${id}`,
      })),
    },
    {
      title: t('categories'),
      links: categories.map((id) => ({
        href: `/${locale}/categories/${id}`,
        label: `${t('categoryPrefix')} ${id}`,
      })),
    },
    {
      title: t('suppliers'),
      links: suppliers.map((id) => ({
        href: `/${locale}/suppliers/${id}`,
        label: `${t('supplierPrefix')} ${id}`,
      })),
    },
    {
      title: t('tags'),
      links: tags.map((id) => ({
        href: `/${locale}/tags/${id}`,
        label: `${t('tagPrefix')} ${id}`,
      })),
    },
  ];

  return (
    <>
      <Header />
      <main className="store-page pt-[calc(var(--store-header-h)+2rem)] min-h-[70vh]">
        <PageHeader title={t('title')} description={t('description')} />

        <section className="store-panel mb-8 p-5 md:p-6">
          <h2 className="mb-3 font-semibold text-[var(--store-text)] text-lg">
            {t('languages')}
          </h2>
          <ul className="flex flex-wrap gap-3">
            {LOCALES.map((lang) => (
              <li key={lang}>
                <Link
                  href={`/${lang}/sitemap`}
                  className="underline text-[var(--primary-color)]"
                >
                  {lang === 'fa' ? t('persian') : t('english')}
                </Link>
              </li>
            ))}
          </ul>
          <p className="mt-4 text-[var(--store-text-muted)] text-sm">
            {t('xmlHint')}{' '}
            <Link href="/sitemap.xml" className="underline text-[var(--primary-color)]">
              /sitemap.xml
            </Link>
          </p>
        </section>

        <div className="gap-6 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3">
          {sections.map((section) => (
            <section key={section.title} className="store-panel p-5 md:p-6">
              <h2 className="mb-4 font-semibold text-[var(--store-text)] text-lg">
                {section.title}
              </h2>
              {section.links.length === 0 ? (
                <p className="text-[var(--store-text-muted)] text-sm">
                  {t('empty')}
                </p>
              ) : (
                <ul className="flex flex-col gap-2 text-sm">
                  {section.links.map((link) => (
                    <li key={link.href}>
                      <Link
                        href={link.href}
                        className="opacity-90 hover:opacity-100 hover:underline transition"
                      >
                        {link.label}
                      </Link>
                    </li>
                  ))}
                </ul>
              )}
            </section>
          ))}
        </div>
      </main>
      <Footer />
    </>
  );
}
