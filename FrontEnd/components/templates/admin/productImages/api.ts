import { browserApiBaseUrl } from '@/lib/api';
import {
  ApiResult,
  describeThrown,
  readApiResult,
} from '@/lib/apiError';
import { fetchWithAuth } from '@/lib/client-auth';
import { revalidateEntity } from '@lib/revalidateEntity';

export const PRODUCT_IMAGES_ENDPOINT = 'productImages';

export interface GalleryImage {
  id: number;
  productId: number;
  productName?: string | null;
  productImageUrl: string;
  isMain: boolean;
  isActive: boolean;
}

async function send<T = unknown>(path: string, init: RequestInit = {}): Promise<ApiResult<T>> {
  try {
    const res = await fetchWithAuth(`${browserApiBaseUrl}/${PRODUCT_IMAGES_ENDPOINT}${path}`, {
      ...init,
      cache: 'no-store',
    });
    return await readApiResult<T>(res);
  } catch (err) {
    return { isSuccess: false, data: null as T, error: describeThrown(err) };
  }
}

/** The whole gallery of one product, main image first. */
export const getProductGallery = (productId: number) =>
  send<GalleryImage[]>(`/product/${productId}`);

/** Adds one image next to the existing ones; the backend makes it main only when the product has none. */
export function addProductImage(productId: number, file: File) {
  const form = new FormData();
  form.append('ProductId', String(productId));
  form.append('Images', file);
  form.append('IsMainImages', 'false');
  return send('', { method: 'POST', body: form });
}

/** Swaps the file of one image; its main and visibility state stay. */
export function replaceProductImage(imageId: number, file: File) {
  const form = new FormData();
  form.append('Image', file);
  return send(`/${imageId}/file`, { method: 'PUT', body: form });
}

export const setMainProductImage = (imageId: number) =>
  send(`/${imageId}/main`, { method: 'PUT' });

export const setProductImageActive = (imageId: number, isActive: boolean) =>
  send('/active', {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id: imageId, isActive }),
  });

export const deleteProductImage = (imageId: number) =>
  send(`/${imageId}`, { method: 'DELETE' });

/** Best effort: the storefront also refreshes on its own revalidate window. */
export async function refreshStorefrontImages() {
  try {
    await Promise.all([
      revalidateEntity(PRODUCT_IMAGES_ENDPOINT),
      revalidateEntity('products'),
    ]);
  } catch {
    /* the save itself already succeeded */
  }
}
