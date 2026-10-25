import { ReactNode } from 'react';

import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';
import localFont from 'next/font/local';

import CustomLayout from '@layout/index';
import { getActiveTheme, themeToCss } from '@lib/theme';
import type { TLang } from '@slice/config/type';

interface IProps {
  children: ReactNode;
  params: Promise<{
    locale: string;
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
  const themeCss = themeToCss(await getActiveTheme());

  // Theme is applied client-side / via cookie bootstrap — avoid cookies() here
  // so storefront segments can stay SSG/ISR instead of always dynamic.
  return (
    <html lang={locale} dir={locale === 'fa' ? 'rtl' : 'ltr'} suppressHydrationWarning>
      <head>
        {themeCss ? (
          <style id="site-theme" dangerouslySetInnerHTML={{ __html: themeCss }} />
        ) : null}
      </head>
      <body
        className={`${myFont.className} min-h-screen bg-store-surface text-store-text antialiased`}
      >
        <NextIntlClientProvider locale={locale} messages={messages}>
          <CustomLayout>{children}</CustomLayout>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
