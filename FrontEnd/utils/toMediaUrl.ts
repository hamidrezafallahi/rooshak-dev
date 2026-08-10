/**
 * Backend stores/returns media paths like `uploads/products/1/x.webp` (no leading slash).
 * Seeds may include a leading slash: `/uploads/categories/foo.jpg`.
 *
 * Relative to nested routes (e.g. /fa/products) that becomes /fa/uploads/... → 404.
 * Without a leading slash, next/image also emits an invalid optimizer URL:
 *   /_next/image?url=uploads%2F...  →  "url" parameter is invalid
 *
 * Always expose a root-absolute path for <img> / next/image.
 * For /uploads/* prefer MediaImage (unoptimized) so nginx serves the file directly;
 * the Next optimizer cannot read the uploads volume inside the frontend container
 * without a slow fetch hop that hurts landing LCP.
 *
 * Folder map: see utils/uploadPaths.ts (mirrors Backend UploadPaths).
 */
export function toMediaUrl(path: string | null | undefined): string {
  if (!path) {
    return '';
  }

  const trimmed = path.trim();
  if (!trimmed) {
    return '';
  }

  if (
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('data:') ||
    trimmed.startsWith('blob:')
  ) {
    return trimmed;
  }

  return trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
}

export function isUploadMediaPath(path: string | null | undefined): boolean {
  if (!path) {
    return false;
  }

  const normalized = path.trim().toLowerCase();
  return (
    normalized.includes('/uploads/') ||
    normalized.startsWith('uploads/') ||
    /https?:\/\/[^/]+\/uploads\//i.test(normalized)
  );
}
