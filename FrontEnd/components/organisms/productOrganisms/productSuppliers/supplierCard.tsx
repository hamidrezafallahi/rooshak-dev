import { getTranslations } from 'next-intl/server';
import Link from 'next/link';

import MediaImage from '@components/atoms/MediaImage';

import ProductCTA from './productCTA';

export interface ISupplier {
  id: number;
  productId: number;
  productName: string;
  supplierId: number;
  supplierSlug?: string | null;
  supplierName: string;
  supplierImage: string;
  supplierDesc: null | string;
  basePrice: number;
  finalPrice: number;
  inventory: number;
  isActive: true;
  createdAt: string;
  activeDiscounts: [];
}

export async function SupplierCardGrid({
  supplier,
  productId,
  locale,
}: {
  supplier: ISupplier;
  productId: number;
  locale: string;
}) {
  const t = await getTranslations();
  const inStock = supplier.inventory > 0;
  const hasDiscount = Array.isArray(supplier.activeDiscounts) && supplier.activeDiscounts.length > 0;
  const profileHref = `/${locale}/suppliers/${supplier.supplierSlug || supplier.supplierId}`;

  return (
    <article className="group relative flex flex-col bg-store-surface border border-store-border hover:border-store-strong h-full overflow-hidden transition-colors duration-300">
      {hasDiscount && (
        <div className="top-3 start-3 z-10 absolute bg-primary px-2 py-1 font-medium text-primary-foreground text-xs">
          {t('common.specialDiscount')}
        </div>
      )}
      {!inStock && (
        <div className="top-3 end-3 z-10 absolute bg-store-muted px-2 py-1 font-medium text-store-text text-xs">
          {t('common.outOfStock')}
        </div>
      )}

      <Link
        href={profileHref}
        className="block relative bg-store-muted h-48 overflow-hidden"
      >
        <MediaImage
          alt={supplier.supplierName}
          src={supplier.supplierImage}
          fill
          className="object-cover group-hover:scale-105 transition-transform duration-300"
          sizes="(max-width: 640px) 100vw, (max-width: 768px) 50vw, (max-width: 1024px) 33vw, 25vw"
        />
      </Link>

      <div className="flex flex-col flex-1 p-4 min-w-0">
        <Link
          href={profileHref}
          className="block mb-2 group-hover:underline underline-offset-4"
        >
          <h4 className="font-normal text-sm sm:text-base line-clamp-1">
            {supplier.supplierName}
          </h4>
        </Link>

        {supplier.supplierDesc ? (
          <p className="mb-3 text-store-subtle text-xs line-clamp-2 leading-5">
            {supplier.supplierDesc}
          </p>
        ) : null}

        {inStock ? (
          <span className="mb-3 bg-store-muted px-2 py-1 w-fit text-store-subtle text-xs">
            {t('common.inStock')}
          </span>
        ) : null}

        <div className="flex flex-wrap items-baseline gap-1 mt-auto mb-3">
          <span className="font-medium text-lg">
            {supplier.finalPrice.toLocaleString('fa-IR')}
          </span>
          <span className="text-store-subtle text-sm">{t('common.currency')}</span>
        </div>

        <ProductCTA id={supplier.id} productId={productId} />
      </div>
    </article>
  );
}
