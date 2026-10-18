import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';
import { permanentRedirect } from 'next/navigation';

import SeoHighlight from '@components/molecules/storefront/SeoHighlight';
import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import FaqSection, { parseFaqJson } from '@components/molecules/storefront/FaqSection';
import CategoryTemplate from '@components/templates/categoryTemplate';
import { serverApiBaseUrl } from '@lib/api';
import { safeFetchJson } from '@lib/safeFetch';
import { buildPageMetadata } from '@lib/seo';
import { fetchStaticSlugParams } from '@lib/staticParams';
import { SimpleResponse } from '@models/base';
import type { ICategory } from '@models/category';

export const dynamicParams = true;

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};

export async function generateStaticParams() {
  return fetchStaticSlugParams('Categories/getslugs', 'Categories/getids');
}

async function fetchCategory(slug: string): Promise<ICategory | null> {
  const result = await safeFetchJson<SimpleResponse<ICategory>>(
    `${serverApiBaseUrl}/Categories/${slug}`,
    { next: { revalidate: 36 } },
  );

  if (!result.ok || !result.data?.data) return null;
  if (result.data.isSuccess === false) return null;
  return result.data.data;
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const category = await fetchCategory(slug);

  if (!category) {
    return buildPageMetadata({
      locale,
      path: `categories/${slug}`,
      title: tStore('notFound'),
      description: tStore('notFoundHint'),
      noIndex: true,
    });
  }

  const canonical = category.slug || String(category.id);
  const title =
    (locale === 'fa' ? category.seoTitleFa : category.seoTitleEn) ||
    (locale === 'fa' ? category.persianName : category.englishName);
  const description =
    (locale === 'fa' ? category.metaDescriptionFa : category.metaDescriptionEn) ||
    (locale === 'fa'
      ? category.categoryPersianDesc
      : category.categoryEnglishDesc);

  return buildPageMetadata({
    locale,
    path: `categories/${canonical}`,
    title,
    description,
    images: [category.categoryCover],
  });
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const tStore = await getTranslations({ locale, namespace: 'store' });
  const category = await fetchCategory(slug);

  if (!category) {
    return (
      <div className="store-page">
        <div className="store-empty">
          <h1 className="store-empty-title">{tStore('notFound')}</h1>
          <p className="store-empty-desc">{tStore('notFoundHint')}</p>
        </div>
      </div>
    );
  }

  if (category.slug && category.slug !== slug && /^\d+$/.test(slug)) {
    permanentRedirect(`/${locale}/categories/${category.slug}`);
  }

  const title =
    locale === 'fa' ? category.persianName : category.englishName;
  const seoTitle =
    (locale === 'fa' ? category.seoTitleFa : category.seoTitleEn) || null;
  const seoDescription =
    (locale === 'fa' ? category.metaDescriptionFa : category.metaDescriptionEn) ||
    null;

  const faqItems = parseFaqJson(category.faqJson);
  const fallbackFaq =
    faqItems.length > 0
      ? faqItems
      : [
          {
            question:
              locale === 'fa'
                ? `در دسته ${title} چه محصولاتی پیدا می‌کنم؟`
                : `What can I find in ${title}?`,
            answer:
              (locale === 'fa'
                ? category.categoryPersianDesc
                : category.categoryEnglishDesc) ||
              (locale === 'fa'
                ? 'مجموعه‌ای از ظروف کریستال منتخب روشاک در این دسته قرار دارد.'
                : 'A curated selection of Rooshak crystal products in this category.'),
          },
          {
            question:
              locale === 'fa'
                ? 'آیا محصولات این دسته اصل هستند؟'
                : 'Are products in this category authentic?',
            answer:
              locale === 'fa'
                ? 'روشاک روی کریستال اصل و برندهای معتبر تمرکز دارد و مشخصات هر محصول در صفحه همان کالا آمده است.'
                : 'Rooshak focuses on authentic crystal from trusted brands; each product page lists detailed specs.',
          },
        ];

  return (
    <div className="store-page !pt-6">
      <StoreBreadcrumbs
        locale={locale}
        items={[
          { name: locale === 'fa' ? 'خانه' : 'Home', path: '' },
          {
            name: locale === 'fa' ? 'دسته‌بندی‌ها' : 'Categories',
            path: 'categories',
          },
          { name: title },
        ]}
      />
      <SeoHighlight locale={locale} title={seoTitle} description={seoDescription} />
      <CategoryTemplate category={category} />
      <FaqSection locale={locale} items={fallbackFaq} />
    </div>
  );
}
