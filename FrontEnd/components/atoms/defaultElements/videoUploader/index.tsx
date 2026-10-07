'use client';

import React, { useEffect, useRef, useState } from 'react';

import { useTranslations } from 'next-intl';

import { toMediaUrl } from '@utils/toMediaUrl';

const MAX_BYTES = 30 * 1024 * 1024;
const ACCEPT = 'video/mp4,video/webm';

type Props = {
  /** Stored path (string) or a freshly picked File. */
  value?: string | File | null;
  placeHolder?: string;
  onChange: (file: File | undefined) => void;
};

/** Admin picker for a short hero video (mp4/webm, ≤ 30 MB) with a live preview. */
export default function VideoUploader({ value, placeHolder, onChange }: Props) {
  const t = useTranslations('uploader');
  const inputRef = useRef<HTMLInputElement>(null);
  const [previewUrl, setPreviewUrl] = useState<string>('');
  const [error, setError] = useState<string>('');

  useEffect(() => {
    if (value instanceof File) {
      const url = URL.createObjectURL(value);
      setPreviewUrl(url);
      return () => URL.revokeObjectURL(url);
    }
    setPreviewUrl(typeof value === 'string' ? toMediaUrl(value) : '');
  }, [value]);

  const handleFile = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    setError('');
    if (!file) return;
    if (!/^video\/(mp4|webm)$/.test(file.type)) {
      setError(t('videoType'));
      e.target.value = '';
      return;
    }
    if (file.size > MAX_BYTES) {
      setError(t('videoSize'));
      e.target.value = '';
      return;
    }
    onChange(file);
  };

  return (
    <div className="flex flex-col gap-2">
      <input
        ref={inputRef}
        type="file"
        accept={ACCEPT}
        onChange={handleFile}
        className="hidden"
      />
      <button
        type="button"
        onClick={() => inputRef.current?.click()}
        className="px-3 py-3 border border-[var(--admin-border)] border-dashed rounded-xl w-full text-[var(--admin-text-muted)] text-sm text-center hover:bg-[var(--admin-hover)] transition-colors"
      >
        {value instanceof File ? value.name : placeHolder || t('videoPlaceholder')}
      </button>
      {error ? <p className="text-[var(--error-color)] text-xs">{error}</p> : null}
      {previewUrl ? (
        <video
          src={previewUrl}
          controls
          muted
          playsInline
          preload="metadata"
          className="bg-black rounded-xl w-full max-h-56"
        />
      ) : null}
    </div>
  );
}
