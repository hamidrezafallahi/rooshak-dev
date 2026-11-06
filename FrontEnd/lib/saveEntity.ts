import { browserApiBaseUrl } from '@/lib/api';
import {
  ApiResult,
  describeThrown,
  readApiResult,
} from '@/lib/apiError';
import { fetchWithAuth } from '@/lib/client-auth';

export type { FieldError } from '@/lib/apiError';
export type SaveEntityResponse = ApiResult<any>;

/**
 * Persist an admin entity.
 *
 * File uploads must NOT go through Next.js Server Actions: the browser POSTs the
 * whole multipart body to the current page URL, which commonly ends as
 * ERR_CONNECTION_CLOSED under nginx / body-size limits.
 * Instead we POST/PUT via the cookie-aware BFF at /auth/bff/*.
 *
 * Cache busting is done by the caller via router.refresh() after navigation —
 * calling a Server Action here previously surfaced "Failed to fetch" into the
 * locale error boundary when the action hop failed.
 */
export async function saveEntity({
  endPoint,
  body,
  method,
}: {
  endPoint: string;
  body: FormData | Record<string, unknown>;
  method: 'POST' | 'PUT';
}): Promise<SaveEntityResponse> {
  try {
    const path = endPoint.replace(/^\/+/, '');
    const isFormData =
      typeof FormData !== 'undefined' && body instanceof FormData;

    // 401 → refresh + retry, or the login page when the session is over.
    const response = await fetchWithAuth(`${browserApiBaseUrl}/${path}`, {
      method,
      body: isFormData ? body : JSON.stringify(body),
      headers: isFormData
        ? undefined
        : { 'Content-Type': 'application/json' },
      cache: 'no-store',
    });

    return await readApiResult(response);
  } catch (error) {
    return { data: null, isSuccess: false, error: describeThrown(error) };
  }
}
