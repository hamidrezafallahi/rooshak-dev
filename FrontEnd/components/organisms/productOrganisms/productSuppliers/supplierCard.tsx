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
  return (
    <div className="group relative bg-white hover:shadow-xl p-6 border border-gray-100 hover:border-gray-200 rounded-2xl transition-all duration-300">
      {/* بخش بالای کارت - هدر */}
      <div className="flex justify-between items-start mb-4">
        <Link href={`/${locale}/suppliers/${supplier.supplierSlug || supplier.supplierId}`} className="relative">
          <div className="relative w-16 h-16 group-hover:scale-110 transition-transform duration-300">
            <MediaImage
              alt={supplier.supplierName}
              src={supplier.supplierImage}
              width={64}
              height={64}
              className="rounded-xl ring-4 ring-gray-50 group-hover:ring-primary/10 w-16 h-16 object-cover"
            />
          </div>
        </Link>

        {/* نشان موجودی */}
        <div
          className={`px-3 py-1 rounded-full text-xs font-medium ${
            supplier.inventory > 0
              ? "bg-green-50 text-green-700 border border-green-200"
              : "bg-red-50 text-red-700 border border-red-200"
          }`}
        >
          {supplier.inventory > 0 ? t('common.inStock') : t('common.outOfStock')}
        </div>
      </div>

      {/* اطلاعات تأمین‌کننده */}
      <Link
        href={`/${locale}/suppliers/${supplier.supplierSlug || supplier.supplierId}`}
        className="block mb-3 group-hover:text-primary transition-colors"
      >
        <h4 className="font-bold text-gray-800 text-lg line-clamp-1">
          {supplier.supplierName}
        </h4>
      </Link>

      {supplier.supplierDesc && (
        <p className="mb-4 text-gray-500 text-xs line-clamp-2 leading-5">
          {supplier.supplierDesc}
        </p>
      )}

      {/* قیمت و دکمه خرید */}
      <div className="flex justify-between items-center mt-auto pt-4 border-gray-100 border-t">
        <div className="flex flex-col">
          <span className="text-gray-500 text-xs">{t('common.price')}</span>
          <span className="font-bold text-gray-800 text-lg">
            {supplier.finalPrice.toLocaleString("fa-IR")}
          </span>
          <span className="text-gray-400 text-xs">{t('common.currency')}</span>
        </div>

        <ProductCTA id={supplier.id}  productId={productId} />
      </div>

      {/* تخفیف در صورت وجود */}
      {supplier.activeDiscounts?.length > 0 && (
        <div className="top-4 right-4 absolute bg-gradient-to-r from-red-500 to-pink-500 shadow-lg px-3 py-1 rounded-full font-medium text-white text-xs">
          {t('common.specialDiscount')}
        </div>
      )}
    </div>
  );
}
