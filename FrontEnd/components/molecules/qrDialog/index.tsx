'use client';

import {
  useEffect,
  useState,
} from 'react';

interface Props {
  open: boolean;
  title: string;
  body: string;
  /** Absolute URL encoded in the QR code. */
  url: string;
  closeLabel: string;
  onClose: () => void;
}

/** Small modal that shows a QR code so a desktop visitor can continue on their phone. */
export default function QrDialog({ open, title, body, url, closeLabel, onClose }: Props) {
  const [dataUrl, setDataUrl] = useState<string>('');

  useEffect(() => {
    if (!open || !url) return;
    let cancelled = false;
    import('qrcode')
      .then((mod) =>
        mod.default.toDataURL(url, {
          margin: 1,
          width: 240,
          errorCorrectionLevel: 'M',
        }),
      )
      .then((value) => {
        if (!cancelled) setDataUrl(value);
      })
      .catch(() => {
        if (!cancelled) setDataUrl('');
      });
    return () => {
      cancelled = true;
    };
  }, [open, url]);

  useEffect(() => {
    if (!open) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', onKey);
    return () => window.removeEventListener('keydown', onKey);
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/50 p-4"
      onClick={onClose}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-label={title}
        className="flex w-full max-w-sm flex-col items-center gap-4 bg-store-surface p-6 text-center text-store-text"
        onClick={(e) => e.stopPropagation()}
      >
        <h2 className="text-lg font-semibold">{title}</h2>
        {dataUrl && (
          // eslint-disable-next-line @next/next/no-img-element
          <img src={dataUrl} alt="QR" width={240} height={240} className="bg-white" />
        )}
        <p className="text-sm leading-6 text-store-subtle">{body}</p>
        <button
          type="button"
          onClick={onClose}
          className="border border-store-strong px-4 py-2 text-sm"
        >
          {closeLabel}
        </button>
      </div>
    </div>
  );
}
