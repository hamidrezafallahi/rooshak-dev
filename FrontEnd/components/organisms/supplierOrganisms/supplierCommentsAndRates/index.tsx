import React from 'react';

import { getTranslations } from 'next-intl/server';

import MediaImage from '@components/atoms/MediaImage';
import { StarIcon } from '@components/atoms/iconComponents';
import { serverApiBaseUrl } from '@lib/api';
import {
  EnumTargetType,
  IComment,
} from '@models/comment';

import CommentAndRate from './CommentAndRate';

export async function SupplierCommentsAndRates(props: {
  params: { slug: string; locale: string };
}) {
  const { params } = props;
  const slug = await params.slug;
  const t = await getTranslations();

  const response = await fetch(
    `${serverApiBaseUrl}/Comments/${EnumTargetType.Supplier}/${slug}`,{next: { revalidate: 36 }});
  if (!response.ok) return <div>{t('common.supplierNotFound')}</div>;

  const { data }: { data: IComment[] } = await response.json();
  return (
    <>
      <div className="gap-6 grid sm:grid-cols-3 mb-16">
        {data.map((comment) => (
          <div
            key={comment.id}
            className="bg-gray-50 hover:shadow-md p-6 rounded-2xl text-right transition"
          >
            <div className="flex items-center mb-3">
              <MediaImage
                src={comment.userImage}
                alt={comment.userFullName}
                width={50}
                height={50}
                className="rounded-full w-14 h-14"
              />
              <div className="mr-3">
                <h4 className="font-semibold">{comment.userFullName}</h4>
                <div className="flex text-yellow-500">
                  {Array.from({ length:comment.userRate}).map((_, i) => (
                    <StarIcon key={i} />
                  ))}
                </div>
              </div>
            </div>
            <p className="text-gray-600 text-sm leading-relaxed">
              {comment.content}
            </p>
              <time className="text-gray-400 text-xs">
            {new Date(comment.createdAt).toLocaleDateString()}
          </time>
          </div>
        ))}
      </div>
      <CommentAndRate TargetType={EnumTargetType.Supplier} TargetId={slug}/>
    </>
  );
}
