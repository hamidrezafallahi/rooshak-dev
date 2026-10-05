import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import PageHeader from '@components/molecules/storefront/PageHeader';
import ContactRequestForm from '@components/organisms/contactRequestForm';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'cooperationPage' });

  return buildPageMetadata({
    locale,
    path: 'cooperation',
    title: t('title'),
    description: t('metaDescription'),
  });
}

export default async function Page({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'cooperationPage' });

  return (
    <article className="store-page !pt-6">
      <PageHeader
        title={t('title')}
        description={t('description')}
        align="center"
      />

      <div className="mx-auto w-full max-w-2xl">
        <ContactRequestForm />
      </div>
    </article>
  );
}
