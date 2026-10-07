import React from 'react';

import {
  getLocale,
  getTranslations,
} from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { IUser } from '@models/user';

export async function SimpleSupplierCard({
  supplier,
}: {
  supplier: IUser;
}) {
  const locale = await getLocale();
  const t = await getTranslations();
  return (
    <Link
      className="group relative flex justify-between items-start gap-4 bg-store-surface mb-4 p-6 border border-store-border hover:border-store-strong transition-colors duration-300"
      href={`/${locale}/suppliers/${supplier.slug || supplier.id}`}
    >
      <div className="relative w-16 h-16 group-hover:scale-110 transition-transform duration-300">
        <MediaImage
          alt={supplier.fullName}
          src={supplier.image}
          width={64}
          height={64}
          className="w-16 h-16 object-cover"
        />
      </div>
      <div className='text-center'>
        <div className="font-medium">
          <div>{t("register.fullName")}:</div> 
          <div> {supplier.fullName}</div> 
        </div>
 
        {supplier.userDescription &&<div className="mb-4 text-store-subtle text-xs line-clamp-2 leading-5">
          {t("register.userDescription")}:{supplier.userDescription}
        </div>}
       {supplier.email&& <div className="mb-4 text-store-subtle text-xs line-clamp-2 leading-5">
         {t("register.email")} : {supplier.email}
        </div>}
        {supplier.phoneNumber && <div className="mb-4 text-store-subtle text-xs line-clamp-2 leading-5">
          {t("register.phoneNumber")} : {supplier.phoneNumber}
        </div>}
      </div>
    </Link>
  );
}
