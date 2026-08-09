"use client";
import React, {
  ChangeEvent,
  DragEvent,
  useEffect,
  useRef,
  useState,
} from 'react';

import { useTranslations } from 'next-intl';

import { cn } from '@/lib/utils';
import { toMediaUrl } from '@utils/toMediaUrl';

interface UploaderProps {
  value?: string | File | null;
  defaultValue?: string;
  onChange?: (file: File) => void;
  placeHolder?: string | boolean;
  className?: string;
}

const resolvePreviewUrl = (src: string | null | undefined): string | null => {
  const resolved = toMediaUrl(src);
  return resolved || null;
};

const Uploader = ({ ...props }: UploaderProps) => {
  const t = useTranslations();
  const {
    value,
    defaultValue,
    onChange,
    placeHolder,
    className = '',
  } = props;
  const resolvedPlaceholder =
    placeHolder === undefined ? t('uploader.placeholder') : placeHolder;
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [backgroundImage, setBackgroundImage] = useState<string | null>(
    resolvePreviewUrl(defaultValue),
  );
  const [previewFailed, setPreviewFailed] = useState(false);

  const setPreview = (src: string | null) => {
    setPreviewFailed(false);
    setBackgroundImage(src);
  };

  const handleFile = (file: File) => {
    const url = URL.createObjectURL(file);
    setPreview(url);
    onChange?.(file);
  };

  const handleDrop = (e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    const droppedFiles = Array.from(e.dataTransfer.files).filter((file) =>
      file.type.startsWith('image/'),
    );
    if (droppedFiles.length > 0) handleFile(droppedFiles[0]);
  };

  const handleFileChange = (e: ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    const files = Array.from(e.target.files).filter((file) =>
      file.type.startsWith('image/'),
    );
    if (files.length > 0) handleFile(files[0]);
  };

  const handleDragOver = (e: DragEvent<HTMLDivElement>) => e.preventDefault();

  useEffect(() => {
    if (typeof value === 'string') {
      setPreview(resolvePreviewUrl(value));
    } else if (value instanceof File) {
      const url = URL.createObjectURL(value);
      setPreview(url);
      return () => URL.revokeObjectURL(url);
    } else if (value == null && defaultValue) {
      setPreview(resolvePreviewUrl(defaultValue));
    }
  }, [value, defaultValue]);

  const showPreview = Boolean(backgroundImage) && !previewFailed;

  return (
    <label
      className={cn(
        'relative flex flex-col justify-center items-center w-full min-h-36 h-36',
        'border border-dashed border-gray-300 bg-white rounded-lg',
        'text-gray-400 text-sm overflow-hidden cursor-pointer',
        className,
      )}
      style={{
        backgroundImage: showPreview
          ? `url("${backgroundImage}")`
          : undefined,
        backgroundSize: 'cover',
        backgroundPosition: 'center',
      }}
    >
      {/* Probe remote/local preview load; CSS background cannot report 404. */}
      {backgroundImage && !previewFailed && (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={backgroundImage}
          alt=""
          aria-hidden
          className="absolute opacity-0 pointer-events-none w-0 h-0"
          onError={() => setPreviewFailed(true)}
        />
      )}

      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        className="hidden"
        accept="image/*"
      />

      <div
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        className="flex justify-center items-center w-full h-full"
      >
        {!showPreview && resolvedPlaceholder && (
          <div className="flex flex-col justify-center items-center gap-1 p-3 w-full h-full text-center">
            <span>{resolvedPlaceholder}</span>
            {previewFailed && (
              <span className="text-xs text-amber-600">
                {t('uploader.previousNotFound')}
              </span>
            )}
          </div>
        )}
      </div>
    </label>
  );
};

export default Uploader;
