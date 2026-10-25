import { browserApiBaseUrl } from '@/lib/api';
import {
  fetchWithAuth,
  forbiddenMessage,
} from '@/lib/client-auth';
import { revalidateEntity } from '@lib/revalidateEntity';

export const THEME_ENDPOINT = 'themeSettings';

type Result = { isSuccess: boolean; error?: string | null };

async function send(path: string, init: RequestInit): Promise<Result> {
  try {
    const res = await fetchWithAuth(`${browserApiBaseUrl}/${path}`, {
      ...init,
      cache: 'no-store',
    });
    if (res.status === 403) return { isSuccess: false, error: forbiddenMessage() };
    const json = (await res.json().catch(() => null)) as Result | null;
    if (!res.ok || !json?.isSuccess) {
      return { isSuccess: false, error: json?.error || `HTTP ${res.status}` };
    }
    return { isSuccess: true };
  } catch (err) {
    return { isSuccess: false, error: err instanceof Error ? err.message : null };
  }
}

/** Makes this theme the live one; the backend deactivates every other theme. */
export const activateTheme = (id: number) =>
  send(`${THEME_ENDPOINT}/active`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ id, isActive: true }),
  });

export const deleteTheme = (id: number) =>
  send(`${THEME_ENDPOINT}/${id}`, { method: 'DELETE' });

/**
 * Busts the `themeSettings` fetch cache so the storefront picks up the new colours
 * now instead of after its 60s revalidate. Best effort: a failed hop must never
 * surface as an error, the save itself already succeeded.
 */
export async function refreshStorefrontTheme() {
  try {
    await revalidateEntity(THEME_ENDPOINT);
  } catch {
    /* the storefront still refreshes on its own within a minute */
  }
}
