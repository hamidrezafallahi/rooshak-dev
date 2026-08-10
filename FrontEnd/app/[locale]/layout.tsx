import { ReactNode } from 'react';

import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';
import localFont from 'next/font/local';

import CustomLayout from '@layout/index';
import type { TLang } from '@slice/config/type';

interface IProps {
  children: ReactNode;
  params: Promise<{
    locale: TLang;
  }>;
}

const myFont = localFont({
  src: [
    {
      path: '../../public/fonts/IRANSansWeb(FaNum).woff',
      weight: '400',
      style: 'normal',
    },
  ],
  variable: '--IRANSans-font',
  display: 'swap',
});

export async function generateStaticParams() {
  return [{ locale: 'fa' }, { locale: 'en' }];
}

export default async function BaseLayout({ children, params }: IProps) {
  const { locale } = await params;
  const messages = await getMessages({ locale });

  // Theme is applied client-side / via cookie bootstrap — avoid cookies() here
  // so storefront segments can stay SSG/ISR instead of always dynamic.
  return (
    <div
      className={myFont.className}
      dir={locale === 'fa' ? 'rtl' : 'ltr'}
      lang={locale}
    >
      <NextIntlClientProvider locale={locale} messages={messages}>
        <CustomLayout>{children}</CustomLayout>
      </NextIntlClientProvider>
    </div>
  );
}
