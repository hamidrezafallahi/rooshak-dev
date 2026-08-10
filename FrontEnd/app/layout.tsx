import '../style/globals.css';

import { ReactNode } from 'react';

import type { Metadata } from 'next';
import Script from 'next/script';

import { siteBaseUrl } from '@lib/api';
import { SITE_NAME } from '@lib/seo';

type Props = {
  children: ReactNode;
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

const THEME_BOOTSTRAP = `(function(){try{var m=document.cookie.match(/(?:^|; )theme=([^;]*)/);var t=m?decodeURIComponent(m[1]):'';if(t&&t!=='default')document.documentElement.setAttribute('data-theme',t);}catch(e){}})();`;

export default function RootLayout({ children }: Props) {
  return (
    <html lang="fa" suppressHydrationWarning>
      <head>
        <Script id="theme-bootstrap" strategy="beforeInteractive">
          {THEME_BOOTSTRAP}
        </Script>
      </head>
      <body className="bg-[radial-gradient(circle_at_top,color-mix(in_srgb,var(--primary-color)_55%,transparent)_0%,color-mix(in_srgb,var(--secondary-color)_30%,#0b1224)_55%,#060914_100%)] min-h-screen antialiased">
        {children}
      </body>
    </html>
  );
}
