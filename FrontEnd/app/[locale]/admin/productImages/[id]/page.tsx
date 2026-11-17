import { redirect } from 'next/navigation';

import { getById } from '@lib/getAll';

export const dynamic = 'force-dynamic';

/** Editing one image opens its product's gallery, where every image can be changed. */
export default async function EditProductImagePage({
  params,
}: {
  params: Promise<{ locale: string; id: string }>;
}) {
  const { locale, id } = await params;
  let productId: number | undefined;
  if (/^\d+$/.test(id)) {
    try {
      const image = await getById('productImages', id);
      productId = Number(image?.productId) || undefined;
    } catch {
      productId = undefined;
    }
  }
  redirect(`/${locale}/admin/productImages${productId ? `?productId=${productId}` : ''}`);
}
