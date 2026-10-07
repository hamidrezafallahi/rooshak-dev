"use client";
import React from 'react';

import { useLocale, useTranslations } from 'next-intl';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';
import { IUser } from '@models/user';

export default function SupplierCard({
  supplier,
}: {
  supplier: IUser;
}) {
  const locale = useLocale();
  const t = useTranslations('common');

  return (
    <Link
      href={`/${locale}/suppliers/${supplier.slug || supplier.id}`}
      className="group relative flex flex-col bg-store-surface border border-store-border hover:border-store-strong overflow-hidden transition-colors duration-300"
    >
      {/* Image Section */}
      <div className="relative flex justify-center items-center bg-store-muted w-full h-48">
        <MediaImage
          src={supplier.image}
          alt={supplier.fullName}
          width={400}
          height={400}
          className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
        />
      </div>

      {/* Content Section */}
      <div className="space-y-2 p-4 sm:p-5">
        <div>
          <h3 className="font-normal text-lg">
            {supplier.fullName}
          </h3>
          <p className="text-store-subtle text-xs">
            {supplier.email}
          </p>
        </div>

        <div className="space-y-1 text-store-subtle text-xs">
          <p>📞 {supplier.phoneNumber || '—'}</p>
          <p>
            {supplier.role
              ? t('roleLabel', { role: supplier.role })
              : t('supplier')}
          </p>
        </div>

        {supplier.userDescription && (
          <p className="text-store-subtle text-xs line-clamp-2">
            {supplier.userDescription}
          </p>
        )}

        <span className="inline-block pt-2 pb-0.5 border-b border-current font-medium text-sm">
          {t('viewSupplier')}
        </span>
      </div>
    </Link>
  );
}