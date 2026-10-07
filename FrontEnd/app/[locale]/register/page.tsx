import type { Metadata } from 'next';
import { getTranslations } from 'next-intl/server';

import Register from '@components/templates/register';
import { getSlides, splitHeroSlide } from '@lib/landing';
import { toMediaUrl } from '@utils/toMediaUrl';
import { buildPageMetadata } from '@lib/seo';

type Props = {
  params: Promise<{ locale: string }>;
};

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'registerPage' });

  return buildPageMetadata({
    locale,
    path: 'register',
    title: t('metaTitle'),
    description: t('metaDescription'),
    noIndex: true,
  });
}

export default async function RegisterPage({ params }: Props) {
  const { locale } = await params;
  const t = await getTranslations({ locale, namespace: 'homePage' });
  const { hero } = splitHeroSlide(await getSlides());

  return (
    <Register
      imageSrc={hero?.bannerUrl ? toMediaUrl(hero.bannerUrl) : '/images/landingPage/11.jpg'}
      videoSrc={hero?.videoUrl ? toMediaUrl(hero.videoUrl) : undefined}
      mediaAlt={t('heroImageAlt')}
      playLabel={t('videoPlay')}
      pauseLabel={t('videoPause')}
    />
  );
}
