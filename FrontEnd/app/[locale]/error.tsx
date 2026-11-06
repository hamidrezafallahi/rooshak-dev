'use client';

import { useEffect } from 'react';

import { reportClientError } from '@components/organisms/runtimeErrorBridge';
import { messageFromDigest } from '@lib/apiError';

type Props = {
  error: Error & { digest?: string };
  reset: () => void;
};

export default function LocaleError({ error, reset }: Props) {
  // Server errors reach production builds without their message; ours carry it in the digest.
  const explanation = messageFromDigest(error.digest);
  useEffect(() => {
    reportClientError(error.message, error, {
      scope: 'app/[locale]/error',
      digest: error.digest,
    });
  }, [error]);

  return (
    <div className="flex flex-col justify-center items-center gap-4 mx-auto px-4 py-24 max-w-lg text-center">
      <h1 className="font-normal text-store-text text-2xl">
        خطایی رخ داد
      </h1>
      <p className="text-store-subtle text-sm leading-relaxed">
        {explanation ??
          (process.env.NODE_ENV === 'development'
            ? error.message
            : 'لطفاً دوباره تلاش کنید. اگر مشکل ادامه داشت با پشتیبانی تماس بگیرید.')}
      </p>
      {process.env.NODE_ENV === 'development' && error.digest && !explanation ? (
        <p className="opacity-70 font-mono text-xs">digest: {error.digest}</p>
      ) : null}
      <button type="button" className="bg-primary hover:bg-transparent px-8 py-3 border border-primary text-primary-foreground hover:text-store-text text-sm transition-colors" onClick={reset}>
        تلاش مجدد
      </button>
    </div>
  );
}
