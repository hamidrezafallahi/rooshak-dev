/**
 * Server-side JSON fetch that never throws on bad HTML/network.
 * Used by SSG/ISR pages so one flaky API response cannot fail the whole build.
 */
export async function safeFetchJson<T>(
  url: string,
  init?: RequestInit & { next?: { revalidate?: number | false; tags?: string[] } },
  retries = 2,
): Promise<{ ok: true; status: number; data: T } | { ok: false; status: number; data: null }> {
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
        return { ok: false, status: response.status, data: null };
      }

      let data: T;
      try {
        data = JSON.parse(raw) as T;
      } catch {
        if (attempt < retries) {
          await new Promise((r) => setTimeout(r, 250 * (attempt + 1)));
          continue;
        }
        return { ok: false, status: response.status, data: null };
      }

      if (!response.ok) {
        if (attempt < retries && (response.status === 429 || response.status >= 500)) {
          await new Promise((r) => setTimeout(r, 400 * (attempt + 1)));
          continue;
        }
        return { ok: false, status: response.status, data: null };
      }

      return { ok: true, status: response.status, data };
    } catch (error) {
      if (attempt < retries) {
        await new Promise((r) => setTimeout(r, 250 * (attempt + 1)));
        continue;
      }
      console.error(`safeFetchJson failed for ${url}:`, error);
      return { ok: false, status: 0, data: null };
    }
  }

  return { ok: false, status: 0, data: null };
}
