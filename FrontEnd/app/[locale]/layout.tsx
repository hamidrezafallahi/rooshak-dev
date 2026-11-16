import { ReactNode } from 'react';

import { NextIntlClientProvider } from 'next-intl';
import { getMessages } from 'next-intl/server';

import CustomLayout from '@layout/index';
import { getCustomFonts } from '@lib/customFonts';
import { fontVariables } from '@lib/fonts';
import { getActiveTheme, themeToCss } from '@lib/theme';
import type { TLang } from '@slice/config/type';

interface IProps {
  children: ReactNode;
  params: Promise<{
    locale: string;
  }>;
}

export async function generateStaticParams() {
  return [{ locale: 'fa' }, { locale: 'en' }];
}

export default async function BaseLayout({ children, params }: IProps) {
  const { locale } = await params;
  const messages = await getMessages({ locale });
  const [theme, customFonts] = await Promise.all([getActiveTheme(), getCustomFonts()]);
  const themeCss = themeToCss(theme, customFonts);

  // Theme is applied client-side / via cookie bootstrap — avoid cookies() here
  // so storefront segments can stay SSG/ISR instead of always dynamic.
  return (
    <html
      lang={locale}
      dir={locale === 'fa' ? 'rtl' : 'ltr'}
      className={fontVariables}
      suppressHydrationWarning
    >
      <head>
        {themeCss ? (
          <style id="site-theme" dangerouslySetInnerHTML={{ __html: themeCss }} />
        ) : null}
      </head>
      <body className="min-h-screen bg-store-surface text-store-text antialiased">
        <NextIntlClientProvider locale={locale} messages={messages}>
          <CustomLayout>{children}</CustomLayout>
        </NextIntlClientProvider>
      </body>
    </html>
  );
}
