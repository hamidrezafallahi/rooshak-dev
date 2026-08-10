import Image, { type ImageProps } from 'next/image';

import { siteBaseUrl } from '@lib/api';
import {
  isUploadMediaPath,
  toMediaUrl,
} from '@utils/toMediaUrl';

type MediaImageProps = Omit<ImageProps, 'src'> & {
  src: string | null | undefined;
  fallbackSrc?: string;
};

/**
 * Prefer an origin the Next image optimizer can fetch.
 * Uploads are not on the frontend disk — only nginx/backend serve /uploads.
 * Server-side: use the Docker-internal origin when available.
 * Client-side: use the public site URL (remotePatterns allow it).
 */
function uploadsFetchOrigin(): string {
  if (typeof window === 'undefined') {
    const internal = (
      process.env.INTERNAL_UPLOADS_ORIGIN?.trim() ||
      process.env.INTERNAL_SERVER_SIDE_API_URL?.trim() ||
      process.env.NEXT_PUBLIC_INTERNAL_API_URL?.trim() ||
      ''
    )
      .replace(/\/+$/, '')
      .replace(/\/api$/i, '');

    if (internal) return internal;
  }

  return siteBaseUrl.replace(/\/+$/, '');
}

function toOptimizableSrc(path: string): string {
  if (
    path.startsWith('http://') ||
    path.startsWith('https://') ||
    path.startsWith('data:') ||
    path.startsWith('blob:')
  ) {
    return path;
  }

  if (!isUploadMediaPath(path)) {
    return path;
  }

  return `${uploadsFetchOrigin()}${path.startsWith('/') ? path : `/${path}`}`;
}

/**
 * next/image for backend media with responsive optimization.
 *
 * Uploads are rewritten to an absolute URL so the optimizer can fetch them
 * (they are not inside the frontend container filesystem).
 */
export default function MediaImage({
  src,
  fallbackSrc = '/images/user-placeholder.png',
  alt,
  unoptimized = false,
  quality = 70,
  sizes,
  fill,
  ...props
}: MediaImageProps) {
  const resolved = toMediaUrl(src) || fallbackSrc;
  const optimizableSrc = toOptimizableSrc(resolved);

  return (
    <Image
      {...props}
      src={optimizableSrc}
      alt={alt}
      fill={fill}
      quality={quality}
      sizes={sizes ?? (fill ? '(max-width: 768px) 100vw, 33vw' : undefined)}
      unoptimized={unoptimized}
    />
  );
}
