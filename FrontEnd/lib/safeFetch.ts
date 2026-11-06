import { describeStatus, errorTextOf } from './apiError';

export type SafeFetchResult<T> =
  | { ok: true; status: number; data: T; error?: undefined }
  | { ok: false; status: number; data: null; error: string };

/**
 * Server-side JSON fetch that never throws on bad HTML/network.
 * Used by SSG/ISR pages so one flaky API response cannot fail the whole build.
 * A failure carries in `error` the backend's explanation, or a description of what happened.
 */
export async function safeFetchJson<T>(
  url: string,
  init?: RequestInit & { next?: { revalidate?: number | false; tags?: string[] } },
  retries = 2,
): Promise<SafeFetchResult<T>> {
  for (let attempt = 0; attempt <= retries; attempt += 1) {
    try {
      const response = await fetch(url, init);
      const contentType = response.headers.get('content-type') || '';
      const raw = await response.text();

      // nginx rate-limit / proxy errors often return HTML with 429/502.
      if (
        !contentType.includes('application/json') ||
        raw.trimStart().startsWith('<')
      ) {
        if (attempt < retries) {
          await new Promise((r) => setTimeout(r, 250 * (attempt + 1)));
          continue;
        }
        return {
          ok: false,
          status: response.status,
          data: null,
          error: describeStatus(response.ok ? 'PARSING_ERROR' : response.status),
        };
      }

      let data: T;
      try {
        data = JSON.parse(raw) as T;
      } catch {
        if (attempt < retries) {
          await new Promise((r) => setTimeout(r, 250 * (attempt + 1)));
          continue;
        }
        return {
          ok: false,
          status: response.status,
          data: null,
          error: describeStatus(response.ok ? 'PARSING_ERROR' : response.status),
        };
      }

      if (!response.ok) {
        if (attempt < retries && (response.status === 429 || response.status >= 500)) {
          await new Promise((r) => setTimeout(r, 400 * (attempt + 1)));
          continue;
        }
        return {
          ok: false,
          status: response.status,
          data: null,
          error: errorTextOf(data) ?? describeStatus(response.status),
        };
      }

      return { ok: true, status: response.status, data };
    } catch (error) {
      if (attempt < retries) {
        await new Promise((r) => setTimeout(r, 250 * (attempt + 1)));
        continue;
      }
      console.error(`safeFetchJson failed for ${url}:`, error);
      return { ok: false, status: 0, data: null, error: describeStatus(503) };
    }
  }

  return { ok: false, status: 0, data: null, error: describeStatus(503) };
}

/** What a page section should say when it could not load; a 404 only means there is nothing to show. */
export function loadFailure<T>(result: SafeFetchResult<T>): string | null {
  return !result.ok && result.status !== 404 ? result.error : null;
}
