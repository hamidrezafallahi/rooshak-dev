import React from 'react';

import ProductImageStudio from '@components/templates/admin/productImages';

export const dynamic = 'force-dynamic';

/**
 * Dedicated page instead of the generic [field] table: a product's images are managed
 * together in one gallery (`?productId=<id>` opens that product's gallery).
 */
export default async function ProductImagesPage({
  searchParams,
}: {
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const raw = (await searchParams)?.productId;
  const productId = typeof raw === 'string' && /^\d+$/.test(raw) ? Number(raw) : undefined;
  return <ProductImageStudio initialProductId={productId} />;
}
