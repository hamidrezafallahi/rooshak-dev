import type { NextPageContext } from 'next';
import Head from 'next/head';

import {
  describeStatus,
  messageFromDigest,
} from '@lib/apiError';

type Props = {
  title: string;
  retry: string;
  message: string;
  isEn: boolean;
};

const COPY = {
  fa: { title: 'خطا در بارگذاری', retry: 'تلاش مجدد' },
  en: { title: 'Loading error', retry: 'Try again' },
} as const;

/**
 * Next.js skips app/[locale]/error.tsx when a page with generateStaticParams or a revalidate
 * time fails while rendering on the server, and answers with this page instead. Without it
 * visitors get the built-in "500 Internal Server Error" screen. Pages that already have a
 * cached version keep showing it, so this only appears when there is nothing to fall back on.
 *
 * Having a pages/ directory makes next/navigation hooks nullable in the types, which is why
 * components write useParams()!: under the App Router they always return a value.
 */
export default function ErrorPage({ title, retry, message, isEn }: Props) {
  return (
    <main
      dir={isEn ? 'ltr' : 'rtl'}
      lang={isEn ? 'en' : 'fa'}
      style={{
        display: 'grid',
        placeItems: 'center',
        minHeight: '100vh',
        padding: 24,
        boxSizing: 'border-box',
        fontFamily: 'Tahoma, sans-serif',
        color: '#111111',
      }}
    >
      <Head>
        <title>{title}</title>
      </Head>
      <div style={{ maxWidth: 480, textAlign: 'center' }}>
        <h1 style={{ fontSize: 22, marginBottom: 12 }}>{title}</h1>
        <p role="alert" style={{ fontSize: 14, lineHeight: 1.9, marginBottom: 20 }}>
          {message}
        </p>
        {/* An empty href reloads the address this page was reached at. */}
        <a href="" style={{ color: 'inherit', fontSize: 14 }}>
          {retry}
        </a>
      </div>
    </main>
  );
}

ErrorPage.getInitialProps = ({ err, res, asPath }: NextPageContext): Props => {
  const isEn = /^\/en(\/|$)/.test(asPath ?? '');
  const digest = (err as (Error & { digest?: string }) | null | undefined)?.digest;

  return {
    ...(isEn ? COPY.en : COPY.fa),
    message: messageFromDigest(digest) ?? describeStatus(res?.statusCode ?? 500),
    isEn,
  };
};
