import 'server-only';

import { notFound } from 'next/navigation';

import { IBlog } from '@models/Blog';

import { requireAbsoluteUrl, serverApiBaseUrl } from './api';
import { ApiRequestError, describeStatus, readApiResult } from './apiError';
import { logger } from './logger';

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
    // A network failure is a server error, not a missing article.
    throw new ApiRequestError(describeStatus(503));
  }

  const response = await readApiResult<IBlog>(res);

  // notFound() must run outside try/catch: it works by throwing.
  if (res.status === 404 || (res.ok && !response.data)) {
    logger.warn('getBlogBySlug not found', {
      scope: 'blog',
      source: 'server',
      url,
      errorMessage: response.error,
    });
    notFound();
  }

  if (!response.isSuccess) {
    logger.error('getBlogBySlug HTTP error', {
      scope: 'blog',
      source: 'server',
      url,
      status: res.status,
    });
    throw new ApiRequestError(response.error ?? describeStatus(res.status));
  }

  return response.data;
}
