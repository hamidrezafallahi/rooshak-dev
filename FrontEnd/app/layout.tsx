import '../style/globals.css';

import { ReactNode } from 'react';

import type { Metadata, Viewport } from 'next';

import { siteBaseUrl } from '@lib/api';
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
};

// <html>/<body> live in app/[locale]/layout.tsx so lang/dir follow the URL locale.
// The global 404 (app/not-found.tsx) renders its own document.
export default function RootLayout({ children }: Props) {
  return children;
}
