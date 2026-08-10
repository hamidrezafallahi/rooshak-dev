import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import {
  BlogContent,
  HeroSection,
  RelatedArticles,
} from '@components/templates/blogTemplate';
import JsonLd from '@components/molecules/storefront/JsonLd';
import StoreBreadcrumbs from '@components/molecules/storefront/StoreBreadcrumbs';
import { getBlogBySlug } from '@lib/blog';
import { extractFaqFromHtml } from '@lib/blogFaq';
import { serverApiBaseUrl } from '@lib/api';
import { absoluteUrl, buildPageMetadata } from '@lib/seo';
import { SimpleResponse } from '@models/base';
import { IBlog } from '@models/Blog';

export const revalidate = 60;

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};





// ===== 1. تولید مسیرهای استاتیک =====
export async function generateStaticParams() {
  try {
    const res = await fetch(`${serverApiBaseUrl}/Blogs/getslugs`, {
      next: { revalidate: 60 },
    });

    if (!res.ok) return [];
    const response: SimpleResponse<{ slug: string }[]> = await res.json();
    if (!response.isSuccess) return [];
    return (response.data || [])
      .filter((item) => Boolean(item.slug))
      .map((item) => ({ slug: item.slug }));
  } catch (error) {
    console.error('Error generating static params:', error);
    return [];
  }
}
export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const tStore = await getTranslations({ locale, namespace: 'store' });

  try {
    const res = await fetch(`${serverApiBaseUrl}/Blogs/${slug}`, {
      next: { revalidate: 60 },
    });
    if (!res.ok) {
      return buildPageMetadata({
        locale,
        path: `blog/${slug}`,
        title: tStore('notFound'),
        description: tStore('notFoundHint'),
        noIndex: true,
      });
    }

    const response: SimpleResponse<IBlog> = await res.json();

    if (!response.isSuccess || !response.data) {
      return buildPageMetadata({
        locale,
        path: `blog/${slug}`,
        title: tStore('notFound'),
        description: tStore('notFoundHint'),
        noIndex: true,
      });
    }

    const blog = response.data;
    const title =
      locale === 'fa'
        ? blog.titleFa || blog.titleEn
        : blog.titleEn || blog.titleFa;
    const description = (
      (locale === 'fa' ? blog.metaDescriptionFa : blog.metaDescriptionEn) ||
      (locale === 'fa' ? blog.excerptFa : blog.excerptEn) ||
      ''
    ).slice(0, 160);

    return buildPageMetadata({
      locale,
      path: `blog/${slug}`,
      title,
      description,
      images: [blog.thumbnailFile],
      type: 'article',
      keywords: (
        locale === 'fa' ? blog.metaKeywordsFa : blog.metaKeywordsEn
      )
        ?.split(',')
        .map((k) => k.trim())
        .filter(Boolean),
    });
  } catch {
    return buildPageMetadata({
      locale,
      path: `blog/${slug}`,
      title: tStore('loadError'),
      description: '',
      noIndex: true,
    });
  }
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const blog = await getBlogBySlug({ params: { slug } });

  const title =
    locale === 'fa'
      ? blog.titleFa || blog.titleEn
      : blog.titleEn || blog.titleFa;

  const contentHtml =
    locale === 'fa'
      ? blog.contentFa || blog.contentEn
      : blog.contentEn || blog.contentFa;

  const faqItems = extractFaqFromHtml(contentHtml);

  const articleLd = {
    '@context': 'https://schema.org',
    '@type': 'Article',
    headline: title,
    image: blog.thumbnailFile ? [blog.thumbnailFile] : undefined,
    datePublished: blog.createdAt,
    dateModified: blog.updatedAt || blog.createdAt,
    inLanguage: locale,
    url: absoluteUrl(locale, `blog/${slug}`),
    author: blog.authorName
      ? { '@type': 'Person', name: blog.authorName }
      : undefined,
  };

  return (
    <article className="store-page !pt-6">
      <JsonLd data={articleLd} />
      <StoreBreadcrumbs
        locale={locale}
        items={[
          { name: locale === 'fa' ? 'خانه' : 'Home', path: '' },
          { name: locale === 'fa' ? 'بلاگ' : 'Blog', path: 'blog' },
          { name: title },
        ]}
      />
      <HeroSection blog={blog} locale={locale} />
      <BlogContent blog={blog} locale={locale} faqItems={faqItems} />
      <RelatedArticles />
    </article>
  );
}
