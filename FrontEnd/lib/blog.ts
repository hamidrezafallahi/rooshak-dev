import 'server-only';

import { notFound } from 'next/navigation';

import { SimpleResponse } from '@models/base';
import { IBlog } from '@models/Blog';

import { requireAbsoluteUrl, serverApiBaseUrl } from './api';
import { logger } from './logger';

export async function getBlogBySlug({ params }: { params: { slug: string } }) {
  const slug = params.slug;
  const url = requireAbsoluteUrl(
    `${serverApiBaseUrl}/Blogs/${slug}`,
    'getBlogBySlug URL',
  );

  try {
    const res = await fetch(url, { next: { revalidate: 60, tags: ['blogs'] } });
    if (!res.ok) {
      logger.error('getBlogBySlug HTTP error', {
        scope: 'blog',
        source: 'server',
        url,
        status: res.status,
      });
      notFound();
    }

    const response: SimpleResponse<IBlog> = await res.json();
    if (!response.isSuccess) {
      logger.warn('getBlogBySlug business error', {
        scope: 'blog',
        source: 'server',
        url,
        errorMessage: response.error,
      });
      notFound();
    }

    return response.data;
  } catch (err) {
    logger.error('getBlogBySlug failed', { scope: 'blog', source: 'server', url }, err);
    notFound();
  }
}
