import React from 'react';

import { getTranslations } from 'next-intl/server';

import MediaImage from '@components/atoms/MediaImage';
import { Rate } from '@components/atoms/defaultElements/customRate';
import { IUser } from '@models/user';

export async function SupplierProfile({ ...props }: { user: IUser }) {
  const { user } = props;
  const t = await getTranslations();
  return (
    <div className="group relative shadow-sm rounded-2xl w-fit h-44 md:h-52 overflow-hidden">
      <MediaImage
        src={user.userImage}
        alt={user.fullName}
        height={500}
        width={300}
        className="object-cover group-hover:scale-105 transition-transform transform"
        loading="lazy"
      />
      <div className="absolute inset-0 flex flex-col justify-end bg-gradient-to-t from-black/40 to-transparent p-4">
        <div className="flex justify-between items-center w-full">
          <h3 className="font-semibold text-white">{user.fullName} </h3>
          <div className="flex justify-center items-center">
            <Rate value={user.averageRate} />
            <h3 className="font-semibold text-white text-xs">
              {t('common.votesCount', { count: user.rateCount })}
            </h3>
          </div>
        </div>
        <div className="text-gray-200 text-xs">{user.userDescription}</div>
      </div>
    </div>
  );
}
