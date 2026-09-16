'use client';

import { useCallback, useEffect, useState } from 'react';

import { useTranslations } from 'next-intl';

type Props = {
  url: string;
  title: string;
};

/** Only the interactive bits — the QR and copy stay server-rendered. */
export default function ShareActions({ url, title }: Props) {
  const t = useTranslations('exhibition');
  const [copied, setCopied] = useState(false);
  const [canNativeShare, setCanNativeShare] = useState(false);

  useEffect(() => {
    setCanNativeShare(typeof navigator !== 'undefined' && 'share' in navigator);
  }, []);

  useEffect(() => {
    if (!copied) return;
    const timer = setTimeout(() => setCopied(false), 2000);
    return () => clearTimeout(timer);
  }, [copied]);

  const copyLink = useCallback(async () => {
    try {
      await navigator.clipboard.writeText(url);
      setCopied(true);
    } catch {
      // Clipboard is blocked on insecure origins; the URL stays visible above.
      setCopied(false);
    }
  }, [url]);

  const nativeShare = useCallback(async () => {
    try {
      await navigator.share({ title, url });
    } catch {
      // User dismissed the sheet — nothing to recover from.
    }
  }, [title, url]);

  const shareText = `${title} — ${url}`;

  return (
    <div className="exhibit-share-actions">
      {canNativeShare && (
        <button type="button" onClick={nativeShare} className="store-btn store-btn-primary">
          {t('share')}
        </button>
      )}

      <button type="button" onClick={copyLink} className="store-btn">
        {copied ? t('copied') : t('copyLink')}
      </button>

      <a
        href={`https://wa.me/?text=${encodeURIComponent(shareText)}`}
        target="_blank"
        rel="noopener noreferrer"
        className="store-btn"
      >
        WhatsApp
      </a>

      <a
        href={`https://t.me/share/url?url=${encodeURIComponent(url)}&text=${encodeURIComponent(title)}`}
        target="_blank"
        rel="noopener noreferrer"
        className="store-btn"
      >
        Telegram
      </a>
    </div>
  );
}
