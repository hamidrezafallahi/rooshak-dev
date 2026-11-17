import { redirect } from 'next/navigation';

export const dynamic = 'force-dynamic';

/** Adding images happens inside the gallery; old links to the "new" form land there. */
export default async function NewProductImagePage({
  params,
  searchParams,
}: {
  params: Promise<{ locale: string }>;
  searchParams?: Promise<{ [key: string]: string | string[] | undefined }>;
}) {
  const { locale } = await params;
  const productId = (await searchParams)?.productId;
  const query = typeof productId === 'string' && /^\d+$/.test(productId) ? `?productId=${productId}` : '';
  redirect(`/${locale}/admin/productImages${query}`);
}
