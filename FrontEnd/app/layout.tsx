import '../style/globals.css';

import { ReactNode } from 'react';

import type { Metadata, Viewport } from 'next';

import { siteBaseUrl } from '@lib/api';
import { getActiveTheme, themeToCss } from '@lib/theme';
import { SITE_NAME } from '@lib/seo';

type Props = {
  children: ReactNode;
};

export const viewport: Viewport = {
  width: 'device-width',
  initialScale: 1,
  themeColor: '#ffffff',
};

export const metadata: Metadata = {
  metadataBase: new URL(siteBaseUrl),
  title: {
    default: SITE_NAME,
    template: `%s | ${SITE_NAME}`,
  },
  description:
    'Rooshak Shop — premium authentic crystal dishes, glassware, and serving sets in Persian and English.',
  applicationName: SITE_NAME,
  icons: {
    icon: [
      { url: '/favicon.ico', sizes: '64x64' },
      { url: '/brand/icon-192.png', sizes: '192x192', type: 'image/png' },
    ],
    apple: '/brand/icon-192.png',
  },
  referrer: 'origin-when-cross-origin',
  formatDetection: {
    telephone: false,
    email: false,
    address: false,
  },
  verification: {
    google: process.env.NEXT_PUBLIC_GOOGLE_SITE_VERIFICATION || undefined,
  },
  openGraph: {
    type: 'website',
    siteName: SITE_NAME,
  },
  twitter: {
    card: 'summary_large_image',
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default async function RootLayout({ children }: Props) {
  const themeCss = themeToCss(await getActiveTheme());

  return (
    <html lang="fa" suppressHydrationWarning>
      <head>
        {themeCss ? (
          <style id="site-theme" dangerouslySetInnerHTML={{ __html: themeCss }} />
        ) : null}
      </head>
      <body className="min-h-screen bg-store-surface text-store-text antialiased">
        {children}
      </body>
    </html>
  );
}
