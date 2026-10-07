'use client';

import { useEffect } from 'react';

import { reportClientError } from '@components/organisms/runtimeErrorBridge';

type ErrorProps = {
  error: Error;
  reset: () => void;
};

/** Legacy layout error UI — prefers app/[locale]/error.tsx in App Router. */
export default function Error({ error, reset }: ErrorProps) {
  useEffect(() => {
    reportClientError(error.message, error, { scope: 'layout/error' });
  }, [error]);

  return (
    <div className="flex flex-col items-center gap-3 p-8 text-center">
      <h2 className="font-semibold text-lg">خطایی رخ داد</h2>
      {process.env.NODE_ENV === 'development' ? (
        <p className="max-w-md text-sm break-words">{error.message}</p>
      ) : null}
      <button type="button" className="bg-primary hover:bg-transparent px-8 py-3 border border-primary text-primary-foreground hover:text-store-text text-sm transition-colors" onClick={reset}>
        تلاش مجدد
      </button>
    </div>
  );
}
