import Image, { type ImageProps } from 'next/image';

import {
  isUploadMediaPath,
  toMediaUrl,
} from '@utils/toMediaUrl';

/** Neutral placeholder in public/images. SVG, so next/image serves it as-is. */
export const MEDIA_PLACEHOLDER = '/images/placeholder.svg';

type MediaImageProps = Omit<ImageProps, 'src'> & {
  src: string | null | undefined;
  fallbackSrc?: string;
  /**
   * Route uploads through `/_next/image` so `sizes` picks a resized variant.
   * Use for small, below-the-fold images (product cards); keep it off for the LCP hero.
   */
  optimize?: boolean;
};

/**
 * next/image for backend media.
 *
 * Uploads live on the nginx/backend volume, not inside the frontend container.
 * Always use a root-absolute `/uploads/...` path (see toMediaUrl) so locale
 * routes like `/fa/blog/...` do not resolve media as `/fa/uploads/...`.
 *
 * Uploads are already WebP from the API, so by default they are served directly
 * (unoptimized). Routing them through `/_next/image` adds cold-start latency and
 * hurts LCP on the landing page more than the byte savings help. Pass `optimize`
 * for card images, where the originals are 2–4x larger than the display size.
 */
export default function MediaImage({
  src,
  fallbackSrc = MEDIA_PLACEHOLDER,
  alt,
  unoptimized,
  optimize = false,
  sizes,
  fill,
  ...props
}: MediaImageProps) {
  const resolved = toMediaUrl(src) || fallbackSrc;
  const isUpload = isUploadMediaPath(resolved);

  return (
    <Image
      {...props}
      src={resolved}
      alt={alt}
      fill={fill}
      sizes={sizes ?? (fill ? '(max-width: 768px) 100vw, 33vw' : undefined)}
      unoptimized={unoptimized ?? (isUpload && !optimize)}
    />
  );
}
