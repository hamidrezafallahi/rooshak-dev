import 'server-only';

import { notFound } from 'next/navigation';

import { SimpleResponse } from '@models/base';
import { IBlog } from '@models/Blog';

import { requireAbsoluteUrl, serverApiBaseUrl } from './api';
import { logger } from './logger';

/**
 * Calls notFound() when the post does not exist (HTTP 400/404 or `isSuccess: false`).
 * Throws when the API is unavailable, so an outage is a 5xx and never a 404.
 */
export async function getBlogBySlug({ params }: { params: { slug: string } }) {
  const slug = params.slug;
  const url = requireAbsoluteUrl(
    `${serverApiBaseUrl}/Blogs/${slug}`,
    'getBlogBySlug URL',
  );

  let res: Response;
  try {
    res = await fetch(url, { next: { revalidate: 60, tags: ['blogs'] } });
  } catch (err) {
    logger.error('getBlogBySlug failed', { scope: 'blog', source: 'server', url }, err);
    throw err;
  }

  if (res.status === 400 || res.status === 404) {
    notFound();
  }
  if (!res.ok) {
    logger.error('getBlogBySlug HTTP error', {
      scope: 'blog',
      source: 'server',
      url,
      status: res.status,
    });
    throw new Error(`Blogs/${slug}: API unavailable (status ${res.status})`);
  }

  const response: SimpleResponse<IBlog> = await res.json();
  if (!response.isSuccess || !response.data) {
    logger.warn('getBlogBySlug business error', {
      scope: 'blog',
      source: 'server',
      url,
      errorMessage: response.error,
    });
    notFound();
  }

  return response.data;
}
