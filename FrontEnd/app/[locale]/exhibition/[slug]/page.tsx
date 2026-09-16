import type { Metadata } from 'next';
import { notFound, permanentRedirect } from 'next/navigation';

import { getTranslations } from 'next-intl/server';

import ExhibitionPhoto from '@components/organisms/exhibition/ExhibitionPhoto';
import {
  exhibitionCatalogName,
  exhibitionStaticSlugs,
  findExhibitionCatalog,
} from '@lib/exhibitionCatalogs';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ slug: string; locale: string }>;
};

/** Fully static sheets — data lives in the repo, not the API. */
export const dynamic = 'force-static';
export const revalidate = false;

export function generateStaticParams() {
  return exhibitionStaticSlugs().map((slug) => ({ slug }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { slug, locale } = await params;
  const catalog = findExhibitionCatalog(slug);
  const t = await getTranslations({ locale, namespace: 'exhibition' });

  if (!catalog) {
    return buildPageMetadata({
      locale,
      path: `exhibition/${slug}`,
      title: t('indexTitle'),
      description: t('indexDescription'),
      noIndex: true,
    });
  }

  const name = exhibitionCatalogName(catalog, locale);

  return buildPageMetadata({
    locale,
    path: `exhibition/${catalog.slug}`,
    title: t('metaTitle', { family: name }),
    description: t('metaDescription', {
      family: name,
      count: Math.max(catalog.photos.length - 1, 0),
    }),
    images: catalog.photos.slice(0, 3).map((p) => p.src),
  });
}

export default async function Page({ params }: Props) {
  const { slug, locale } = await params;
  const catalog = findExhibitionCatalog(slug);

  if (!catalog) {
    notFound();
  }

  if (slug !== catalog.slug) {
    permanentRedirect(`/${locale}/exhibition/${catalog.slug}`);
  }

  const name = exhibitionCatalogName(catalog, locale);

  return (
    <main className="exhibit-sheet" aria-label={name}>
      <h1 className="sr-only">{name}</h1>
      {catalog.photos.map((photo, index) => (
        <ExhibitionPhoto
          key={photo.src}
          src={photo.src}
          alt={photo.alt}
          width={photo.width}
          height={photo.height}
          priority={index === 0}
        />
      ))}
    </main>
  );
}
